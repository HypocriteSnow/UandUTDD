namespace ArknightsLite.View {
    using UnityEngine;
    using ArknightsLite.Model;
    using ArknightsLite.Config;

    /// <summary>
    /// 单个格子的渲染器
    /// 负责单个格子的视觉表现
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class TileRenderer : MonoBehaviour {
        
        private MeshRenderer _renderer;
        private Tile _tileData;
        private GridVisualConfig _visualConfig;
        
        // 标记状态
        private bool _isSpawnPoint;
        private bool _isGoalPoint;
        private bool _isOccupied;
        
        
        private void Awake() {
            _renderer = GetComponent<MeshRenderer>();
        }
        
        
        /// <summary>
        /// 初始化格子渲染器
        /// </summary>
        public void Initialize(Tile tile, GridVisualConfig visualConfig, bool isSpawn, bool isGoal) {
            _tileData = tile;
            _visualConfig = visualConfig;
            _isSpawnPoint = isSpawn;
            _isGoalPoint = isGoal;
            _isOccupied = tile.Occupier != null;
            
            UpdateVisual();
        }
        
        /// <summary>
        /// 更新视觉表现
        /// </summary>
        public void UpdateVisual() {
            if (_renderer == null || _visualConfig == null) return;
            
            Material targetMaterial = null;
            
            // 优先级：占据 > 起点/终点 > 格子类型
            if (_isOccupied && _visualConfig.occupiedMaterial != null) {
                targetMaterial = _visualConfig.occupiedMaterial;
            }
            else if (_isGoalPoint && _visualConfig.goalPointMaterial != null) {
                targetMaterial = _visualConfig.goalPointMaterial;
            }
            else if (_isSpawnPoint && _visualConfig.spawnPointMaterial != null) {
                targetMaterial = _visualConfig.spawnPointMaterial;
            }
            else {
                targetMaterial = _visualConfig.GetMaterialForType(_tileData.Type);
            }
            
            if (targetMaterial != null) {
                _renderer.material = targetMaterial;
            }
        }
        
        /// <summary>
        /// 设置占据状态
        /// </summary>
        public void SetOccupied(bool occupied) {
            _isOccupied = occupied;
            UpdateVisual();
        }
        
        /// <summary>
        /// 获取关联的格子数据
        /// </summary>
        public Tile TileData => _tileData;
    }
}
