namespace ArknightsLite.Editor.LevelEditor.Core {
    using System;
    using System.Collections.Generic;
    using ArknightsLite.Config;
    using ArknightsLite.Model;
    using UnityEngine;

    [Serializable]
    public sealed class LevelEditorWorkspace {
        [Serializable]
        public sealed class SemanticMarker {
            public string Id = string.Empty;
            public Vector2Int Position = Vector2Int.zero;
        }

        [Serializable]
        public sealed class PortalPairDefinition {
            public string PairId = string.Empty;
            public string EntranceId = string.Empty;
            public Vector2Int EntrancePosition = Vector2Int.zero;
            public string ExitId = string.Empty;
            public Vector2Int ExitPosition = Vector2Int.zero;
            public float Delay = 0f;
            public string Color = "#ffffff";
        }

        public string LevelName = string.Empty;
        public string ExportName = string.Empty;
        public int MapWidth = 10;
        public int MapDepth = 10;
        public float CellSize = 1f;
        public TileType DefaultTileType = TileType.Ground;
        public LevelRuntimeParameters Runtime = new LevelRuntimeParameters();

        [SerializeField] private string _legacySpawnId = "spawn_01";
        [SerializeField] private Vector2Int _legacySpawnPoint = Vector2Int.zero;
        [SerializeField] private string _legacyGoalId = "goal_01";
        [SerializeField] private Vector2Int _legacyGoalPoint = new Vector2Int(9, 9);

        public List<SemanticMarker> SpawnMarkers = new List<SemanticMarker>();
        public List<SemanticMarker> GoalMarkers = new List<SemanticMarker>();
        public List<PortalPairDefinition> PortalPairs = new List<PortalPairDefinition>();
        public List<TileData> TileOverrides = new List<TileData>();
        public List<PortalDefinition> Portals = new List<PortalDefinition>();
        public List<WaveDefinition> Waves = new List<WaveDefinition>();
        public List<EnemyTemplateData> Enemies = new List<EnemyTemplateData>();
        public List<OperatorTemplateData> Operators = new List<OperatorTemplateData>();

        public string SpawnId => string.IsNullOrWhiteSpace(_legacySpawnId) ? "spawn_01" : _legacySpawnId;

        public Vector2Int SpawnPoint => _legacySpawnPoint;

        public string GoalId => string.IsNullOrWhiteSpace(_legacyGoalId) ? "goal_01" : _legacyGoalId;

        public Vector2Int GoalPoint => _legacyGoalPoint;

        public static LevelEditorWorkspace CreateNew(string levelName) {
            string resolvedLevelName = string.IsNullOrWhiteSpace(levelName) ? "NewLevel" : levelName;
            return new LevelEditorWorkspace {
                LevelName = resolvedLevelName,
                ExportName = resolvedLevelName,
                Runtime = new LevelRuntimeParameters(),
                _legacySpawnId = "spawn_01",
                _legacySpawnPoint = Vector2Int.zero,
                _legacyGoalId = "goal_01",
                _legacyGoalPoint = new Vector2Int(9, 9)
            };
        }

        public bool EnsureDefaults() {
            bool changed = false;

            if (string.IsNullOrWhiteSpace(LevelName)) {
                LevelName = "NewLevel";
                changed = true;
            }

            if (ExportName == null) {
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

            if (Runtime == null) {
                Runtime = new LevelRuntimeParameters();
                changed = true;
            }

            if (SpawnMarkers == null) {
                SpawnMarkers = new List<SemanticMarker>();
                changed = true;
            }

            if (GoalMarkers == null) {
                GoalMarkers = new List<SemanticMarker>();
                changed = true;
            }

            if (PortalPairs == null) {
                PortalPairs = new List<PortalPairDefinition>();
                changed = true;
            }

            if (TileOverrides == null) {
                TileOverrides = new List<TileData>();
                changed = true;
            }

            if (Portals == null) {
                Portals = new List<PortalDefinition>();
                changed = true;
            }

            if (Waves == null) {
                Waves = new List<WaveDefinition>();
                changed = true;
            }

            if (Enemies == null) {
                Enemies = new List<EnemyTemplateData>();
                changed = true;
            }

            if (Operators == null) {
                Operators = new List<OperatorTemplateData>();
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(_legacySpawnId)) {
                _legacySpawnId = "spawn_01";
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(_legacyGoalId)) {
                _legacyGoalId = "goal_01";
                changed = true;
            }

            Vector2Int clampedSpawn = ClampToBounds(_legacySpawnPoint);
            if (_legacySpawnPoint != clampedSpawn) {
                _legacySpawnPoint = clampedSpawn;
                changed = true;
            }

            Vector2Int clampedGoal = ClampToBounds(_legacyGoalPoint);
            if (_legacyGoalPoint != clampedGoal) {
                _legacyGoalPoint = clampedGoal;
                changed = true;
            }

            changed |= NormalizeMarkers(SpawnMarkers);
            changed |= NormalizeMarkers(GoalMarkers);
            changed |= NormalizePortalPairs();

            if (PortalPairs.Count > 0) {
                SyncLegacyPortalsFromPairs();
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
                spawnId = string.Empty,
                targetId = string.Empty,
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

        public SemanticMarker AddSpawnMarker(Vector2Int position) {
            EnsureDefaults();

            var marker = new SemanticMarker {
                Id = BuildNextMarkerId(SpawnMarkers, "R"),
                Position = ClampToBounds(position)
            };

            SpawnMarkers.Add(marker);
            return marker;
        }

        public SemanticMarker AddGoalMarker(Vector2Int position) {
            EnsureDefaults();

            var marker = new SemanticMarker {
                Id = BuildNextMarkerId(GoalMarkers, "B"),
                Position = ClampToBounds(position)
            };

            GoalMarkers.Add(marker);
            return marker;
        }

        public PortalPairDefinition AddPortalPair(Vector2Int entrancePosition, Vector2Int exitPosition) {
            EnsureDefaults();

            int pairNumber = GetNextPortalPairNumber();
            var pair = new PortalPairDefinition {
                PairId = pairNumber.ToString(),
                EntranceId = $"IN{pairNumber}",
                EntrancePosition = ClampToBounds(entrancePosition),
                ExitId = $"OUT{pairNumber}",
                ExitPosition = ClampToBounds(exitPosition),
                Delay = 0f,
                Color = "#ffffff"
            };

            PortalPairs.Add(pair);
            SyncLegacyPortalsFromPairs();
            return pair;
        }

        public bool IsSpawnMarker(string id, int x, int z) {
            EnsureDefaults();
            return MatchesMarker(SpawnMarkers, id, x, z);
        }

        public bool IsGoalMarker(string id, int x, int z) {
            EnsureDefaults();
            return MatchesMarker(GoalMarkers, id, x, z);
        }

        public List<string> GetSemanticLabelsAt(int x, int z) {
            EnsureDefaults();

            var labels = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AddMarkerLabels(labels, seen, SpawnMarkers, x, z);
            AddMarkerLabels(labels, seen, GoalMarkers, x, z);

            foreach (var pair in PortalPairs) {
                if (pair == null) {
                    continue;
                }

                if (pair.EntrancePosition.x == x && pair.EntrancePosition.y == z) {
                    AddLabel(labels, seen, pair.EntranceId);
                }

                if (pair.ExitPosition.x == x && pair.ExitPosition.y == z) {
                    AddLabel(labels, seen, pair.ExitId);
                }
            }

            return labels;
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

            if (HasAnySemanticMarkerAt(x, z)) {
                return true;
            }

            return GetTileOverride(x, z).walkable;
        }

        public void SetSpawnPoint(Vector2Int position) {
            EnsureDefaults();
            _legacySpawnPoint = ClampToBounds(position);
        }

        public void SetGoalPoint(Vector2Int position) {
            EnsureDefaults();
            _legacyGoalPoint = ClampToBounds(position);
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
            return TryResolveMarker(SpawnMarkers, spawnId, out position);
        }

        public bool TryResolveGoal(string goalId, out Vector2Int position) {
            EnsureDefaults();
            return TryResolveMarker(GoalMarkers, goalId, out position);
        }

        public Vector2Int GetResolvedSpawnPoint() {
            EnsureDefaults();
            return ClampToBounds(_legacySpawnPoint);
        }

        public Vector2Int GetResolvedGoalPoint() {
            EnsureDefaults();
            return ClampToBounds(_legacyGoalPoint);
        }

        private bool NormalizeMarkers(List<SemanticMarker> markers) {
            bool changed = false;
            if (markers == null) {
                return false;
            }

            foreach (var marker in markers) {
                if (marker == null) {
                    continue;
                }

                Vector2Int clampedPosition = ClampToBounds(marker.Position);
                if (marker.Position != clampedPosition) {
                    marker.Position = clampedPosition;
                    changed = true;
                }
            }

            return changed;
        }

        private bool NormalizePortalPairs() {
            bool changed = false;
            if (PortalPairs == null) {
                return false;
            }

            foreach (var pair in PortalPairs) {
                if (pair == null) {
                    continue;
                }

                Vector2Int clampedEntrance = ClampToBounds(pair.EntrancePosition);
                if (pair.EntrancePosition != clampedEntrance) {
                    pair.EntrancePosition = clampedEntrance;
                    changed = true;
                }

                Vector2Int clampedExit = ClampToBounds(pair.ExitPosition);
                if (pair.ExitPosition != clampedExit) {
                    pair.ExitPosition = clampedExit;
                    changed = true;
                }
            }

            return changed;
        }

        private void SyncLegacyPortalsFromPairs() {
            Portals.Clear();
            foreach (var pair in PortalPairs) {
                if (pair == null) {
                    continue;
                }

                Portals.Add(new PortalDefinition {
                    id = string.IsNullOrWhiteSpace(pair.PairId) ? pair.EntranceId : pair.PairId,
                    inPos = pair.EntrancePosition,
                    outPos = pair.ExitPosition,
                    delay = pair.Delay,
                    color = pair.Color
                });
            }
        }

        private static void AddMarkerLabels(List<string> labels, HashSet<string> seen, List<SemanticMarker> markers, int x, int z) {
            if (markers == null) {
                return;
            }

            foreach (var marker in markers) {
                if (marker == null || marker.Position.x != x || marker.Position.y != z) {
                    continue;
                }

                AddLabel(labels, seen, marker.Id);
            }
        }

        private static void AddLabel(List<string> labels, HashSet<string> seen, string label) {
            if (string.IsNullOrWhiteSpace(label) || !seen.Add(label)) {
                return;
            }

            labels.Add(label);
        }

        private bool HasAnySemanticMarkerAt(int x, int z) {
            return ContainsMarkerAt(SpawnMarkers, x, z)
                || ContainsMarkerAt(GoalMarkers, x, z)
                || ContainsPortalAt(x, z);
        }

        private static bool ContainsMarkerAt(List<SemanticMarker> markers, int x, int z) {
            if (markers == null) {
                return false;
            }

            foreach (var marker in markers) {
                if (marker != null && marker.Position.x == x && marker.Position.y == z) {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsPortalAt(int x, int z) {
            if (PortalPairs == null) {
                return false;
            }

            foreach (var pair in PortalPairs) {
                if (pair == null) {
                    continue;
                }

                if ((pair.EntrancePosition.x == x && pair.EntrancePosition.y == z)
                    || (pair.ExitPosition.x == x && pair.ExitPosition.y == z)) {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesMarker(List<SemanticMarker> markers, string id, int x, int z) {
            if (markers == null) {
                return false;
            }

            foreach (var marker in markers) {
                if (marker == null) {
                    continue;
                }

                if (string.Equals(marker.Id, id, StringComparison.OrdinalIgnoreCase)
                    && marker.Position.x == x
                    && marker.Position.y == z) {
                    return true;
                }
            }

            return false;
        }

        private static bool TryResolveMarker(List<SemanticMarker> markers, string markerId, out Vector2Int position) {
            if (markers != null) {
                foreach (var marker in markers) {
                    if (marker == null || !string.Equals(marker.Id, markerId, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    position = marker.Position;
                    return true;
                }
            }

            position = Vector2Int.zero;
            return false;
        }

        private static string BuildNextMarkerId(List<SemanticMarker> markers, string prefix) {
            int maxIndex = 0;
            if (markers != null) {
                foreach (var marker in markers) {
                    if (marker == null || string.IsNullOrWhiteSpace(marker.Id)) {
                        continue;
                    }

                    if (TryParseTrailingNumber(marker.Id, prefix, out int parsedIndex) && parsedIndex > maxIndex) {
                        maxIndex = parsedIndex;
                    }
                }
            }

            return $"{prefix}{maxIndex + 1}";
        }

        private int GetNextPortalPairNumber() {
            int maxIndex = 0;
            foreach (var pair in PortalPairs) {
                if (pair == null) {
                    continue;
                }

                if (int.TryParse(pair.PairId, out int parsedIndex) && parsedIndex > maxIndex) {
                    maxIndex = parsedIndex;
                }
            }

            return maxIndex + 1;
        }

        private static bool TryParseTrailingNumber(string value, string prefix, out int parsedNumber) {
            parsedNumber = 0;
            if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                return false;
            }

            string suffix = value.Substring(prefix.Length);
            return int.TryParse(suffix, out parsedNumber);
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
