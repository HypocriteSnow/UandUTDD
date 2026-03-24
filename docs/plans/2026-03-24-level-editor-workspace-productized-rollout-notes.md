# Level Editor Workspace Productized Rollout Notes

## 目标

本次交付把 Unity 关卡编辑器收敛为以 `Workspace` 为编辑真源、以白模为主要交互载体、以 `<ExportName>_LevelConfig` 为运行时产物的完整工作流。

## 用户工作流

1. 打开 `Level Editor` 窗口。
2. 新建 Workspace，或打开已有 Workspace 资产。
3. 填写并维护：
   - `Workspace Name`
   - `Export Name`
   - 地图尺寸、格子大小、运行时参数
4. 点击生成白模，在 SceneView 中开始编辑。
5. 在白模上放置并调整语义对象：
   - 红门 `R1/R2/...`
   - 蓝门 `B1/B2/...`
   - 传送门 `IN1/OUT1`、`IN2/OUT2`...
6. 在波次面板里为每个 wave 选择 `spawnId` 与 `targetId`。
7. 先使用 A* 自动生成路径，再按需对白模中的当前 wave 路径做手工补点或修正。
8. 点击保存，将当前编辑状态写回 Workspace 资产。
9. 重新打开该 Workspace 时，语义标记、波次引用、路径、运行时参数与导出命名应被完整恢复。
10. 点击导出，生成或覆盖 `Assets/Resources/Levels/Configs/<ExportName>_LevelConfig.asset`。

## 保存与恢复

- Workspace 资产保存目录为 `Assets/Editor/LevelEditor/Workspaces/`。
- `Workspace Name` 用于 Workspace 资产命名。
- `Export Name` 用于运行时导出资源命名；为空时回退到 `Workspace Name`。
- 白模不是真源，重新生成或重新打开后应始终从当前 Workspace 状态恢复。

## 导出与校验

- 导出前先校验 Workspace 本身，再校验临时构建出的 `LevelConfig`。
- 校验失败时阻止导出，并在编辑器中展示明确错误。
- 当目标 `LevelConfig` 已存在时，编辑器会弹出覆盖确认。
- 导出不会覆盖 Workspace 资产本身。

## 本轮重点覆盖

- 保存后的 Workspace 可以重新打开并直接导出，不需要手工修补引用。
- 重新打开后的白模会正确渲染语义门与传送门标签。
- 第一次从 legacy spawn/goal fallback 迁移到语义 marker 时，旧 fallback tile 的白模视觉会被正确清除。
- 重新打开后的 wave 仍可基于持久化语义 ID 与传送门图进行寻路与导出。
