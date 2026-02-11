# 修复日志：TileAuthoring 组件错误

**日期**: 2026-02-11  
**问题**: TileAuthoring 无法附加到 GameObject  
**严重性**: 🔴 阻塞性错误（关卡编辑器无法使用）  
**状态**: ✅ 已修复

---

## 📋 问题描述

### 错误信息

**警告（白色提示）**：
```
Can't add script behaviour 'TileAuthoring' because it is an editor script. 
To attach a script it needs to be outside the 'Editor' folder.
UnityEngine.GameObject:AddComponent<TileAuthoring>()
LevelEditorWindow:CreateTileAuthoring(int,int) (at Assets/Scripts/Editor/LevelEditorWindow.cs:238)
```

**异常（红色错误）**：
```
NullReferenceException: Object reference not set to an instance of an object
LevelEditorWindow.CreateTileAuthoring (line 239)
```

### 触发条件

1. 打开 `ArknightsLite → Level Editor`
2. 配置 `LevelConfig` 和 `GridVisualConfig`
3. 点击"进入编辑模式"
4. **结果**：Scene 视图无网格，Console 报错

---

## 🔍 根因分析

### 问题根源：违反 Unity Editor 文件夹规则

**错误路径**：
```
❌ Assets/Scripts/Editor/Authoring/TileAuthoring.cs
```

**Unity 规则**：
- `Editor/` 文件夹内的脚本会被标记为"纯编辑器代码"
- 这些脚本**不能**作为 MonoBehaviour 组件附加到 GameObject
- 即使使用 `#if UNITY_EDITOR`，也无法绕过此限制

### 错误链路

```
LevelEditorWindow.EnterEditMode()
  → GenerateEditGrid()
    → CreateTileAuthoring(x, z)
      → AddComponent<TileAuthoring>()  ← Unity 拒绝（返回 null）
      → authoring.Initialize(...)       ← NullReferenceException
```

### 为什么会出现这个问题？

**设计误区**：错误地将 MonoBehaviour 组件放入 `Editor/` 文件夹

- `TileAuthoring` 是 `MonoBehaviour` 组件
- 需要附加到场景 GameObject 上
- 虽然只在编辑器模式下使用，但它仍是**场景组件**，不是**纯编辑器工具**

**正确理解**：
- **Editor/ 文件夹**：用于 `EditorWindow`、`Editor`、`PropertyDrawer` 等不需要附加到对象的工具
- **Runtime/ + #if UNITY_EDITOR**：用于编辑器模式下的 MonoBehaviour 组件

---

## ✅ 修复方案

### 1. 文件重定位

**操作**：将 `TileAuthoring.cs` 移出 `Editor/` 文件夹

```diff
- Assets/Scripts/Editor/Authoring/TileAuthoring.cs
+ Assets/Scripts/Runtime/EditorTools/TileAuthoring.cs
```

**新文件夹**：`Assets/Scripts/Runtime/EditorTools/`
- 用途：存放编辑器模式下的 MonoBehaviour 组件
- 特征：`#if UNITY_EDITOR` + `[ExecuteInEditMode]`

### 2. 代码保持不变

**`TileAuthoring.cs` 内容无需修改**：
- ✅ 仍然使用 `#if UNITY_EDITOR` 条件编译
- ✅ 仍然是 MonoBehaviour 组件
- ✅ 仍然使用 `[ExecuteInEditMode]`
- ✅ 构建时仍会被排除（条件编译）

**关键**：只是**物理位置**改变，代码逻辑完全不变

### 3. 文档同步更新

更新了以下文档中的路径引用：

| 文档 | 更新内容 |
|------|---------|
| `Document/Summary_LevelEditor.md` | 文件清单路径 |
| `Document/START_HERE.md` | 文件结构说明 |
| `Document/README_GridSystem.md` | Editor 工具表格 |

新增技术文档：

| 文档 | 用途 |
|------|------|
| `Document/TechNote_UnityEditorFolderRules.md` | Unity 文件夹规则详解 |
| `Document/QuickFix_TileAuthoringError.md` | 快速修复指南 |
| `Document/FIXLOG_TileAuthoringError_20260211.md` | 本文档（修复日志） |

---

## 🧪 验证测试

### 测试步骤

1. **Unity 刷新**：`Ctrl + R` 确保文件变更生效
2. **打开关卡编辑器**：`ArknightsLite → Level Editor`
3. **配置资源**：
   - Level Config：拖入现有 `LevelConfig`
   - Grid Visual Config：拖入 `DefaultVisual`
4. **进入编辑模式**：点击按钮
5. **验证结果**：
   - ✅ Scene 视图生成网格
   - ✅ 每个格子都有 `TileAuthoring` 组件
   - ✅ Inspector 显示格子属性
   - ✅ Console 无错误

### 预期行为

**场景层级**（编辑模式）：
```
Hierarchy
└── [EDIT MODE] Level_TestLevel01
    ├── Tile_0_0 (TileAuthoring)
    ├── Tile_0_1 (TileAuthoring)
    ├── ...
    └── Tile_14_14 (TileAuthoring)
```

**组件检查**：
```
Tile_0_0
├── Transform
├── Mesh Renderer
└── TileAuthoring  ← ✅ 组件成功附加
    ├── Tile Type: Forbidden
    ├── Height Level: 0
    ├── Walkable: false
    └── Deploy Tag: All
```

---

## 📊 影响范围

### 直接影响

✅ **已修复功能**：
- 关卡编辑器窗口正常运行
- TileAuthoring 组件可正常附加
- 场景网格正常生成
- Inspector 编辑功能恢复
- 笔刷工具可用

❌ **无破坏性变更**：
- 代码逻辑零修改
- 现有配置资产无需重新生成
- 其他系统无影响

### 架构改进

✅ **结构优化**：
```
Before (错误结构):
Editor/
  ├── LevelEditorWindow.cs     ← EditorWindow（✅ 正确）
  └── Authoring/
      └── TileAuthoring.cs      ← MonoBehaviour（❌ 错误）

After (正确结构):
Editor/
  ├── LevelEditorWindow.cs     ← EditorWindow（✅ 正确）
  └── LevelConfigEditor.cs     ← CustomEditor（✅ 正确）
Runtime/
  └── EditorTools/
      └── TileAuthoring.cs      ← MonoBehaviour + #if（✅ 正确）
```

---

## 📚 经验总结

### 设计原则

1. **MonoBehaviour 放置规则**：
   - 永远不要放在 `Editor/` 文件夹
   - 如果仅编辑器使用 → `Runtime/EditorTools/` + `#if UNITY_EDITOR`
   - 如果运行时也用 → `Runtime/` 或 `Core/View/`

2. **Editor 文件夹专用**：
   - `EditorWindow` - 编辑器窗口
   - `Editor` - 自定义 Inspector
   - `PropertyDrawer` - 属性绘制器
   - `[MenuItem]` - 菜单项

3. **条件编译 vs. 文件夹**：
   - `#if UNITY_EDITOR`：控制代码是否包含在构建中
   - `Editor/` 文件夹：控制脚本能否作为组件附加
   - **两者作用不同，不可互相替代**

### 常见误区

❌ **误区 1**：以为 `#if UNITY_EDITOR` 可以在任何位置排除代码
- **真相**：条件编译只影响构建，不影响 Unity 编辑器对文件夹的识别

❌ **误区 2**：编辑器专用组件应该放在 `Editor/` 文件夹
- **真相**：MonoBehaviour 组件必须在 Runtime 区域，用条件编译排除构建

❌ **误区 3**：`[ExecuteInEditMode]` 脚本属于 Editor 范畴
- **真相**：它只是让组件在编辑模式下运行，仍是场景组件

### 最佳实践

✅ **推荐做法**：
```
Runtime/EditorTools/  ← 编辑器模式 MonoBehaviour
  ├── TileAuthoring.cs         ← #if UNITY_EDITOR
  ├── PathPreviewComponent.cs  ← #if UNITY_EDITOR
  └── DebugVisualizerComponent.cs

Editor/  ← 纯编辑器工具
  ├── LevelEditorWindow.cs     ← EditorWindow
  ├── TileAuthoringEditor.cs   ← CustomEditor（如需自定义 Inspector）
  └── GridSetupHelper.cs       ← EditorWindow（工具）
```

---

## 🔗 相关资源

### 文档链接

- **技术详解**：`Document/TechNote_UnityEditorFolderRules.md`
- **快速修复**：`Document/QuickFix_TileAuthoringError.md`
- **关卡编辑器**：`Document/Guide_LevelEditorUsage.md`
- **系统概览**：`Document/README_GridSystem.md`

### Unity 官方文档

- [Special folder names](https://docs.unity3d.com/Manual/SpecialFolders.html)
- [Script Compilation Order](https://docs.unity3d.com/Manual/ScriptCompileOrderFolders.html)
- [Platform Dependent Compilation](https://docs.unity3d.com/Manual/PlatformDependentCompilation.html)

---

## ✅ 结论

**问题类型**：架构设计错误（文件放置不当）  
**修复难度**：简单（仅需移动文件）  
**影响范围**：关卡编辑器功能  
**是否可避免**：是（遵循 Unity 文件夹规范）

**关键教训**：
> Unity 的 `Editor/` 文件夹规则是**强制性的**，不是建议。MonoBehaviour 组件必须在 Runtime 区域，无论它是否仅在编辑器使用。条件编译 `#if UNITY_EDITOR` 负责构建排除，文件夹位置负责组件附加权限，两者职责不同。

**后续建议**：
1. 更新开发规范文档，明确文件夹使用规则
2. Code Review 时检查 MonoBehaviour 放置位置
3. 项目模板中预建 `Runtime/EditorTools/` 文件夹

---

**修复人员**: Assistant  
**审核状态**: ✅ 已完成  
**向后兼容**: ✅ 完全兼容  
**文档更新**: ✅ 已同步  
