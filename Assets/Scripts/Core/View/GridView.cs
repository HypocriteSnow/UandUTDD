namespace ArknightsLite.View {
    using UnityEngine;
    using ArknightsLite.Model;
    using ArknightsLite.Infrastructure;
    using ArknightsLite.Core;

    /// <summary>
    /// 网格可视化组件
    /// 职责：绘制网格线、显示格子类型、响应格子状态变化
    /// </summary>
    public class GridView : MonoBehaviour {
        
        [Header("可视化配置")]
        [SerializeField] private bool _showGrid = true;
        [SerializeField] private Color _gridLineColor = new Color(1, 1, 1, 0.3f);
        [SerializeField] private Color _groundColor = new Color(0.39f, 0.78f, 0.39f, 0.5f);        // 绿色
        [SerializeField] private Color _highGroundColor = new Color(0.16f, 0.31f, 0.16f, 0.5f);   // 墨绿色
        [SerializeField] private Color _forbiddenColor = new Color(1f, 1f, 1f, 0.5f);             // 白色
        [SerializeField] private Color _holeColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);            // 黑色
        [SerializeField] private Color _occupiedColor = new Color(1f, 0.7f, 0.4f, 0.7f);          // 橙色
        
        
        private void OnEnable() {
            // 监听网格变化事件
            EventManager.Instance.AddListener<GameEvents.GridChangedEvent>(OnGridChanged);
        }
        
        private void OnDisable() {
            // 移除事件监听
            if (EventManager.Instance != null) {
                EventManager.Instance.RemoveListener<GameEvents.GridChangedEvent>(OnGridChanged);
            }
        }
        
        private void OnGridChanged(GameEvents.GridChangedEvent evt) {
            // 格子状态改变时可以触发特效、音效等
            Debug.Log($"[GridView] Grid changed at ({evt.X}, {evt.Y})");
        }
        
        
        // ==================== Gizmos 可视化 ====================
        
        private void OnDrawGizmos() {
            if (!_showGrid) return;
            if (!GridModel.Instance.IsInitialized) return;
            
            DrawGridLines();
            DrawTileColors();
        }
        
        /// <summary>
        /// 绘制网格线
        /// </summary>
        private void DrawGridLines() {
            Gizmos.color = _gridLineColor;
            
            int width = GridModel.Instance.Width;
            int depth = GridModel.Instance.Depth;
            float cellSize = GridModel.Instance.CellSize;
            
            // 绘制 X 方向的线
            for (int x = 0; x <= width; x++) {
                Vector3 start = new Vector3(x * cellSize, 0, 0);
                Vector3 end = new Vector3(x * cellSize, 0, depth * cellSize);
                Gizmos.DrawLine(start, end);
            }
            
            // 绘制 Z 方向的线
            for (int z = 0; z <= depth; z++) {
                Vector3 start = new Vector3(0, 0, z * cellSize);
                Vector3 end = new Vector3(width * cellSize, 0, z * cellSize);
                Gizmos.DrawLine(start, end);
            }
        }
        
        /// <summary>
        /// 绘制格子颜色（根据类型和占据状态）
        /// </summary>
        private void DrawTileColors() {
            int width = GridModel.Instance.Width;
            int depth = GridModel.Instance.Depth;
            float cellSize = GridModel.Instance.CellSize;
            
            for (int x = 0; x < width; x++) {
                for (int z = 0; z < depth; z++) {
                    var tile = GridModel.Instance.GetTile(x, z);
                    if (tile == null) continue;
                    
                    // 根据格子类型选择颜色
                    Color color = GetTileColor(tile);
                    
                    // 如果格子被占据，覆盖高亮色
                    if (tile.Occupier != null) {
                        color = _occupiedColor;
                    }
                    
                    // 绘制格子（根据高度等级偏移 Y 坐标）
                    Gizmos.color = color;
                    Vector3 center = GridModel.Instance.GridToWorld(x, z, tile.HeightLevel);
                    center.y += 0.01f; // 略微抬高避免 Z-Fighting
                    Gizmos.DrawCube(center, new Vector3(cellSize * 0.9f, 0.02f, cellSize * 0.9f));
                }
            }
        }
        
        /// <summary>
        /// 根据格子类型获取颜色
        /// </summary>
        private Color GetTileColor(Tile tile) {
            switch (tile.Type) {
                case TileType.Ground:
                    return _groundColor;
                case TileType.HighGround:
                    return _highGroundColor;
                case TileType.Forbidden:
                    return _forbiddenColor;
                case TileType.Hole:
                    return _holeColor;
                default:
                    return Color.white;
            }
        }
        
        
        // ==================== 运行时交互（可选） ====================
        
        /// <summary>
        /// 点击格子测试（示例）
        /// </summary>
        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit)) {
                    Vector2Int gridPos = GridModel.Instance.WorldToGrid(hit.point);
                    Debug.Log($"[GridView] Clicked tile: {gridPos}");
                }
            }
        }
    }
}
