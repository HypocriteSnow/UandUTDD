# 核心逻辑层 - 01. 网格与空间系统 (GridSystem)

## 1. 系统概述
`GridModel` 负责管理战斗地图的静态与动态数据。它是整个塔防逻辑的物理基础，提供坐标转换、地形查询和放置检测功能。

## 2. 核心职责
1.  **地图数据管理**：存储 `Tile` 二维数组，记录每个格子的属性（可部署、不可通行、特殊地形）。
2.  **坐标转换**：提供 `GridPos` (逻辑坐标) <-> `WorldPos` (Unity世界坐标) 的双向转换。
3.  **放置检测**：判断某坐标是否允许部署干员或放置障碍物。

## 3. 数据结构

### 3.1 Tile (格子数据)
```csharp
public enum TileType {
    Ground,     // 地面 (可部署近战)
    HighGround, // 高台 (可部署远程)
    Forbidden,  // 禁区 (不可部署)
    Hole        // 坑洞 (不可通行，空军可过)
}

public class Tile {
    public int X;
    public int Y;
    public TileType Type;
    public bool IsWalkable; // 基础通行性
    public string DeployableTag; // "Melee", "Ranged", "All", "None"
    
    // 动态状态
    public Entity Occupier; // 当前格子上的干员/障碍物
    
    public bool IsBlocked => !IsWalkable || (Occupier != null && Occupier.IsBlocker);
}
```

### 3.2 GridModel (单例)
```csharp
namespace ArknightsLite.Model {
    using UnityEngine;
    using ArknightsLite.Infrastructure;

    public class GridModel {
        // 单例实现
        private static GridModel _instance;
        public static GridModel Instance => _instance ??= new GridModel();
        private GridModel() {}

        // 数据字段
        private int _width;
        private int _height;
        private float _cellSize = 1.0f;
        private Tile[,] _grid;

        // 初始化
        public void Init(int width, int height, float cellSize) {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _grid = new Tile[width, height];
            
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    _grid[x, y] = new Tile { X = x, Y = y, Type = TileType.Ground, IsWalkable = true };
                }
            }
        }

        // 坐标转换
        public Vector3 GridToWorld(int x, int y) {
            return new Vector3(x * _cellSize, 0, y * _cellSize);
        }

        public Vector2Int WorldToGrid(Vector3 worldPos) {
            int x = Mathf.RoundToInt(worldPos.x / _cellSize);
            int y = Mathf.RoundToInt(worldPos.z / _cellSize);
            return new Vector2Int(x, y);
        }

        // 查询接口
        public Tile GetTile(int x, int y) {
            if (IsValid(x, y)) return _grid[x, y];
            return null;
        }

        public bool IsValid(int x, int y) {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        // 动态修改
        public void SetOccupier(int x, int y, Entity entity) {
            var tile = GetTile(x, y);
            if (tile != null) {
                tile.Occupier = entity;
                // 地形改变可能影响寻路，需通知 NavigationSystem
                EventManager.Instance.Broadcast(new GameEvents.GridChangedEvent { X = x, Y = y });
            }
        }
    }
}
```

## 4. 扩展性设计
*   **多层网格**：目前仅设计了 `Ground` 层。未来可扩展 `AirGrid` 或 `EffectGrid` (地火/毒雾)，只需在 `GridModel` 中增加对应的二维数组即可。
*   **地图加载**：`Init` 方法目前是硬编码，未来应对接 `ConfigManager`，读取 JSON 或二进制地图文件进行初始化。
