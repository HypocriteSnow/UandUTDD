namespace ArknightsLite.Config {
    using System.Collections.Generic;
    using ArknightsLite.Model;
    using UnityEngine;

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "ArknightsLite/Level Config", order = 1)]
    public class LevelConfig : ScriptableObject {
        [Header("地图尺寸")]
        [Tooltip("地图宽度，沿 X 轴方向的格子数量")]
        [Range(5, 30)]
        public int mapWidth = 10;

        [Tooltip("地图深度，沿 Z 轴方向的格子数量")]
        [Range(5, 30)]
        public int mapDepth = 10;

        [Tooltip("单元格尺寸")]
        [Range(0.5f, 2.0f)]
        public float cellSize = 1.0f;

        [Tooltip("默认地块类型")]
        public TileType defaultTileType = TileType.Forbidden;

        [Header("运行时参数")]
        [Tooltip("初始部署点")]
        public int initialDp = 20;

        [Tooltip("基地生命值")]
        public int baseHealth = 3;

        [Tooltip("部署点恢复间隔")]
        public float dpRecoveryInterval = 1f;

        [Tooltip("单次恢复部署点")]
        public int dpRecoveryAmount = 1;

        [Header("关卡语义点")]
        [Tooltip("终点坐标，未设置时为 (-1, -1)")]
        public Vector2Int goalPoint = new Vector2Int(-1, -1);

        [Tooltip("出生点坐标列表")]
        public List<Vector2Int> spawnPoints = new List<Vector2Int>();

        [Header("显式地块数据")]
        [Tooltip("只保存默认地块以外，或语义点要求显式导出的地块")]
        public List<TileData> specialTiles = new List<TileData>();

        [Header("扩展数据")]
        public List<PortalDefinition> portals = new List<PortalDefinition>();
        public List<WaveDefinition> waves = new List<WaveDefinition>();
        public List<EnemyTemplateData> enemies = new List<EnemyTemplateData>();
        public List<OperatorTemplateData> operators = new List<OperatorTemplateData>();

        public TileData GetTileData(int x, int z) {
            var existing = specialTiles.Find(t => t.x == x && t.z == z);
            if (existing != null) {
                return existing;
            }

            return new TileData {
                x = x,
                z = z,
                tileType = defaultTileType,
                heightLevel = 0,
                walkable = defaultTileType != TileType.Forbidden && defaultTileType != TileType.Hole,
                deployTag = "All"
            };
        }

        public bool IsSpawnPoint(int x, int z) {
            return spawnPoints != null && spawnPoints.Exists(p => p.x == x && p.y == z);
        }

        public bool IsGoalPoint(int x, int z) {
            return goalPoint.x == x && goalPoint.y == z;
        }

        public void SetTileData(int x, int z, TileData data) {
            var existing = specialTiles.Find(t => t.x == x && t.z == z);
            bool isDefault = data == null || (data.tileType == defaultTileType && data.heightLevel == 0);

            if (isDefault) {
                if (existing != null) {
                    specialTiles.Remove(existing);
                }

                return;
            }

            if (existing != null) {
                existing.tileType = data.tileType;
                existing.heightLevel = data.heightLevel;
                existing.walkable = data.walkable;
                existing.deployTag = data.deployTag;
                return;
            }

            specialTiles.Add(new TileData {
                x = x,
                z = z,
                tileType = data.tileType,
                heightLevel = data.heightLevel,
                walkable = data.walkable,
                deployTag = data.deployTag
            });
        }

        public List<string> CollectValidationErrors() {
            var errors = new List<string>();

            if (mapWidth <= 0 || mapDepth <= 0) {
                errors.Add("[LevelConfig] Invalid map size: width and depth must be positive");
            }

            if (cellSize <= 0f) {
                errors.Add("[LevelConfig] Invalid cellSize: must be positive");
            }

            if (goalPoint.x < 0 || goalPoint.x >= mapWidth || goalPoint.y < 0 || goalPoint.y >= mapDepth) {
                errors.Add($"[LevelConfig] Goal point ({goalPoint.x}, {goalPoint.y}) is out of bounds");
            }

            if (spawnPoints == null || spawnPoints.Count == 0) {
                errors.Add("[LevelConfig] At least one spawn point is required");
            } else {
                foreach (var spawn in spawnPoints) {
                    if (spawn.x < 0 || spawn.x >= mapWidth || spawn.y < 0 || spawn.y >= mapDepth) {
                        errors.Add($"[LevelConfig] Spawn point ({spawn.x}, {spawn.y}) is out of bounds");
                    }
                }
            }

            if (specialTiles != null) {
                foreach (var tile in specialTiles) {
                    if (tile.x < 0 || tile.x >= mapWidth || tile.z < 0 || tile.z >= mapDepth) {
                        errors.Add($"[LevelConfig] Special tile ({tile.x}, {tile.z}) is out of bounds");
                    }
                }
            }

            if (spawnPoints != null) {
                foreach (var spawn in spawnPoints) {
                    if (!HasExplicitWalkableTile(spawn.x, spawn.y)) {
                        errors.Add($"[LevelConfig] Spawn point ({spawn.x}, {spawn.y}) requires explicit walkable tile data.");
                    }
                }
            }

            if (!HasExplicitWalkableTile(goalPoint.x, goalPoint.y)) {
                errors.Add($"[LevelConfig] Goal point ({goalPoint.x}, {goalPoint.y}) requires explicit walkable tile data.");
            }

            if (portals != null) {
                foreach (var portal in portals) {
                    if (portal == null) {
                        continue;
                    }

                    if (!HasExplicitWalkableTile(portal.inPos.x, portal.inPos.y)) {
                        errors.Add($"[LevelConfig] Portal entrance ({portal.inPos.x}, {portal.inPos.y}) requires explicit walkable tile data.");
                    }

                    if (!HasExplicitWalkableTile(portal.outPos.x, portal.outPos.y)) {
                        errors.Add($"[LevelConfig] Portal exit ({portal.outPos.x}, {portal.outPos.y}) requires explicit walkable tile data.");
                    }
                }
            }

            return errors;
        }

        public bool Validate() {
            List<string> errors = CollectValidationErrors();
            foreach (var error in errors) {
                Debug.LogError(error);
            }

            return errors.Count == 0;
        }

        private bool HasExplicitWalkableTile(int x, int z) {
            if (specialTiles == null) {
                return false;
            }

            var tileData = specialTiles.Find(t => t.x == x && t.z == z);
            return tileData != null
                && tileData.walkable
                && tileData.tileType != TileType.Forbidden
                && tileData.tileType != TileType.Hole;
        }

    }
}
