namespace ArknightsLite.Editor.LevelEditor.Core {
    using System;
    using System.Collections.Generic;
    using ArknightsLite.Config;
    using ArknightsLite.Model;
    using UnityEngine;

    [Serializable]
    public sealed class LevelEditorWorkspace {
        public string LevelName = string.Empty;
        public string ExportName = string.Empty;
        public int MapWidth = 10;
        public int MapDepth = 10;
        public float CellSize = 1f;
        public TileType DefaultTileType = TileType.Ground;
        public LevelRuntimeParameters Runtime = new LevelRuntimeParameters();
        public string SpawnId = "spawn_01";
        public Vector2Int SpawnPoint = Vector2Int.zero;
        public string GoalId = "goal_01";
        public Vector2Int GoalPoint = new Vector2Int(9, 9);
        public List<TileData> TileOverrides = new List<TileData>();
        public List<PortalDefinition> Portals = new List<PortalDefinition>();
        public List<WaveDefinition> Waves = new List<WaveDefinition>();
        public List<EnemyTemplateData> Enemies = new List<EnemyTemplateData>();
        public List<OperatorTemplateData> Operators = new List<OperatorTemplateData>();

        public static LevelEditorWorkspace CreateNew(string levelName) {
            string resolvedLevelName = string.IsNullOrWhiteSpace(levelName) ? "NewLevel" : levelName;
            return new LevelEditorWorkspace {
                LevelName = resolvedLevelName,
                ExportName = resolvedLevelName,
                Runtime = new LevelRuntimeParameters(),
                SpawnId = "spawn_01",
                SpawnPoint = Vector2Int.zero,
                GoalId = "goal_01",
                GoalPoint = new Vector2Int(9, 9)
            };
        }

        public bool EnsureDefaults() {
            bool changed = false;

            if (string.IsNullOrWhiteSpace(LevelName)) {
                LevelName = "NewLevel";
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(ExportName)) {
                ExportName = LevelName;
                changed = true;
            }

            if (MapWidth <= 0) {
                MapWidth = 10;
                changed = true;
            }

            if (MapDepth <= 0) {
                MapDepth = 10;
                changed = true;
            }

            if (CellSize <= 0f) {
                CellSize = 1f;
                changed = true;
            }

            Runtime ??= new LevelRuntimeParameters();
            TileOverrides ??= new List<TileData>();
            Portals ??= new List<PortalDefinition>();
            Waves ??= new List<WaveDefinition>();
            Enemies ??= new List<EnemyTemplateData>();
            Operators ??= new List<OperatorTemplateData>();

            if (string.IsNullOrWhiteSpace(SpawnId)) {
                SpawnId = "spawn_01";
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(GoalId)) {
                GoalId = "goal_01";
                changed = true;
            }

            Vector2Int clampedSpawn = ClampToBounds(SpawnPoint);
            if (SpawnPoint != clampedSpawn) {
                SpawnPoint = clampedSpawn;
                changed = true;
            }

            Vector2Int clampedGoal = ClampToBounds(GoalPoint);
            if (GoalPoint != clampedGoal) {
                GoalPoint = clampedGoal;
                changed = true;
            }

            return changed;
        }

        public static WaveDefinition CreateDefaultWave(string waveId) {
            return new WaveDefinition {
                waveId = string.IsNullOrWhiteSpace(waveId) ? "wave_01" : waveId,
                time = 0f,
                enemyId = string.Empty,
                count = 1,
                interval = 1f,
                spawnId = "spawn_01",
                targetId = "goal_01",
                path = new List<PathNodeDefinition>()
            };
        }

        public static PortalDefinition CreateDefaultPortal(string portalId) {
            return new PortalDefinition {
                id = string.IsNullOrWhiteSpace(portalId) ? "portal_01" : portalId,
                inPos = Vector2Int.zero,
                outPos = Vector2Int.right,
                delay = 0f,
                color = "#ffffff"
            };
        }

        public TileData GetTileOverride(int x, int z) {
            EnsureDefaults();
            var existing = TileOverrides.Find(tile => tile.x == x && tile.z == z);
            return existing ?? CreateDefaultTile(x, z);
        }

        public void SetTileOverride(int x, int z, TileData tileData) {
            EnsureDefaults();
            var existing = TileOverrides.Find(tile => tile.x == x && tile.z == z);
            var normalized = tileData ?? CreateDefaultTile(x, z);
            normalized.x = x;
            normalized.z = z;

            bool isDefaultTile = normalized.tileType == DefaultTileType && normalized.heightLevel == 0;
            if (isDefaultTile) {
                if (existing != null) {
                    TileOverrides.Remove(existing);
                }

                return;
            }

            if (existing != null) {
                existing.tileType = normalized.tileType;
                existing.heightLevel = normalized.heightLevel;
                existing.walkable = normalized.walkable;
                existing.deployTag = normalized.deployTag;
                return;
            }

            TileOverrides.Add(new TileData {
                x = x,
                z = z,
                tileType = normalized.tileType,
                heightLevel = normalized.heightLevel,
                walkable = normalized.walkable,
                deployTag = normalized.deployTag
            });
        }

        public bool IsTileWalkable(int x, int z) {
            EnsureDefaults();
            if (x < 0 || x >= MapWidth || z < 0 || z >= MapDepth) {
                return false;
            }

            if (IsSpawnPoint(x, z) || IsGoalPoint(x, z)) {
                return true;
            }

            return GetTileOverride(x, z).walkable;
        }

        public void SetSpawnPoint(Vector2Int position) {
            EnsureDefaults();
            SpawnPoint = ClampToBounds(position);
        }

        public void SetGoalPoint(Vector2Int position) {
            EnsureDefaults();
            GoalPoint = ClampToBounds(position);
        }

        public bool IsSpawnPoint(int x, int z) {
            EnsureDefaults();
            Vector2Int point = GetResolvedSpawnPoint();
            return point.x == x && point.y == z;
        }

        public bool IsGoalPoint(int x, int z) {
            EnsureDefaults();
            Vector2Int point = GetResolvedGoalPoint();
            return point.x == x && point.y == z;
        }

        public bool TryResolveSpawn(string spawnId, out Vector2Int position) {
            EnsureDefaults();
            position = GetResolvedSpawnPoint();
            return string.IsNullOrWhiteSpace(spawnId) || string.Equals(spawnId, SpawnId, StringComparison.OrdinalIgnoreCase);
        }

        public bool TryResolveGoal(string goalId, out Vector2Int position) {
            EnsureDefaults();
            position = GetResolvedGoalPoint();
            return string.IsNullOrWhiteSpace(goalId) || string.Equals(goalId, GoalId, StringComparison.OrdinalIgnoreCase);
        }

        public Vector2Int GetResolvedSpawnPoint() {
            EnsureDefaults();
            return ClampToBounds(SpawnPoint);
        }

        public Vector2Int GetResolvedGoalPoint() {
            EnsureDefaults();
            return ClampToBounds(GoalPoint);
        }

        private TileData CreateDefaultTile(int x, int z) {
            return new TileData {
                x = x,
                z = z,
                tileType = DefaultTileType,
                heightLevel = 0,
                walkable = DefaultTileType != TileType.Forbidden && DefaultTileType != TileType.Hole,
                deployTag = "All"
            };
        }

        private Vector2Int ClampToBounds(Vector2Int position) {
            int maxX = Mathf.Max(0, MapWidth - 1);
            int maxZ = Mathf.Max(0, MapDepth - 1);
            return new Vector2Int(
                Mathf.Clamp(position.x, 0, maxX),
                Mathf.Clamp(position.y, 0, maxZ)
            );
        }
    }
}
