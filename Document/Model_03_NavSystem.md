# 核心逻辑层 - 03. 寻路与阻挡系统 (NavSystem)

## 1. 系统概述
`NavModel` 负责计算敌人的移动路径，并处理动态阻挡逻辑。由于塔防游戏中地形会因放置箱子/干员而改变，寻路系统必须支持高效的动态重算。

## 2. 核心职责
1.  **路径计算**：计算从起点到终点的最短路径（支持 BFS 或 Flow Field）。
2.  **动态阻挡**：当干员放置时，判断敌人是否被阻挡；当箱子放置时，重算全局路径。
3.  **死路检测**：预判放置操作是否会导致敌人无路可走（如果是，则禁止放置）。

## 3. 算法选择
*   **MVP 阶段**：使用 **BFS (广度优先搜索)**。因为地图较小 (10x10 ~ 20x20)，BFS 足够快且能保证最短路。
*   **进阶阶段**：使用 **Flow Field (流场)**。适合大量单位共享同一个终点的情况，只需计算一次场，所有单位查表即可移动。

## 4. 核心逻辑

### 4.1 NavModel (单例)
```csharp
namespace ArknightsLite.Model {
    using System.Collections.Generic;
    using UnityEngine;

    public class NavModel {
        private static NavModel _instance;
        public static NavModel Instance => _instance ??= new NavModel();
        private NavModel() {}

        // 路径缓存 (Flow Field: 每个格子指向下一个格子的方向)
        private Dictionary<Vector2Int, Vector2Int> _flowField = new Dictionary<Vector2Int, Vector2Int>();
        private Vector2Int _targetPos;

        // 更新流场 (当且仅当地形改变时调用)
        public void UpdateFlowField(Vector2Int target) {
            _targetPos = target;
            _flowField.Clear();
            
            // 执行 BFS 反向搜索 (从终点向起点)
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(target);
            _flowField[target] = target; // 终点指向自己

            while (queue.Count > 0) {
                var current = queue.Dequeue();
                
                foreach (var neighbor in GetNeighbors(current)) {
                    if (!_flowField.ContainsKey(neighbor) && GridModel.Instance.GetTile(neighbor.x, neighbor.y).IsWalkable) {
                        _flowField[neighbor] = current; // 指向来源（即去往终点的下一步）
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        // 获取下一步移动方向
        public Vector2Int GetNextStep(Vector2Int current) {
            if (_flowField.TryGetValue(current, out var next)) {
                return next;
            }
            return current; // 无路可走或已在终点
        }
        
        // 辅助：获取邻居
        private List<Vector2Int> GetNeighbors(Vector2Int pos) {
            // 返回上下左右 4 个邻居
            return new List<Vector2Int> {
                new Vector2Int(pos.x + 1, pos.y),
                new Vector2Int(pos.x - 1, pos.y),
                new Vector2Int(pos.x, pos.y + 1),
                new Vector2Int(pos.x, pos.y - 1)
            };
        }
    }
}
```

### 4.2 阻挡逻辑 (Blocking Logic)
阻挡不仅仅是寻路层面的“墙”，更是物理层面的“吸附”。

*   **触发时机**：在 `EntityModel.OnTick` 或 `MovementSystem` 中，每当敌人移动进入一个新的格子。
*   **判定流程**：
    1.  敌人进入格子 `(x, y)`。
    2.  检查该格子是否有 `Operator`。
    3.  如果有，检查 `Operator.BlockCount > Operator.CurrentBlockedEnemies`。
    4.  如果满足，敌人状态设为 `Blocked`，停止移动；干员 `CurrentBlockedEnemies++`。

## 5. 扩展性设计
*   **不可阻挡单位**：幽灵/弑君者等单位拥有 `Unblockable` 属性，在判定流程第 3 步直接跳过。
*   **阻挡数修改**：蛇屠箱开启技能 `BlockCount + 1`，需即时更新阻挡状态（可能吸附更多敌人）。
