# Unity Editor 文件夹规则与架构调整说明

**日期**: 2026-02-11  
**问题**: `TileAuthoring` 组件无法附加到 GameObject  
**原因**: 违反 Unity Editor 文件夹规则  
**解决方案**: 文件重定位 + 架构规范化

---

## 🚨 问题复现

### 错误信息
```
白色提示：Can't add script behaviour 'TileAuthoring' because it is an editor script. 
To attach a script it needs to be outside the 'Editor' folder.

NullReferenceException: Object reference not set to an instance of an object
LevelEditorWindow.CreateTileAuthoring (line 239)
```

### 错误链路
1. **`TileAuthoring.cs`** 位于 `Assets/Scripts/Editor/Authoring/`
2. **`LevelEditorWindow.cs:238`** 调用 `AddComponent<TileAuthoring>()`
3. **Unity 拒绝添加**：Editor 文件夹内的脚本不能作为组件
4. **返回 null** → **line 239 访问组件** → **NullReferenceException**

---

## 📚 Unity Editor 文件夹规则

### **核心规则**

| 文件夹位置 | 用途 | MonoBehaviour | 条件编译 | 构建包含 |
|-----------|------|--------------|---------|---------|
| `Editor/` | 纯编辑器工具 | ❌ 不可作为组件 | ❌ 不需要 | ❌ 自动排除 |
| `Runtime/` + `#if UNITY_EDITOR` | 编辑器模式组件 | ✅ 可作为组件 | ✅ 必须 | ❌ 条件排除 |
| `Runtime/` | 运行时组件 | ✅ 可作为组件 | - | ✅ 包含 |

### **Editor 文件夹适用场景**

✅ **应该放在 Editor/ 文件夹**：
- `EditorWindow` - 编辑器窗口
- `Editor` - 自定义 Inspector
- `PropertyDrawer` - 属性绘制器
- `[MenuItem]` - 菜单项
- `AssetPostprocessor` - 资源处理器

❌ **不应放在 Editor/ 文件夹**：
- `MonoBehaviour` 组件（即使只在编辑器使用）
- `ScriptableObject`（如果需要在场景引用）
- 任何需要附加到 GameObject 的脚本

---

## ✅ 解决方案

### **文件重定位**

```diff
旧位置（❌ 错误）:
- Assets/Scripts/Editor/Authoring/TileAuthoring.cs

新位置（✅ 正确）:
+ Assets/Scripts/Runtime/EditorTools/TileAuthoring.cs
```

### **为什么这样修改？**

| 特性 | TileAuthoring 的需求 | 为什么需要在 Runtime/ |
|------|---------------------|---------------------|
| **MonoBehaviour** | ✅ 是 | 需要附加到 GameObject，必须在 Runtime/ |
| **编辑器专用** | ✅ 是 | 使用 `#if UNITY_EDITOR` 条件编译 |
| **ExecuteInEditMode** | ✅ 是 | 编辑模式下运行，但仍是场景组件 |
| **构建排除** | ✅ 需要 | `#if UNITY_EDITOR` 自动排除 |

---

## 🏗️ 架构规范

### **新的文件组织结构**

```
Assets/Scripts/
├── Core/
│   ├── Infrastructure/        ← Manager 层（MonoSingleton）
│   ├── Model/                ← Model 层（纯 C# 单例）
│   └── View/                 ← View 层（运行时 MonoBehaviour）
│
├── Config/                   ← 配置数据（ScriptableObject）
│
├── Runtime/
│   └── EditorTools/          ← 编辑器模式 MonoBehaviour 组件
│       └── TileAuthoring.cs  ← #if UNITY_EDITOR + MonoBehaviour
│
└── Editor/                   ← 纯编辑器工具（非 MonoBehaviour）
    ├── LevelEditorWindow.cs  ← EditorWindow
    ├── LevelConfigEditor.cs  ← CustomEditor
    └── GridSetupHelper.cs    ← EditorWindow (一键生成工具)
```

### **职责划分**

#### **`Runtime/EditorTools/`** - 编辑器模式场景组件
- **特征**：
  - `MonoBehaviour` 组件
  - `#if UNITY_EDITOR` 条件编译
  - `[ExecuteInEditMode]` 或 `[ExecuteAlways]`
  - 可附加到场景 GameObject
- **示例**：
  - `TileAuthoring.cs` - 格子编辑组件
  - 未来可能的 `PathPreview.cs` - 路径预览组件

#### **`Editor/`** - 纯编辑器工具
- **特征**：
  - 不是 `MonoBehaviour`
  - Unity 自动识别为编辑器脚本
  - 不需要条件编译（Unity 自动排除）
- **示例**：
  - `LevelEditorWindow.cs` - 编辑器窗口
  - `LevelConfigEditor.cs` - 自定义 Inspector

---

## 🔍 技术细节

### **`#if UNITY_EDITOR` 的作用**

```csharp
#if UNITY_EDITOR
using UnityEditor;  // ← 编辑器专用命名空间

[ExecuteInEditMode]
public class TileAuthoring : MonoBehaviour {
    // 编辑器专用代码
}
#endif
```

**效果**：
- ✅ **编辑器模式**：完整编译和运行
- ✅ **运行模式（Play）**：完整编译和运行（因为在 Runtime/ 文件夹）
- ✅ **构建（Build）**：整个文件被条件编译排除，不包含在最终包中

### **Unity 的编译顺序**

```
1. Runtime/ 文件夹（标准编译）
   ↓
2. Editor/ 文件夹（编辑器编译）
   ↓
3. 链接阶段（Editor 脚本不可引用 Runtime 的 MonoBehaviour 作为组件）
```

---

## ✅ 验证清单

修复后，确保以下功能正常：

- [x] `TileAuthoring` 可以通过 `AddComponent<>()` 添加
- [x] Scene 视图中格子正常显示材质
- [x] Inspector 可以修改 `TileAuthoring` 属性
- [x] 修改后实时同步到 `LevelConfig`
- [x] 笔刷工具可以正常绘制
- [x] 不会出现 NullReferenceException
- [x] 构建时不包含 `TileAuthoring`（通过条件编译排除）

---

## 📝 经验总结

### **设计原则**

1. **MonoBehaviour 组件**：
   - 永远不要放在 `Editor/` 文件夹
   - 如果仅编辑器使用，放在 `Runtime/` + 条件编译

2. **纯编辑器工具**：
   - `EditorWindow`、`Editor`、`PropertyDrawer` 等放在 `Editor/` 文件夹
   - 不需要条件编译（Unity 自动处理）

3. **文件夹命名**：
   - `Runtime/EditorTools/` 明确表示"运行时文件夹中的编辑器工具"
   - 避免歧义，见名知义

### **常见错误**

❌ **错误做法 1**：把 MonoBehaviour 放在 Editor/ 文件夹
```
Assets/Scripts/Editor/MyComponent.cs  ← ❌ 无法附加到 GameObject
```

❌ **错误做法 2**：编辑器工具不使用条件编译
```csharp
// Runtime/EditorTools/Tool.cs
using UnityEditor;  // ← ❌ 构建时会报错（没有 #if UNITY_EDITOR）
```

✅ **正确做法**：
```
Assets/Scripts/Runtime/EditorTools/MyComponent.cs  ← ✅ 可附加，构建排除
```
```csharp
#if UNITY_EDITOR
using UnityEditor;
public class MyComponent : MonoBehaviour { }
#endif
```

---

## 📖 参考资料

### Unity 官方文档
- [Special folder names](https://docs.unity3d.com/Manual/SpecialFolders.html)
- [Script Compilation](https://docs.unity3d.com/Manual/ScriptCompileOrderFolders.html)
- [Platform dependent compilation](https://docs.unity3d.com/Manual/PlatformDependentCompilation.html)

### 最佳实践
- 编辑器扩展应遵循 Unity 的文件夹约定
- 使用条件编译避免构建冗余代码
- 文件夹结构应清晰表达代码的用途和生命周期

---

**总结**：`TileAuthoring` 是 MonoBehaviour 组件，虽然仅在编辑器使用，但必须放在 `Runtime/` 文件夹中，并使用 `#if UNITY_EDITOR` 条件编译来排除构建。这是 Unity 架构的基本规则，也是保证项目健壮性的关键。
