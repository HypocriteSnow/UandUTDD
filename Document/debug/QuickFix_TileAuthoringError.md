# 快速修复指南：TileAuthoring 组件错误

## 问题症状

```
白色提示：Can't add script behaviour 'TileAuthoring' because it is an editor script.
NullReferenceException at LevelEditorWindow.cs:239
```

## 原因

`TileAuthoring.cs` 被错误地放在了 `Editor/` 文件夹中，Unity 不允许 Editor 文件夹内的脚本作为 MonoBehaviour 组件附加到 GameObject。

## ✅ 已修复

文件已从 `Assets/Scripts/Editor/Authoring/` 移动到 `Assets/Scripts/Runtime/EditorTools/`

## 验证步骤

1. **确认文件位置**：
   ```
   ✅ Assets/Scripts/Runtime/EditorTools/TileAuthoring.cs
   ❌ Assets/Scripts/Editor/Authoring/TileAuthoring.cs（已删除）
   ```

2. **重新打开关卡编辑器**：
   - `ArknightsLite → Level Editor`
   - 拖入 `LevelConfig` 和 `GridVisualConfig`
   - 点击"进入编辑模式"

3. **预期结果**：
   - ✅ Scene 视图中生成网格
   - ✅ 每个格子都有 `TileAuthoring` 组件
   - ✅ 无错误提示
   - ✅ Inspector 可以修改格子属性

## Unity 编辑器操作

如果遇到编译错误：

1. **刷新 Unity**：`Ctrl + R` 或 `Assets → Refresh`
2. **重新导入脚本**：右键 `TileAuthoring.cs` → `Reimport`
3. **清理并重新编译**：`Assets → Reimport All`

## 技术说明

详见：`Document/TechNote_UnityEditorFolderRules.md`

**核心原则**：
- MonoBehaviour 组件 → Runtime/ 文件夹
- 纯编辑器工具（EditorWindow, Inspector）→ Editor/ 文件夹
