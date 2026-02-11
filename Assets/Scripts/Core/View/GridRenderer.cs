namespace ArknightsLite.View {
    using UnityEngine;
    using System.Collections.Generic;
    using ArknightsLite.Model;
    using ArknightsLite.Infrastructure;
    using ArknightsLite.Core;
    using ArknightsLite.Config;

    /// <summary>
    /// 网格渲染器 - 负责生成和管理整个网格的视觉表现
    /// 遵循 View 层规范：被动响应 Model 变化
    /// </summary>
    public class GridRenderer : MonoBehaviour {
        
        [Header("配置")]
        [SerializeField] private GridVisualConfig _visualConfig;
        [SerializeField] private LevelConfig _levelConfig;
        
        [Header("对象池设置")]
        [SerializeField] private int _initialPoolSize = 100;
        
        [Header("调试选项")]
        [SerializeField] private bool _autoGenerate = true;
        
        
        private Dictionary<Vector2Int, TileRenderer> _tileRenderers = new Dictionary<Vector2Int, TileRenderer>();
        private Queue<GameObject> _tilePool = new Queue<GameObject>();
        private Transform _gridParent;
        
        
        private void Awake() {
            // 创建网格父对象
            _gridParent = new GameObject("Grid").transform;
            _gridParent.SetParent(transform);
            _gridParent.localPosition = Vector3.zero;
            
            // 初始化对象池
            InitializePool();
        }
        
        private void Start() {
            if (_autoGenerate && GridModel.Instance.IsInitialized) {
                GenerateGrid();
            }
        }
        
        private void OnEnable() {
            // 监听网格变化事件
            EventManager.Instance.AddListener<GameEvents.GridChangedEvent>(OnGridChanged);
        }
        
        private void OnDisable() {
            if (EventManager.Instance != null) {
                EventManager.Instance.RemoveListener<GameEvents.GridChangedEvent>(OnGridChanged);
            }
        }
        
        
        // ==================== 对象池管理 ====================
        
        /// <summary>
        /// 初始化对象池
        /// </summary>
        private void InitializePool() {
            if (_visualConfig == null || _visualConfig.tilePrefab == null) {
                Debug.LogError("[GridRenderer] VisualConfig or TilePrefab is null!");
                return;
            }
            
            for (int i = 0; i < _initialPoolSize; i++) {
                CreatePooledTile();
            }
        }
        
        /// <summary>
        /// 创建池化的格子对象
        /// </summary>
        private GameObject CreatePooledTile() {
            GameObject tile = Instantiate(_visualConfig.tilePrefab, _gridParent);
            tile.SetActive(false);
            
            // 确保有 TileRenderer 组件
            if (!tile.GetComponent<TileRenderer>()) {
                tile.AddComponent<TileRenderer>();
            }
            
            _tilePool.Enqueue(tile);
            return tile;
        }
        
        /// <summary>
        /// 从对象池获取格子
        /// </summary>
        private GameObject GetTileFromPool() {
            if (_tilePool.Count == 0) {
                return CreatePooledTile();
            }
            
            GameObject tile = _tilePool.Dequeue();
            tile.SetActive(true);
            return tile;
        }
        
        /// <summary>
        /// 归还格子到对象池
        /// </summary>
        private void ReturnTileToPool(GameObject tile) {
            tile.SetActive(false);
            _tilePool.Enqueue(tile);
        }
        
        
        // ==================== 网格生成 ====================
        
        /// <summary>
        /// 生成整个网格
        /// </summary>
        [ContextMenu("Generate Grid")]
        public void GenerateGrid() {
            if (!GridModel.Instance.IsInitialized) {
                Debug.LogError("[GridRenderer] GridModel is not initialized!");
                return;
            }
            
            if (_visualConfig == null) {
                Debug.LogError("[GridRenderer] GridVisualConfig is not assigned!");
                return;
            }
            
            // 清除现有网格
            ClearGrid();
            
            // 生成所有格子
            int width = GridModel.Instance.Width;
            int depth = GridModel.Instance.Depth;
            
            for (int x = 0; x < width; x++) {
                for (int z = 0; z < depth; z++) {
                    GenerateTile(x, z);
                }
            }
            
            Debug.Log($"[GridRenderer] Generated {width}x{depth} grid with {_tileRenderers.Count} tiles");
        }
        
        /// <summary>
        /// 生成单个格子
        /// </summary>
        private void GenerateTile(int x, int z) {
            Tile tileData = GridModel.Instance.GetTile(x, z);
            if (tileData == null) return;
            
            // 从对象池获取格子
            GameObject tileObj = GetTileFromPool();
            
            // 设置位置（考虑高度等级）
            Vector3 worldPos = GridModel.Instance.GridToWorld(x, z, tileData.HeightLevel);
            tileObj.transform.position = worldPos;
            tileObj.transform.localScale = Vector3.one * GridModel.Instance.CellSize * 0.95f; // 略微缩小避免重叠
            tileObj.name = $"Tile_{x}_{z}";
            
            // 初始化 TileRenderer
            TileRenderer renderer = tileObj.GetComponent<TileRenderer>();
            bool isSpawn = _levelConfig != null && _levelConfig.IsSpawnPoint(x, z);
            bool isGoal = _levelConfig != null && _levelConfig.IsGoalPoint(x, z);
            renderer.Initialize(tileData, _visualConfig, isSpawn, isGoal);
            
            // 记录
            _tileRenderers[new Vector2Int(x, z)] = renderer;
        }
        
        /// <summary>
        /// 清除所有格子
        /// </summary>
        [ContextMenu("Clear Grid")]
        public void ClearGrid() {
            foreach (var kvp in _tileRenderers) {
                ReturnTileToPool(kvp.Value.gameObject);
            }
            _tileRenderers.Clear();
        }
        
        
        // ==================== 事件响应 ====================
        
        /// <summary>
        /// 响应网格变化事件
        /// </summary>
        private void OnGridChanged(GameEvents.GridChangedEvent evt) {
            Vector2Int pos = new Vector2Int(evt.X, evt.Y);
            
            if (_tileRenderers.TryGetValue(pos, out TileRenderer renderer)) {
                renderer.SetOccupied(evt.Occupier != null);
                Debug.Log($"[GridRenderer] Updated tile ({evt.X}, {evt.Y}), Occupied: {evt.Occupier != null}");
            }
        }
        
        
        // ==================== 公共接口 ====================
        
        /// <summary>
        /// 获取指定坐标的渲染器
        /// </summary>
        public TileRenderer GetTileRenderer(int x, int z) {
            _tileRenderers.TryGetValue(new Vector2Int(x, z), out TileRenderer renderer);
            return renderer;
        }
        
        /// <summary>
        /// 设置视觉配置（运行时切换主题）
        /// </summary>
        public void SetVisualConfig(GridVisualConfig config) {
            _visualConfig = config;
            
            // 更新所有格子的视觉
            foreach (var kvp in _tileRenderers) {
                kvp.Value.UpdateVisual();
            }
        }
    }
}
