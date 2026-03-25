namespace ArknightsLite.Editor.LevelEditor.Core {
    using ArknightsLite.Model;

    public static class LevelEditorText {
        public static class Window {
            public const string MenuPath = "ArknightsLite/关卡编辑器";
            public const string Title = "关卡编辑器";

            public const string WorkspaceSectionTitle = "Workspace";
            public const string WorkspaceNameLabel = "Workspace 名称";
            public const string ExportNameLabel = "导出名称";
            public const string MapWidthLabel = "地图宽度";
            public const string MapDepthLabel = "地图深度";
            public const string CellSizeLabel = "格子尺寸";
            public const string SyncNamingButton = "同步命名";
            public const string NewWorkspaceButton = "新建 Workspace";
            public const string OpenWorkspaceButton = "打开 Workspace";
            public const string SaveWorkspaceButton = "保存 Workspace";
            public const string GenerateWhiteboxButton = "生成/刷新白模";
            public const string ExportLevelConfigButton = "导出 LevelConfig";
            public const string EmptyWorkspaceHelp = "请先配置基础参数，创建 Workspace 后再生成白模。";

            public const string ExportInfoSectionTitle = "导出信息";
            public const string ExportInfoEmptyHelp = "请先创建或打开 Workspace 以查看导出信息。";
            public const string ExportInfoNotice = "Workspace 是编辑阶段的唯一数据来源，LevelConfig 仅在导出时生成。";

            public const string WorkspaceModulesTitle = "Workspace 模块";
            public static readonly string[] WorkspaceModuleTabs = { "地图", "传送门", "路径", "波次" };
            public const string MapEditingHelp = "请使用上方白模和场景工具编辑地图。";

            public const string EditingSectionTitle = "编辑控制";
            public const string EditModeHelp = "当前已进入场景编辑模式，可使用上方白模工具编辑地形、语义点、传送门和路径。";
            public const string SaveAndLeaveEditModeButton = "保存 Workspace 并退出编辑模式";
            public const string RegenerateWhiteboxButton = "按 Workspace 重新生成白模";
            public const string EnterWhiteboxEditModeButton = "进入白模编辑模式";

            public const string BrushSectionTitle = "笔刷工具";
            public const string EnableBrushLabel = "启用笔刷";
            public static readonly string[] BrushToolTabs = { "地块类型", "路径" };
            public const string BrushHelp = "先选择地块类型，再在场景视图中点击或拖拽绘制。\n快捷键：1-地面 2-高台 3-禁用 4-坑洞 5-出生点 6-目标点 7-传送入口 8-传送出口";
            public const string BrushTypeLabel = "地块类型";
            public const string BrushHeightLabel = "笔刷高度";
            public const string GroundButton = "地面 (1)";
            public const string HighGroundButton = "高台 (2)";
            public const string ForbiddenButton = "禁用 (3)";
            public const string HoleButton = "坑洞 (4)";
            public const string SpawnButton = "出生点 (5)";
            public const string GoalButton = "目标点 (6)";
            public const string PortalEntranceButton = "传送入口 (7)";
            public const string PortalExitButton = "传送出口 (8)";

            public const string StatusSectionTitle = "状态信息";
            public const string NoWorkspaceLoadedHelp = "当前未加载 Workspace。";

            public const string MissingWorkspaceTitle = "缺少 Workspace";
            public const string ErrorDialogTitle = "错误";
            public const string ConfirmDialogTitle = "确认";
            public const string SuccessDialogTitle = "成功";
            public const string OkButton = "确定";
            public const string CancelButton = "取消";
            public const string OverwriteButton = "覆盖";
            public const string RegenerateButton = "重新生成";

            public const string RegenerateWhiteboxConfirmMessage = "是否按当前 Workspace 状态重新生成白模？";
            public const string MissingWorkspaceBeforeEditMessage = "进入编辑模式前请先创建或打开 Workspace。";
            public const string ExitEditModeSavedMessage = "Workspace 修改已保存，之后可以继续编辑。";
            public const string EmptyWorkspaceNameMessage = "Workspace 名称不能为空。";
            public const string OpenWorkspacePathErrorMessage = "选中的文件必须位于项目 Assets 目录下，才能作为 Workspace 打开。";
            public const string LoadWorkspaceFailedMessage = "加载所选 Workspace 资源失败。";
            public const string SaveWorkspaceMissingMessage = "请先创建或打开 Workspace，再保存 Workspace 资源。";
            public const string MissingWorkspaceMessage = "请先创建或打开 Workspace。";
            public const string WorkspaceValidationFailedTitle = "Workspace 校验失败";
            public const string ExportValidationFailedTitle = "导出校验失败";
            public const string OverwriteExportTitle = "确认覆盖";
            public const string NoneValue = "<无>";

            public const string SpawnHelp = "在场景视图中点击格子放置出生点。快捷键：5";
            public const string GoalHelp = "在场景视图中点击格子放置目标点。快捷键：6";
            public const string PortalEntranceHelp = "在场景视图中点击格子记录下一组传送入口。快捷键：7";
            public const string PortalExitHelp = "在场景视图中点击格子完成当前待设置的传送门配对。快捷键：8";
            public const string PathEditHelp = "在场景视图中点击格子，为当前波次添加或移除路径节点。快捷键：9";

            public const string PortalExitRequiresEntranceWarning = "[LevelEditor] 请先选择传送入口，再放置传送出口。";
            public const string PortalExitMustUseDifferentTileWarning = "[LevelEditor] 传送入口和传送出口必须位于不同格子。";
            public const string UndoPlaceSpawnMarker = "放置 Workspace 出生点";
            public const string UndoPlaceGoalMarker = "放置 Workspace 目标点";
            public const string UndoPlacePortalPair = "放置 Workspace 传送门";
            public const string UndoPaintTile = "绘制 Workspace 格子";

            public static string CurrentWorkspaceSummary(LevelEditorWorkspace workspace) {
                return $"当前 Workspace: {workspace.LevelName} ({workspace.MapWidth}x{workspace.MapDepth}，格子 {workspace.CellSize:0.##})";
            }

            public static string ExportAssetLabel(string assetName) {
                return $"导出资源: {assetName}";
            }

            public static string LastExportLabel(string assetName) {
                return $"最近导出: {assetName}";
            }

            public static string SpawnMarkersLabel(int count) {
                return $"出生点标记数: {count}";
            }

            public static string GoalMarkersLabel(int count) {
                return $"目标点标记数: {count}";
            }

            public static string PortalPairsLabel(int count) {
                return $"传送门对数: {count}";
            }

            public static string PendingPortalEntranceLabel(string value) {
                return $"待设置传送入口: {value}";
            }

            public static string SelectedWaveLabel(string value) {
                return $"当前选中波次: {value}";
            }

            public static string CurrentWorkspaceLabel(string name) {
                return $"当前 Workspace: {name}";
            }

            public static string MapSizeLabel(int width, int depth) {
                return $"地图尺寸: {width}x{depth}";
            }

            public static string CellSizeValueLabel(float cellSize) {
                return $"格子尺寸: {cellSize:0.##}";
            }

            public static string CurrentModeLabel(LevelEditorMode mode) {
                return $"当前模式: {mode}";
            }

            public static string WhiteboxTilesLabel(int count) {
                return $"白模格子数: {count}";
            }

            public static string OverwriteExportMessage(string assetName) {
                return $"是否覆盖 {assetName}？";
            }

            public static string ExportSuccessMessage(string assetName) {
                return $"已导出 {assetName}";
            }

            public static string NoWorkspaceOverlay(TileType tileType) {
                return $"工具: 地块类型 / {GetTileTypeLabel(tileType)}\n快捷键: 1-8 地块类型";
            }

            public const string SpawnOverlay = "工具: 出生点\n点击放置出生点标记\n快捷键: 5";
            public const string GoalOverlay = "工具: 目标点\n点击放置目标点标记\n快捷键: 6";

            public static string PortalEntranceOverlay(string pendingValue) {
                return $"工具: 传送入口\n待设置: {pendingValue}\n快捷键: 7";
            }

            public static string PortalExitOverlay(string pendingValue) {
                return $"工具: 传送出口\n待设置: {pendingValue}\n快捷键: 8";
            }

            public static string PathOverlay(string waveId) {
                return $"工具: 路径\n点击为 {waveId} 添加/移除节点\n快捷键: 9";
            }

            public static string TerrainOverlay(TileType tileType, int brushHeight) {
                return $"工具: 地块类型 / {GetTileTypeLabel(tileType)}\n高度: {brushHeight}\n快捷键: 1-8 地块类型，9 路径";
            }

            private static string GetTileTypeLabel(TileType tileType) {
                switch (tileType) {
                    case TileType.HighGround:
                        return "高台";
                    case TileType.Forbidden:
                        return "禁用";
                    case TileType.Hole:
                        return "坑洞";
                    default:
                        return "地面";
                }
            }
        }

        public static class RuntimePanel {
            public const string SectionTitle = "关卡运行时参数";
            public const string EmptyHelp = "创建 Workspace 后可直接编辑初始 DP、基地生命和回费参数。";
            public const string InitialDpLabel = "初始 DP";
            public const string BaseHealthLabel = "基地生命";
            public const string DpRecoveryIntervalLabel = "回费间隔";
            public const string DpRecoveryAmountLabel = "单次回费";
        }

        public static class PortalPanel {
            public const string SectionTitle = "传送门";
            public const string EmptyHelp = "创建 Workspace 后可编辑传送门。";
            public const string AddButton = "添加传送门";
            public const string EntranceLabel = "入口";
            public const string ExitLabel = "出口";
            public const string DelayLabel = "延迟";
            public const string ColorLabel = "颜色";
            public const string DeleteButton = "删除传送门";
        }

        public static class PathPanel {
            public const string SectionTitle = "路径";
            public const string EmptyWorkspaceHelp = "请先创建 Workspace 后再编辑路径。";
            public const string EmptyWaveHelp = "请先创建并选中波次，再编辑路径。";
            public const string GenerateButton = "自动生成路径";
            public const string ClearButton = "清空路径";
            public const string AddNodeButton = "添加路径节点";
            public const string WaitLabel = "等待时间";
            public const string DeleteButton = "删除";

            public static string CurrentWaveLabel(string waveId) {
                return $"当前波次: {waveId}";
            }
        }

        public static class WavePanel {
            public const string SectionTitle = "波次";
            public const string EmptyWorkspaceHelp = "请先创建 Workspace 后再编辑波次。";
            public const string AddButton = "添加波次";
            public const string EmptyHelp = "当前还没有波次。";
            public const string CurrentWaveLabel = "当前波次";
            public const string WaveIdLabel = "波次 ID";
            public const string EnemyIdLabel = "敌人 ID";
            public const string StartTimeLabel = "开始时间";
            public const string CountLabel = "数量";
            public const string IntervalLabel = "间隔";
            public const string SpawnIdLabel = "出生点 ID";
            public const string TargetIdLabel = "目标点 ID";
            public const string DeleteButton = "删除当前波次";

            public static string PathNodesLabel(int count) {
                return $"路径节点数: {count}";
            }
        }

        public static class TileInspector {
            public const string SectionTitle = "格子信息";
            public const string WhiteboxDrivenHelp = "该格子由 Workspace 白模驱动。";
            public const string DataSectionTitle = "格子数据";
            public const string TileTypeLabel = "地块类型";
            public const string HeightLevelLabel = "高度层级";
            public const string WalkableLabel = "可通行";
            public const string DeployTagLabel = "部署标签";

            public static string GridLabel(int x, int z) {
                return $"网格坐标: ({x}, {z})";
            }

            public static string SemanticLabelsLabel(string label) {
                return $"语义标签: {label}";
            }

            public static string SpawnMarkerLabel(string markerId) {
                return $"出生点标记: {markerId}";
            }

            public static string GoalMarkerLabel(string markerId) {
                return $"目标点标记: {markerId}";
            }
        }
    }
}
