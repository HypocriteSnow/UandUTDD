namespace ArknightsLite.DebugTools {
    using UnityEngine;
    using ArknightsLite.Model;

    /// <summary>
    /// 网格调试器 - 用于测试网格功能
    /// </summary>
    public class GridDebugger : MonoBehaviour {
        
        [Header("测试设置")]
        [SerializeField] private KeyCode _occupyKey = KeyCode.Space;
        [SerializeField] private KeyCode _clearKey = KeyCode.C;
        [SerializeField] private Vector2Int _testPosition = new Vector2Int(5, 5);
        
        
        private void Update() {
            if (!GridModel.Instance.IsInitialized) return;
            
            // 测试占据
            if (Input.GetKeyDown(_occupyKey)) {
                TestOccupy();
            }
            
            // 测试清除
            if (Input.GetKeyDown(_clearKey)) {
                TestClear();
            }
            
            // 鼠标点击测试
            if (Input.GetMouseButtonDown(0)) {
                TestMouseClick();
            }
        }
        
        /// <summary>
        /// 测试占据功能
        /// </summary>
        private void TestOccupy() {
            if (GridModel.Instance.IsValid(_testPosition.x, _testPosition.y)) {
                GridModel.Instance.SetOccupier(_testPosition.x, _testPosition.y, this);
                Debug.Log($"[GridDebugger] Occupied tile ({_testPosition.x}, {_testPosition.y})");
            } else {
                Debug.LogWarning($"[GridDebugger] Invalid position: {_testPosition}");
            }
        }
        
        /// <summary>
        /// 测试清除功能
        /// </summary>
        private void TestClear() {
            if (GridModel.Instance.IsValid(_testPosition.x, _testPosition.y)) {
                GridModel.Instance.ClearOccupier(_testPosition.x, _testPosition.y);
                Debug.Log($"[GridDebugger] Cleared tile ({_testPosition.x}, {_testPosition.y})");
            }
        }
        
        /// <summary>
        /// 测试鼠标点击
        /// </summary>
        private void TestMouseClick() {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                Vector2Int gridPos = GridModel.Instance.WorldToGrid(hit.point);
                
                if (GridModel.Instance.IsValid(gridPos.x, gridPos.y)) {
                    var tile = GridModel.Instance.GetTile(gridPos.x, gridPos.y);
                    Debug.Log($"[GridDebugger] Clicked tile ({gridPos.x}, {gridPos.y})\n" +
                              $"- Type: {tile.Type}\n" +
                              $"- Height: {tile.HeightLevel}\n" +
                              $"- Walkable: {tile.IsWalkable}\n" +
                              $"- Deployable: {tile.IsDeployable}");
                    
                    // 切换占据状态
                    if (tile.Occupier == null) {
                        GridModel.Instance.SetOccupier(gridPos.x, gridPos.y, this);
                    } else {
                        GridModel.Instance.ClearOccupier(gridPos.x, gridPos.y);
                    }
                }
            }
        }
        
        /// <summary>
        /// GUI 提示
        /// </summary>
        private void OnGUI() {
            if (!GridModel.Instance.IsInitialized) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Box("网格调试器", GUILayout.Width(290));
            GUILayout.Label($"Space: 占据格子 {_testPosition}");
            GUILayout.Label($"C: 清除格子 {_testPosition}");
            GUILayout.Label("鼠标左键: 点击格子查看信息");
            GUILayout.Label($"\n当前网格: {GridModel.Instance.Width}x{GridModel.Instance.Depth}");
            GUILayout.EndArea();
        }
    }
}
