namespace ArknightsLite.Config {
    using UnityEngine;
    using System.Collections.Generic;
    using ArknightsLite.Model;

    /// <summary>
    /// 关卡配置 - ScriptableObject
    /// 用于在 Inspector 中可视化编辑关卡数据
    /// 使用方式：右键 Create -> ArknightsLite -> Level Config
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "ArknightsLite/Level Config", order = 1)]
    public class LevelConfig : ScriptableObject {
        
        [Header("地图尺寸（俯视图）")]
        [Tooltip("地图宽度 - X轴方向的格子数量")]
        [Range(5, 30)]
        public int mapWidth = 10;
        
        [Tooltip("地图长度 - Z轴方向（纵深）的格子数量")]
        [Range(5, 30)]
        public int mapDepth = 10;
        
        [Tooltip("单元格尺寸（世界坐标单位）")]
        [Range(0.5f, 2.0f)]
        public float cellSize = 1.0f;
        
        [Tooltip("默认格子类型（未在特殊格子列表中的格子使用此类型）")]
        public TileType defaultTileType = TileType.Forbidden;
        
        
        [Header("关卡必需配置")]
        [Tooltip("终点坐标（敌人目标，唯一）")]
        public Vector2Int goalPoint = new Vector2Int(9, 9);
        
        [Tooltip("起点坐标列表（敌人出生点，可多个）")]
        public List<Vector2Int> spawnPoints = new List<Vector2Int> { 
            new Vector2Int(0, 0) 
        };
        
        
        [Header("特殊格子配置（可选）")]
        [Tooltip("特殊格子列表（不配置则全部为默认 Ground/高度0）")]
        public List<TileData> specialTiles = new List<TileData>();
        
        
        /// <summary>
        /// 获取指定坐标的格子配置（如果没有特殊配置则返回默认类型）
        /// </summary>
        public TileData GetTileData(int x, int z) {
            var existing = specialTiles.Find(t => t.x == x && t.z == z);
            if (existing != null) {
                return existing;
            }
            
            // 特殊处理：如果是起点或终点，且没有特殊配置，返回 Ground 类型而不是默认类型
            if (IsSpawnPoint(x, z) || IsGoalPoint(x, z)) {
                return new TileData {
                    x = x,
                    z = z,
                    tileType = TileType.Ground,
                    heightLevel = 0,
                    walkable = true,
                    deployTag = "All"
                };
            }
            
            // 特殊处理：如果是起点或终点，且没有特殊配置，返回 Ground 类型而不是默认类型
            if (IsSpawnPoint(x, z) || IsGoalPoint(x, z)) {
                return new TileData {
                    x = x,
                    z = z,
                    tileType = TileType.Ground,
                    heightLevel = 0,
                    walkable = true,
                    deployTag = "All"
                };
            }
            
            // 返回默认配置
            return new TileData {
                x = x,
                z = z,
                tileType = defaultTileType,
                heightLevel = 0,
                walkable = (defaultTileType != TileType.Forbidden && defaultTileType != TileType.Hole),
                deployTag = "All"
            };
        }
        
        /// <summary>
        /// 判断指定坐标是否为起点
        /// </summary>
        public bool IsSpawnPoint(int x, int z) {
            return spawnPoints.Exists(p => p.x == x && p.y == z);
        }
        
        /// <summary>
        /// 判断指定坐标是否为终点
        /// </summary>
        public bool IsGoalPoint(int x, int z) {
            return goalPoint.x == x && goalPoint.y == z;
        }
        
        /// <summary>
        /// 设置格子数据（用于编辑器回写）
        /// 策略：仅存储非默认类型的格子，节省空间
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="z">Z坐标</param>
        /// <param name="data">格子数据</param>
        public void SetTileData(int x, int z, TileData data) {
            // 查找现有数据
            var existing = specialTiles.Find(t => t.x == x && t.z == z);
            
            // 判断是否为默认类型
            bool isDefault = (data == null || 
                              (data.tileType == defaultTileType && data.heightLevel == 0));
            
            if (isDefault) {
                // 如果是默认类型，从列表中移除（节省空间）
                if (existing != null) {
                    specialTiles.Remove(existing);
                }
            } else {
                // 非默认类型，添加或更新
                if (existing != null) {
                    // 更新现有数据
                    existing.tileType = data.tileType;
                    existing.heightLevel = data.heightLevel;
                    existing.walkable = data.walkable;
                    existing.deployTag = data.deployTag;
                } else {
                    // 添加新数据
                    specialTiles.Add(data);
                }
            }
        }
        
        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool Validate() {
            // 1. 检查地图尺寸
            if (mapWidth <= 0 || mapDepth <= 0) {
                Debug.LogError("[LevelConfig] Invalid map size: width and depth must be positive");
                return false;
            }
            
            if (cellSize <= 0) {
                Debug.LogError("[LevelConfig] Invalid cellSize: must be positive");
                return false;
            }
            
            // 2. 检查终点是否在范围内
            if (goalPoint.x < 0 || goalPoint.x >= mapWidth || goalPoint.y < 0 || goalPoint.y >= mapDepth) {
                Debug.LogError($"[LevelConfig] Goal point ({goalPoint.x}, {goalPoint.y}) is out of bounds");
                return false;
            }
            
            // 3. 检查起点列表
            if (spawnPoints == null || spawnPoints.Count == 0) {
                Debug.LogError("[LevelConfig] At least one spawn point is required");
                return false;
            }
            
            foreach (var spawn in spawnPoints) {
                if (spawn.x < 0 || spawn.x >= mapWidth || spawn.y < 0 || spawn.y >= mapDepth) {
                    Debug.LogError($"[LevelConfig] Spawn point ({spawn.x}, {spawn.y}) is out of bounds");
                    return false;
                }
            }
            
            // 4. 检查特殊格子是否在范围内
            foreach (var tile in specialTiles) {
                if (tile.x < 0 || tile.x >= mapWidth || tile.z < 0 || tile.z >= mapDepth) {
                    Debug.LogError($"[LevelConfig] Special tile ({tile.x}, {tile.z}) is out of bounds");
                    return false;
                }
            }
            
            // 5. 检查起点和终点是否被显式设置为 Forbidden
            foreach (var spawn in spawnPoints) {
                var tileData = specialTiles.Find(t => t.x == spawn.x && t.z == spawn.y);
                if (tileData != null && tileData.tileType == TileType.Forbidden) {
                    Debug.LogWarning($"[LevelConfig] Spawn point ({spawn.x}, {spawn.y}) is explicitly set to Forbidden in specialTiles. This will be overridden to Ground at runtime.");
                }
            }
            
            var goalTileData = specialTiles.Find(t => t.x == goalPoint.x && t.z == goalPoint.y);
            if (goalTileData != null && goalTileData.tileType == TileType.Forbidden) {
                Debug.LogWarning($"[LevelConfig] Goal point ({goalPoint.x}, {goalPoint.y}) is explicitly set to Forbidden in specialTiles. This will be overridden to Ground at runtime.");
            }
            
            return true;
        }
    }
}
