# Level Editor Workspace Productized Rollout Notes

## 目标

本轮交付把 Unity 关卡编辑器收敛为以 `Workspace` 为编辑期真源、以白模为主要交互载体、以 `<ExportName>_LevelConfig` 为运行时产物的完整工作流。

## 当前工作流

1. 打开 `Level Editor` 窗口。
2. 新建或打开 `Workspace` 资源。
3. 在生成白模前先编辑基础参数：
   - `Workspace Name`
   - `Export Name`
   - `MapWidth`
   - `MapDepth`
   - `CellSize`
   - 运行时参数
4. 点击生成白模，在 SceneView 中进入编辑。
5. 使用统一的 `地块类型` 笔刷编辑地图主类型：
   - 禁用
   - 地面
   - 高台
   - 坑洞
   - 出生点
   - 目标点
   - 传送入口
   - 传送出口
6. `路径` 保持独立模块，不并入主类型笔刷。
7. 保存 `Workspace`。
8. 导出 `Assets/Resources/Levels/Configs/<ExportName>_LevelConfig.asset`。

## 已落实规则

- `Workspace` 默认名称保持英文，默认值为 `NewLevel`。
- 新建白模后全图默认主类型为 `禁用`，不再默认整图地面。
- 每个格子同一时刻只保留一个主类型，后刷覆盖前刷。
- 普通地形、出生点、目标点、传送点属于同级主类型。
- 出生点、目标点、传送点都有显式 ID：
  - `R1 / R2 / ...`
  - `B1 / B2 / ...`
  - `IN1 / OUT1 / IN2 / OUT2 / ...`
- 删除后不重排旧 ID，新对象始终使用当前最大编号加一。
- 白模地面颜色直接表达最终主类型，不再只靠辅助球体或方框区分。
- 辅助标记仍然保留，但底色也会同步变化。

## 保存、重开与导出

- `Workspace` 资源保存在 `Assets/Editor/LevelEditor/Workspaces/`。
- 重开 `Workspace` 后，地图尺寸、运行时参数、语义点、传送点、波次和导出命名都会恢复。
- 导出时会把出生点、目标点、传送点所在格子显式写入 `specialTiles`。
- 导出与校验不再依赖任何隐式 spawn/goal fallback。
- 运行时 `LevelConfig` 也不再把 spawn/goal 位置隐式视为 `Ground`。

## 验收重点

- 打开 Unity 后，`Level Editor` 中可以直接看到最新中文化后的编辑器文案。
- 生成白模前可以直接修改 `MapWidth`、`MapDepth`、`CellSize`。
- 地图默认全禁用，需要手刷通路。
- 出生点、目标点、传送点会改变地块底色，并显示稳定 ID。
- 用普通地形覆盖出生点、目标点、传送点后，旧语义会被真正移除。
- 波次只允许引用地图中真实存在的 `R* / B*`。
- 导出后的 `LevelConfig` 必须依赖显式地块数据，不能依赖隐式默认点位。
