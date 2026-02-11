# 开发规范 - 关卡编辑器 (Level Editor)

## 1. 系统概述

关卡编辑器是一套可视化的场景编辑工具，允许设计师在 Unity 编辑器中直接绘制和修改关卡布局，无需手动编写坐标数据。

### 1.1 核心设计理念
- **所见即所得 (WYSIWYG)**：在 Scene 视图中直接看到并编辑格子。
- **数据驱动**：所有修改自动同步到 `LevelConfig` (ScriptableObject)。
- **非破坏性**：编辑用的 GameObject 不会污染运行时场景。
- **防腐层隔离**：编辑器代码严格隔离在 `Editor` 文件夹，不影响运行时性能。

## 2. 架构设计

### 2.1 三层架构

```
┌─────────────────────────────────────────────┐
│         Data Layer (数据层)                  │
│  ┌──────────────────────────────────────┐   │
│  │ LevelConfig (ScriptableObject)       │   │
│  │ - defaultTileType (默认类型)         │   │
│  │ - specialTiles (差异数据)            │   │
│  │ - SetTileData() (回写接口)           │   │
│  └──────────────────────────────────────┘   │
└─────────────────────────────────────────────┘
                     ↕ (双向同步)
┌─────────────────────────────────────────────┐
│      Editor Layer (编辑器交互层)            │
│  ┌─────────────────┐  ┌──────────────────┐ │
│  │ LevelEditorWindow│  │ TileAuthoring    │ │
│  │ - 生成网格       │  │ - 持有坐标       │ │
│  │ - 笔刷工具       │  │ - 同步数据       │ │
│  │ - Scene输入处理  │  │ - 更新视觉       │ │
│  └─────────────────┘  └──────────────────┘ │
└─────────────────────────────────────────────┘
                     ↓ (生成运行时)
┌─────────────────────────────────────────────┐
│       Runtime Layer (运行时层)               │
│  ┌──────────────────────────────────────┐   │
│  │ GridModel → GridRenderer             │   │
│  │ (读取 LevelConfig，不感知编辑器)     │   │
│  └──────────────────────────────────────┘   │
└─────────────────────────────────────────────┘
```

### 2.2 数据流向

**编辑流程**：
```
1. LevelEditorWindow.Generate()
   → 读取 LevelConfig
   → 生成 TileAuthoring GameObject

2. 用户修改 TileAuthoring.tileType (Inspector)
   → OnValidate() 触发
   → LevelConfig.SetTileData()
   → EditorUtility.SetDirty()

3. 保存资源 (Ctrl+S)
   → LevelConfig 持久化到磁盘
```

**运行流程**：
```
1. GameMain.Start()
   → GridModel.LoadFromConfig(LevelConfig)
   → GridRenderer.GenerateGrid()
   → 运行时网格（与编辑器无关）
```

## 3. 核心组件规范

### 3.1 LevelConfig (数据层)

#### 新增字段
```csharp
public TileType defaultTileType = TileType.Forbidden;
```

#### 数据存储策略
- **默认格子**：不存储在 `specialTiles` 中（节省空间）。
- **非默认格子**：仅当 `tileType != defaultTileType` 时，添加到 `specialTiles`。
- **删除优化**：当格子被改回默认类型时，从 `specialTiles` 中移除。

#### 回写接口
```csharp
/// <summary>
/// 设置格子数据（用于编辑器回写）
/// </summary>
/// <param name="x">X坐标</param>
/// <param name="z">Z坐标</param>
/// <param name="data">格子数据（如果为null或等于默认类型，则从列表中移除）</param>
public void SetTileData(int x, int z, TileData data) {
    // 1. 查找现有数据
    var existing = specialTiles.Find(t => t.x == x && t.z == z);
    
    // 2. 判断是否为默认类型
    bool isDefault = (data == null || data.tileType == defaultTileType);
    
    // 3. 添加、更新或移除
    if (isDefault) {
        if (existing != null) specialTiles.Remove(existing);
    } else {
        if (existing != null) {
            // 更新现有数据
            existing.tileType = data.tileType;
            existing.heightLevel = data.heightLevel;
            // ...
        } else {
            // 添加新数据
            specialTiles.Add(data);
        }
    }
}
```

### 3.2 TileAuthoring (交互组件)

#### 职责
- **数据桥梁**：连接场景 GameObject 和 LevelConfig。
- **视觉反馈**：根据类型实时更新材质颜色。
- **自动同步**：Inspector 修改时自动回写配置。

#### 代码规范
```csharp
#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
public class TileAuthoring : MonoBehaviour {
    [Header("坐标（只读）")]
    [SerializeField] private int _x;
    [SerializeField] private int _z;
    
    [Header("格子配置")]
    public TileType tileType = TileType.Forbidden;
    public int heightLevel = 0;
    
    [HideInInspector]
    public LevelConfig config;
    
    [HideInInspector]
    public GridVisualConfig visualConfig;
    
    // 当 Inspector 值改变时触发
    private void OnValidate() {
        if (config == null) return;
        
        SyncToConfig();
        UpdateVisual();
    }
    
    // 同步到配置
    private void SyncToConfig() {
        var data = new TileData {
            x = _x,
            z = _z,
            tileType = tileType,
            heightLevel = heightLevel,
            walkable = (tileType != TileType.Forbidden),
            deployTag = "All"
        };
        
        config.SetTileData(_x, _z, data);
        EditorUtility.SetDirty(config);
    }
    
    // 更新视觉
    private void UpdateVisual() {
        if (visualConfig == null) return;
        
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null) {
            renderer.material = visualConfig.GetMaterialForType(tileType);
        }
        
        // 更新高度
        var pos = transform.position;
        pos.y = heightLevel * 1.0f;
        transform.position = pos;
    }
}
#endif
```

#### 注意事项
- **必须使用 `#if UNITY_EDITOR` 宏**：确保不编译到运行时。
- **ExecuteInEditMode**：允许在编辑模式下执行 `OnValidate`。
- **SetDirty**：必须调用，否则修改不会保存。

### 3.3 LevelEditorWindow (编辑器窗口)

#### 核心功能
1. **生成编辑网格**：根据 Config 创建临时 GameObject。
2. **清理网格**：编辑完成后删除临时物体。
3. **笔刷工具**：在 Scene 视图中涂抹格子类型。

#### 代码规范
```csharp
public class LevelEditorWindow : EditorWindow {
    private LevelConfig _config;
    private GridVisualConfig _visualConfig;
    private GameObject _gridParent;
    
    // 笔刷设置
    private TileType _brushType = TileType.Ground;
    private bool _brushEnabled = false;
    
    [MenuItem("ArknightsLite/Level Editor")]
    public static void ShowWindow() {
        GetWindow<LevelEditorWindow>("关卡编辑器");
    }
    
    private void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    private void OnDisable() {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    // 生成网格
    private void GenerateEditGrid() {
        // 1. 清理旧网格
        ClearEditGrid();
        
        // 2. 创建父节点（DontSave 避免保存到场景）
        _gridParent = new GameObject("LevelEditGrid");
        _gridParent.hideFlags = HideFlags.DontSave;
        
        // 3. 生成格子
        for (int x = 0; x < _config.mapWidth; x++) {
            for (int z = 0; z < _config.mapDepth; z++) {
                CreateTileAuthoring(x, z);
            }
        }
    }
    
    // 创建单个格子
    private void CreateTileAuthoring(int x, int z) {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(_gridParent.transform);
        cube.transform.localScale = new Vector3(0.95f, 0.2f, 0.95f);
        cube.transform.position = new Vector3(x, 0, z);
        cube.name = $"Tile_{x}_{z}";
        
        var authoring = cube.AddComponent<TileAuthoring>();
        authoring.config = _config;
        authoring.visualConfig = _visualConfig;
        // 初始化类型
        var data = _config.GetTileData(x, z);
        authoring.tileType = data?.tileType ?? _config.defaultTileType;
        authoring.heightLevel = data?.heightLevel ?? 0;
    }
    
    // Scene 视图输入处理
    private void OnSceneGUI(SceneView sceneView) {
        if (!_brushEnabled) return;
        
        Event e = Event.current;
        
        // 按住鼠标左键涂抹
        if (e.type == EventType.MouseDrag || e.type == EventType.MouseDown) {
            if (e.button == 0) {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit)) {
                    var authoring = hit.collider.GetComponent<TileAuthoring>();
                    if (authoring != null) {
                        Undo.RecordObject(authoring, "Paint Tile");
                        authoring.tileType = _brushType;
                        EditorUtility.SetDirty(authoring);
                        e.Use(); // 消耗事件
                    }
                }
            }
        }
    }
}
```

## 4. 使用流程

### 4.1 编辑关卡
1. **打开编辑器**：`ArknightsLite -> Level Editor`。
2. **选择配置**：拖入 `LevelConfig` 和 `GridVisualConfig`。
3. **生成网格**：点击"生成编辑网格"。
4. **编辑方式**：
   - **方式一（Inspector）**：选中格子，修改 `Tile Type`。
   - **方式二（笔刷）**：勾选"启用笔刷"，选择类型，鼠标涂抹。
5. **保存**：Ctrl+S 保存资源。
6. **清理**：点击"清理编辑网格"。

### 4.2 运行测试
1. 编辑完成后清理网格。
2. 确保 `GameMain` 和 `GridRenderer` 配置正确。
3. 点击 Play，运行时网格会根据 `LevelConfig` 生成。

## 5. 扩展性设计

### 5.1 笔刷策略模式
未来可扩展多种笔刷类型：
```csharp
public interface IBrushStrategy {
    void Paint(TileAuthoring tile);
}

public class TypeBrush : IBrushStrategy {
    public TileType targetType;
    public void Paint(TileAuthoring tile) {
        tile.tileType = targetType;
    }
}

public class HeightBrush : IBrushStrategy {
    public int targetHeight;
    public void Paint(TileAuthoring tile) {
        tile.heightLevel = targetHeight;
    }
}
```

### 5.2 快捷键系统
```csharp
// 数字键切换笔刷类型
if (e.type == EventType.KeyDown) {
    switch (e.keyCode) {
        case KeyCode.Alpha1: _brushType = TileType.Ground; break;
        case KeyCode.Alpha2: _brushType = TileType.HighGround; break;
        case KeyCode.Alpha3: _brushType = TileType.Forbidden; break;
    }
}
```

### 5.3 批量操作
- **矩形填充**：拖动选区批量修改。
- **填充工具**：点击封闭区域，自动填充。
- **随机笔刷**：随机分布多种类型。

## 6. 注意事项

### 6.1 性能优化
- **对象池**：编辑网格可使用对象池避免频繁创建销毁。
- **延迟更新**：`OnValidate` 中避免重复调用 `SetDirty`，使用定时器合并。
- **LOD**：大地图时，远处格子使用简化显示。

### 6.2 数据安全
- **Undo 支持**：所有修改必须使用 `Undo.RecordObject`，支持撤销。
- **自动保存**：定时提醒用户保存，避免数据丢失。
- **版本控制**：`LevelConfig` 是纯文本资源，方便 Git 管理。

### 6.3 团队协作
- **锁定机制**：编辑模式下锁定 Config，防止多人同时编辑。
- **预览模式**：只读模式下可查看但不能修改。
- **差异对比**：集成 Git Diff 可视化工具。

## 7. 故障排查

### Q1: 修改后保存不生效
**A**: 检查是否调用了 `EditorUtility.SetDirty(config)`。

### Q2: 运行时格子和编辑器不一致
**A**: 
1. 确认保存了 LevelConfig（Ctrl+S）。
2. 检查 `GridModel.LoadFromConfig` 是否正确读取默认类型。

### Q3: 笔刷不响应
**A**: 
1. 确认勾选了"启用笔刷"。
2. 检查格子是否有 Collider 组件（Raycast 需要）。
3. 查看 Console 是否有错误。

## 8. 开发规范总结

| 规范项 | 要求 |
|--------|------|
| **命名空间** | 编辑器代码不需要命名空间或使用 `ArknightsLite.Editor` |
| **文件位置** | 必须放在 `Assets/Scripts/Editor/` 文件夹 |
| **宏保护** | 使用 `#if UNITY_EDITOR` 包裹 |
| **依赖注入** | 通过 Inspector 拖入，不使用 FindObjectOfType |
| **防腐层** | 编辑器代码不得引用运行时 View 层（GridRenderer 等） |
| **数据回写** | 必须调用 `SetDirty`，支持 `Undo` |
