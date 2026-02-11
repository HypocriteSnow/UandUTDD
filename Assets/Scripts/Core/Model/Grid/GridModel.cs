namespace ArknightsLite.Model {
    using UnityEngine;
    using ArknightsLite.Infrastructure;
    using ArknightsLite.Core;

    /// <summary>
    /// 网格管理模型 - 战场地图的核心数据管理
    /// 职责：地图数据管理、坐标转换、放置检测
    /// </summary>
    public class GridModel {
        
        // ==================== 单例实现 ====================
        
        private static GridModel _instance;
        public static GridModel Instance {
            get {
                if (_instance == null) {
                    _instance = new GridModel();
                }
                return _instance;
            }
        }
        
        private GridModel() {
            // 防止外部实例化
        }
        
        
        // ==================== 私有字段 ====================
        
        private int _width;
        private int _depth;
        private float _cellSize;
        private Tile[,] _grid;
        private bool _isInitialized;
        
        
        // ==================== 公共属性 (只读) ====================
        
        /// <summary>网格宽度（X轴方向格子数）</summary>
        public int Width => _width;
        
        /// <summary>网格深度（Z轴方向格子数）</summary>
        public int Depth => _depth;
        
        /// <summary>单元格尺寸（世界坐标单位）</summary>
        public float CellSize => _cellSize;
        
        /// <summary>是否已初始化</summary>
        public bool IsInitialized => _isInitialized;
        
        
        // ==================== 生命周期接口 ====================
        
        /// <summary>
        /// 初始化网格
        /// </summary>
        /// <param name="width">网格宽度（X轴格子数）</param>
        /// <param name="depth">网格深度（Z轴格子数）</param>
        /// <param name="cellSize">单元格尺寸</param>
        public void Init(int width, int depth, float cellSize = 1.0f) {
            _width = width;
            _depth = depth;
            _cellSize = cellSize;
            _grid = new Tile[width, depth];
            
            // 初始化所有格子（默认为地面，高度0）
            for (int x = 0; x < width; x++) {
                for (int z = 0; z < depth; z++) {
                    _grid[x, z] = new Tile(x, z, TileType.Ground, heightLevel: 0, walkable: true, deployTag: "All");
                }
            }
            
            _isInitialized = true;
            
            Debug.Log($"[GridModel] Initialized: {width}x{depth}, CellSize={cellSize}");
        }
        
        /// <summary>
        /// 逻辑帧更新（当前 Grid 为静态数据，无需每帧更新）
        /// </summary>
        public void OnTick(int tick) {
            // GridModel 本身不需要 Tick 驱动
            // 但保留接口以符合 Model 层规范
        }
        
        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear() {
            _grid = null;
            _width = 0;
            _depth = 0;
            _isInitialized = false;
            
            Debug.Log("[GridModel] Cleared");
        }
        
        
        // ==================== 坐标转换 ====================
        
        /// <summary>
        /// 逻辑坐标转世界坐标
        /// </summary>
        /// <param name="x">X轴格子坐标</param>
        /// <param name="z">Z轴格子坐标</param>
        /// <param name="heightLevel">高度等级（可选，用于计算Y轴高度）</param>
        public Vector3 GridToWorld(int x, int z, int heightLevel = 0) {
            float worldY = heightLevel * _cellSize; // 高度等级转世界坐标
            return new Vector3(x * _cellSize, worldY, z * _cellSize);
        }
        
        /// <summary>
        /// 逻辑坐标转世界坐标（Vector2Int 重载）
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPos, int heightLevel = 0) {
            return GridToWorld(gridPos.x, gridPos.y, heightLevel);
        }
        
        /// <summary>
        /// 世界坐标转逻辑坐标（返回 X, Z）
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPos) {
            int x = Mathf.RoundToInt(worldPos.x / _cellSize);
            int z = Mathf.RoundToInt(worldPos.z / _cellSize);
            return new Vector2Int(x, z);
        }
        
        
        // ==================== 查询接口 ====================
        
        /// <summary>
        /// 获取指定坐标的格子
        /// </summary>
        public Tile GetTile(int x, int z) {
            if (!IsValid(x, z)) {
                Debug.LogWarning($"[GridModel] Invalid coordinate: ({x}, {z})");
                return null;
            }
            return _grid[x, z];
        }
        
        /// <summary>
        /// 获取指定坐标的格子（Vector2Int 重载）
        /// </summary>
        public Tile GetTile(Vector2Int gridPos) {
            return GetTile(gridPos.x, gridPos.y);
        }
        
        /// <summary>
        /// 判断坐标是否有效
        /// </summary>
        public bool IsValid(int x, int z) {
            return x >= 0 && x < _width && z >= 0 && z < _depth;
        }
        
        /// <summary>
        /// 判断坐标是否有效（Vector2Int 重载）
        /// </summary>
        public bool IsValid(Vector2Int gridPos) {
            return IsValid(gridPos.x, gridPos.y);
        }
        
        /// <summary>
        /// 判断指定坐标是否可通行
        /// </summary>
        public bool IsWalkable(int x, int z) {
            var tile = GetTile(x, z);
            return tile != null && tile.IsWalkable;
        }
        
        /// <summary>
        /// 判断指定坐标是否可部署
        /// </summary>
        public bool IsDeployable(int x, int z) {
            var tile = GetTile(x, z);
            return tile != null && tile.IsDeployable;
        }
        
        
        // ==================== 动态修改接口 ====================
        
        /// <summary>
        /// 设置格子的占据者
        /// 会触发 GridChangedEvent 事件
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="z">Z坐标</param>
        /// <param name="occupier">占据者对象（Entity、Obstacle 等）</param>
        public void SetOccupier(int x, int z, object occupier) {
            var tile = GetTile(x, z);
            if (tile == null) return;
            
            tile.SetOccupier(occupier);
            
            // 广播事件通知 View 层和其他系统（如寻路系统）
            EventManager.Instance.Broadcast(new GameEvents.GridChangedEvent {
                X = x,
                Y = z,
                Occupier = occupier
            });
        }
        
        /// <summary>
        /// 清除格子的占据者
        /// </summary>
        public void ClearOccupier(int x, int z) {
            var tile = GetTile(x, z);
            if (tile == null) return;
            
            tile.ClearOccupier();
            
            // 广播事件
            EventManager.Instance.Broadcast(new GameEvents.GridChangedEvent {
                X = x,
                Y = z,
                Occupier = null
            });
        }
        
        
        // ==================== 扩展接口（预留） ====================
        
        /// <summary>
        /// 从配置加载地图数据
        /// </summary>
        public void LoadFromConfig(ArknightsLite.Config.LevelConfig config) {
            if (config == null) {
                Debug.LogError("[GridModel] LevelConfig is null");
                return;
            }
            
            if (!config.Validate()) {
                Debug.LogError("[GridModel] Invalid LevelConfig");
                return;
            }
            
            // 1. 初始化基础网格尺寸
            _width = config.mapWidth;
            _depth = config.mapDepth;
            _cellSize = config.cellSize;
            _grid = new Tile[_width, _depth];
            
            // 2. 使用默认类型初始化所有格子
            bool defaultWalkable = (config.defaultTileType != TileType.Forbidden && config.defaultTileType != TileType.Hole);
            
            for (int x = 0; x < _width; x++) {
                for (int z = 0; z < _depth; z++) {
                    _grid[x, z] = new Tile(
                        x, 
                        z, 
                        config.defaultTileType, 
                        heightLevel: 0, 
                        walkable: defaultWalkable, 
                        deployTag: "All"
                    );
                }
            }
            
            // 3. 应用特殊格子配置（覆盖默认值）
            foreach (var tileData in config.specialTiles) {
                if (IsValid(tileData.x, tileData.z)) {
                    _grid[tileData.x, tileData.z] = new Tile(
                        tileData.x, 
                        tileData.z, 
                        tileData.tileType,
                        tileData.heightLevel,
                        tileData.walkable, 
                        tileData.deployTag
                    );
                }
            }
            
            // 4. 强制确保起点和终点为可通行的地面类型
            // 即使 specialTiles 中将这些点设置为 Forbidden，也强制覆盖为 Ground
            foreach (var spawnPoint in config.spawnPoints) {
                if (IsValid(spawnPoint.x, spawnPoint.y)) {
                    var currentTile = _grid[spawnPoint.x, spawnPoint.y];
                    if (currentTile.Type == TileType.Forbidden || currentTile.Type == TileType.Hole) {
                        _grid[spawnPoint.x, spawnPoint.y] = new Tile(
                            spawnPoint.x, 
                            spawnPoint.y, 
                            TileType.Ground,
                            currentTile.HeightLevel,
                            walkable: true, 
                            currentTile.DeployableTag
                        );
                        Debug.Log($"[GridModel] Forced Spawn point ({spawnPoint.x}, {spawnPoint.y}) to Ground (was {currentTile.Type})");
                    }
                }
            }
            
            // 强制终点为地面类型
            if (IsValid(config.goalPoint.x, config.goalPoint.y)) {
                var currentTile = _grid[config.goalPoint.x, config.goalPoint.y];
                if (currentTile.Type == TileType.Forbidden || currentTile.Type == TileType.Hole) {
                    _grid[config.goalPoint.x, config.goalPoint.y] = new Tile(
                        config.goalPoint.x, 
                        config.goalPoint.y, 
                        TileType.Ground,
                        currentTile.HeightLevel,
                        walkable: true, 
                        currentTile.DeployableTag
                    );
                    Debug.Log($"[GridModel] Forced Goal point ({config.goalPoint.x}, {config.goalPoint.y}) to Ground (was {currentTile.Type})");
                }
            }
            
            // 4. 确保起点和终点坐标为 Ground（确保可通行）
            foreach (var spawnPoint in config.spawnPoints) {
                if (IsValid(spawnPoint.x, spawnPoint.y)) {
                    // 如果该位置还是默认的 Forbidden，改为 Ground
                    if (_grid[spawnPoint.x, spawnPoint.y].Type == TileType.Forbidden) {
                        _grid[spawnPoint.x, spawnPoint.y] = new Tile(
                            spawnPoint.x, spawnPoint.y, 
                            TileType.Ground,  // 强制为 Ground
                            heightLevel: 0, 
                            walkable: true, 
                            deployTag: "All"
                        );
                    }
                }
            }

            // 终点同理
            if (IsValid(config.goalPoint.x, config.goalPoint.y)) {
                if (_grid[config.goalPoint.x, config.goalPoint.y].Type == TileType.Forbidden) {
                    _grid[config.goalPoint.x, config.goalPoint.y] = new Tile(
                        config.goalPoint.x, config.goalPoint.y,
                        TileType.Ground, // 强制为 Ground
                        heightLevel: 0,
                        walkable: true,
                        deployTag: "All"
                    );
                }
            }
            
            _isInitialized = true;
            
            Debug.Log($"[GridModel] Loaded from config: {config.name}, DefaultType={config.defaultTileType}, SpecialTiles={config.specialTiles.Count}, SpawnPoints={config.spawnPoints.Count}, Goal=({config.goalPoint.x},{config.goalPoint.y})");
        }
        
        /// <summary>
        /// 设置特定格子的类型（用于动态地形变化）
        /// </summary>
        public void SetTileType(int x, int y, TileType newType) {
            var tile = GetTile(x, y);
            if (tile == null) return;
            
            // 注意：Tile.Type 目前是只读的，未来如需支持动态修改，
            // 需要在 Tile 类中添加 internal setter
            Debug.LogWarning("[GridModel] SetTileType requires Tile.Type to have internal setter");
        }
    }
}
