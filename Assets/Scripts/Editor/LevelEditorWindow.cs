using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using ArknightsLite.Config;
using ArknightsLite.Model;
using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Map;
using ArknightsLite.Editor.LevelEditor.Panels;
using ArknightsLite.Editor.LevelEditor.Services;

/// <summary>
/// 关卡编辑器窗口
/// 职责：生成可编辑的网格、提供笔刷工具、管理编辑会话
/// </summary>
public class LevelEditorWindow : EditorWindow {
    private enum WorkspaceSceneTool {
        Terrain,
        Spawn,
        Goal,
        PortalEntrance,
        PortalExit,
        PathEdit
    }
    
    // ==================== 配置引用 ====================
    
    private LevelConfig _config;
    private GridVisualConfig _visualConfig;
    
    
    // ==================== 编辑状态 ====================
    
    private GameObject _gridParent;
    private WhiteboxRoot _whiteboxRoot;
    private WorkspaceMapController _workspaceMapController;
    private bool _isEditMode = false;
    private int _workspaceToolTab = 0;
    private int _selectedWaveIndex = -1;
    
    
    // ==================== 笔刷设置 ====================
    
    private bool _brushEnabled = false;
    private TileType _brushType = TileType.Ground;
    private int _brushHeight = 0;
    private bool _isPainting = false;
    private WorkspaceSceneTool _workspaceSceneTool = WorkspaceSceneTool.Terrain;
    private Vector2Int? _pendingPortalEntrancePosition;
    
    
    // ==================== 关卡管理 ====================
    
    private string _newLevelName = "NewLevel";
    private readonly LevelEditorSession _session = new LevelEditorSession();
    
    
    // ==================== 窗口管理 ====================
    
    [MenuItem("ArknightsLite/Level Editor")]
    public static void ShowWindow() {
        var window = GetWindow<LevelEditorWindow>("关卡编辑器");
        window.minSize = new Vector2(300, 400);
    }
    
    private void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    private void OnDisable() {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    
    // ==================== UI 绘制 ====================
    
    private void OnGUI() {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("关卡编辑器", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        DrawLevelManagementSection();
        EditorGUILayout.Space(10);

        DrawRuntimeSection();
        EditorGUILayout.Space(10);
        
        DrawConfigSection();
        EditorGUILayout.Space(10);
        
        DrawEditSection();
        EditorGUILayout.Space(10);
        
        DrawBrushSection();
        EditorGUILayout.Space(10);

        DrawWorkspaceModulesSection();
        EditorGUILayout.Space(10);
        
        DrawInfoSection();
    }
    
    /// <summary>
    /// 关卡管理区域
    /// </summary>
    private void DrawLevelManagementSection() {
        EditorGUILayout.LabelField("工作区", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        _newLevelName = EditorGUILayout.TextField("关卡名称", _newLevelName);
        if (GUILayout.Button("新建工作区", GUILayout.Width(80))) {
            EditorApplication.delayCall += CreateWorkspace;
        }
        EditorGUILayout.EndHorizontal();

        var workspace = _session.CurrentWorkspace;
        bool hasWorkspace = workspace != null;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Open Workspace", GUILayout.Width(120))) {
            EditorApplication.delayCall += OpenWorkspace;
        }

        GUI.enabled = hasWorkspace;
        if (GUILayout.Button("Save Workspace", GUILayout.Width(120))) {
            EditorApplication.delayCall += SaveWorkspace;
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        if (hasWorkspace) {
            EditorGUI.BeginChangeCheck();
            string nextLevelName = EditorGUILayout.TextField("Workspace Name", workspace.LevelName);
            string nextExportName = EditorGUILayout.TextField("Export Name", workspace.ExportName);
            if (EditorGUI.EndChangeCheck()) {
                workspace.LevelName = nextLevelName;
                workspace.ExportName = nextExportName;
                _newLevelName = nextLevelName;
                MarkCurrentWorkspaceDirty();
            }

            if (GUILayout.Button("Sync Naming", GUILayout.Width(120))) {
                SyncWorkspaceNaming();
            }
        }

        if (hasWorkspace) {
            EditorGUILayout.HelpBox($"当前工作区: {workspace.LevelName} ({workspace.MapWidth}x{workspace.MapDepth})", MessageType.Info);
        } else {
            EditorGUILayout.HelpBox("先创建工作区，再生成/刷新白模。", MessageType.Info);
        }

        GUI.enabled = hasWorkspace;
        if (GUILayout.Button("生成/刷新白模", GUILayout.Height(30))) {
            EditorApplication.delayCall += GenerateWhiteboxFromWorkspace;
        }
        if (GUILayout.Button("导出 LevelConfig", GUILayout.Height(30))) {
            EditorApplication.delayCall += ExportCurrentWorkspace;
        }
        GUI.enabled = true;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("旧配置兼容", EditorStyles.miniBoldLabel);
        
        bool hasConfigs = (_config != null && _visualConfig != null);
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = hasConfigs;
        if (GUILayout.Button("构建场景", GUILayout.Height(30))) {
            EditorApplication.delayCall += BuildScene;
        }
        GUI.enabled = true;
        
        if (GUILayout.Button("从场景加载", GUILayout.Height(30))) {
            EditorApplication.delayCall += LoadFromScene;
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// 配置区域
    /// </summary>
    private void DrawConfigSection() {
        EditorGUILayout.LabelField("运行时配置兼容", EditorStyles.boldLabel);

        var nextConfig = (LevelConfig)EditorGUILayout.ObjectField("Level Config", _config, typeof(LevelConfig), false);
        if (nextConfig != _config) {
            _config = nextConfig;
            _session.SetCurrentLevel(_config);
        }

        _visualConfig = (GridVisualConfig)EditorGUILayout.ObjectField("Visual Config", _visualConfig, typeof(GridVisualConfig), false);
        
        if (_session.CurrentWorkspace != null) {
            EditorGUILayout.LabelField($"Spawn: {_session.CurrentWorkspace.SpawnId} @ {_session.CurrentWorkspace.GetResolvedSpawnPoint()}");
            EditorGUILayout.LabelField($"Goal: {_session.CurrentWorkspace.GoalId} @ {_session.CurrentWorkspace.GetResolvedGoalPoint()}");
            EditorGUILayout.HelpBox("当前以 Workspace 驱动白模；这里保留旧 LevelConfig/GridVisualConfig 的兼容入口。", MessageType.Info);
        } else if (_config == null || _visualConfig == null) {
            EditorGUILayout.HelpBox("如需走旧流程，请拖入 LevelConfig 和 GridVisualConfig。", MessageType.Warning);
        }
    }

    /// <summary>
    /// 运行时参数区域
    /// </summary>
    private void DrawRuntimeSection() {
        EditorGUI.BeginChangeCheck();
        LevelRuntimePanel.Draw(_session.CurrentWorkspace);
        if (EditorGUI.EndChangeCheck()) {
            MarkCurrentWorkspaceDirty();
        }
    }

    /// <summary>
    /// Workspace 模块区域
    /// </summary>
    private void DrawWorkspaceModulesSection() {
        if (_session.CurrentWorkspace == null) {
            return;
        }

        EditorGUILayout.LabelField("工作区模块", EditorStyles.boldLabel);
        _workspaceToolTab = GUILayout.Toolbar(_workspaceToolTab, new[] { "地图", "传送门", "路径", "波次" });

        EditorGUI.BeginChangeCheck();
        switch (_workspaceToolTab) {
            case 0:
                EditorGUILayout.HelpBox("地图编辑使用上方白模与笔刷工具。", MessageType.Info);
                break;
            case 1:
                PortalEditorPanel.Draw(_session.CurrentWorkspace);
                break;
            case 2:
                PathEditorPanel.Draw(_session.CurrentWorkspace, _selectedWaveIndex);
                break;
            case 3:
                _selectedWaveIndex = WaveEditorPanel.Draw(_session.CurrentWorkspace, _selectedWaveIndex);
                break;
        }

        if (EditorGUI.EndChangeCheck()) {
            MarkCurrentWorkspaceDirty();
        }
    }
    
    /// <summary>
    /// 编辑区域
    /// </summary>
    private void DrawEditSection() {
        EditorGUILayout.LabelField("编辑控制", EditorStyles.boldLabel);
        
        GUI.enabled = (_session.CurrentWorkspace != null || (_config != null && _visualConfig != null));
        
        if (_isEditMode) {
            EditorGUILayout.HelpBox("编辑模式已激活\n在 Scene 视图中可以看到网格", MessageType.Info);
            
            if (GUILayout.Button("保存并退出编辑模式", GUILayout.Height(40))) {
                ExitEditMode();
            }
            
            if (GUILayout.Button("重新生成网格（丢弃未保存的更改）")) {
                if (EditorUtility.DisplayDialog("确认", "这将重新加载配置，丢弃未保存的更改。确定吗？", "确定", "取消")) {
                    GenerateEditGrid();
                }
            }
        } else {
            if (GUILayout.Button("进入编辑模式", GUILayout.Height(40))) {
                EnterEditMode();
            }
        }
        
        GUI.enabled = true;
    }
    
    /// <summary>
    /// 笔刷区域
    /// </summary>
    private void DrawBrushSection() {
        EditorGUILayout.LabelField("笔刷工具", EditorStyles.boldLabel);
        
        GUI.enabled = _isEditMode;
        
        _brushEnabled = EditorGUILayout.Toggle("启用笔刷", _brushEnabled);
        
        if (_brushEnabled) {
            _workspaceSceneTool = (WorkspaceSceneTool)GUILayout.Toolbar(
                (int)_workspaceSceneTool,
                new[] { "Terrain", "Spawn", "Goal", "Portal In", "Portal Out", "Path" }
            );

            DrawWorkspaceToolHelp();
            EditorGUILayout.HelpBox("笔刷模式：在 Scene 视图中按住鼠标左键涂抹\n快捷键：1-Ground 2-HighGround 3-Forbidden 4-Hole", MessageType.Info);
            
            _brushType = (TileType)EditorGUILayout.EnumPopup("笔刷类型", _brushType);
            _brushHeight = EditorGUILayout.IntSlider("笔刷高度", _brushHeight, 0, 3);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ground (1)")) _brushType = TileType.Ground;
            if (GUILayout.Button("HighGround (2)")) _brushType = TileType.HighGround;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Forbidden (3)")) _brushType = TileType.Forbidden;
            if (GUILayout.Button("Hole (4)")) _brushType = TileType.Hole;
            EditorGUILayout.EndHorizontal();
        }
        
        GUI.enabled = true;
    }
    
    /// <summary>
    /// 信息区域
    /// </summary>
    private void DrawWorkspaceToolHelp() {
        if (_session.CurrentWorkspace == null) {
            return;
        }

        switch (_workspaceSceneTool) {
            case WorkspaceSceneTool.Spawn:
                EditorGUILayout.HelpBox("Click a tile in SceneView to place a spawn marker. Hotkey: 5", MessageType.Info);
                EditorGUILayout.LabelField($"Spawn Markers: {_session.CurrentWorkspace.SpawnMarkers.Count}");
                break;

            case WorkspaceSceneTool.Goal:
                EditorGUILayout.HelpBox("Click a tile in SceneView to place a goal marker. Hotkey: 6", MessageType.Info);
                EditorGUILayout.LabelField($"Goal Markers: {_session.CurrentWorkspace.GoalMarkers.Count}");
                break;

            case WorkspaceSceneTool.PortalEntrance:
                EditorGUILayout.HelpBox("Click a tile in SceneView to capture the next portal entrance. Hotkey: 7", MessageType.Info);
                EditorGUILayout.LabelField($"Pending Portal Entrance: {FormatPendingPortalEntrance()}");
                break;

            case WorkspaceSceneTool.PortalExit:
                EditorGUILayout.HelpBox("Click a tile in SceneView to finish the pending portal pair. Hotkey: 8", MessageType.Info);
                EditorGUILayout.LabelField($"Pending Portal Entrance: {FormatPendingPortalEntrance()}");
                EditorGUILayout.LabelField($"Portal Pairs: {_session.CurrentWorkspace.PortalPairs.Count}");
                break;

            case WorkspaceSceneTool.PathEdit:
                EditorGUILayout.HelpBox("Scene path editing is reserved for the selected wave. Hotkey: 9", MessageType.Info);
                EditorGUILayout.LabelField($"Selected Wave: {GetSelectedWaveLabel()}");
                break;

            default:
                if (_session.CurrentWorkspace != null) {
                    EditorGUILayout.LabelField($"Spawn Markers: {_session.CurrentWorkspace.SpawnMarkers.Count}");
                    EditorGUILayout.LabelField($"Goal Markers: {_session.CurrentWorkspace.GoalMarkers.Count}");
                    EditorGUILayout.LabelField($"Portal Pairs: {_session.CurrentWorkspace.PortalPairs.Count}");
                }
                break;
        }
    }

    private void DrawInfoSection() {
        EditorGUILayout.LabelField("统计信息", EditorStyles.boldLabel);
        
        if (_session.CurrentWorkspace != null) {
            EditorGUILayout.LabelField($"当前工作区: {_session.CurrentWorkspace.LevelName}");
            EditorGUILayout.LabelField($"工作区尺寸: {_session.CurrentWorkspace.MapWidth}x{_session.CurrentWorkspace.MapDepth}");
            EditorGUILayout.LabelField($"当前模块: {_session.Mode}");
        } else if (_config != null) {
            EditorGUILayout.LabelField($"地图尺寸: {_config.mapWidth}x{_config.mapDepth}");
            EditorGUILayout.LabelField($"特殊格子数量: {_config.specialTiles.Count}");
            EditorGUILayout.LabelField($"起点数量: {_config.spawnPoints.Count}");
            EditorGUILayout.LabelField($"当前模块: {_session.Mode}");
        }
        
        if (_isEditMode && _gridParent != null) {
            int tileCount = _gridParent.transform.childCount;
            EditorGUILayout.LabelField($"编辑网格: {tileCount} 个格子");
        }

        if (_whiteboxRoot != null) {
            EditorGUILayout.LabelField($"白模格子: {_whiteboxRoot.transform.childCount} 个");
        }
    }
    
    
    // ==================== 编辑模式管理 ====================
    
    /// <summary>
    /// 进入编辑模式
    /// </summary>
    private void EnterEditMode() {
        if (_session.CurrentWorkspace != null) {
            GenerateWhiteboxFromWorkspace();
            return;
        }

        if (_config == null || _visualConfig == null) {
            EditorUtility.DisplayDialog("错误", "请先配置 LevelConfig 和 GridVisualConfig", "确定");
            return;
        }
        
        if (!_config.Validate()) {
            EditorUtility.DisplayDialog("错误", "LevelConfig 配置无效，请检查 Console", "确定");
            return;
        }
        
        GenerateEditGrid();
        _isEditMode = true;
        _session.StartEditing(_config);
        
        Debug.Log("[LevelEditor] 进入编辑模式");
    }
    
    /// <summary>
    /// 退出编辑模式
    /// </summary>
    private void ExitEditMode() {
        // 保存资源
        AssetDatabase.SaveAssets();
        
        // 清理网格
        ClearEditGrid();
        
        _isEditMode = false;
        _brushEnabled = false;
        _pendingPortalEntrancePosition = null;
        _session.StopEditing();
        
        Debug.Log("[LevelEditor] 退出编辑模式，配置已保存");
        
        EditorUtility.DisplayDialog("完成", "编辑完成，配置已保存", "确定");
    }
    
    
    // ==================== 网格生成与清理 ====================
    
    /// <summary>
    /// 生成编辑网格
    /// </summary>
    private void GenerateEditGrid() {
        // 清理旧网格
        ClearEditGrid();
        
        // 创建父节点（DontSave 避免保存到场景）
        _gridParent = new GameObject("LevelEditGrid");
        _gridParent.hideFlags = HideFlags.DontSave;
        
        // 生成所有格子
        for (int x = 0; x < _config.mapWidth; x++) {
            for (int z = 0; z < _config.mapDepth; z++) {
                CreateTileAuthoring(x, z);
            }
        }
        
        // 选中父节点
        Selection.activeGameObject = _gridParent;
        
        Debug.Log($"[LevelEditor] 生成了 {_config.mapWidth}x{_config.mapDepth} 的编辑网格");
    }
    
    /// <summary>
    /// 创建单个格子
    /// </summary>
    private void CreateTileAuthoring(int x, int z) {
        // 创建 Cube
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(_gridParent.transform);
        cube.transform.localScale = new Vector3(_config.cellSize * 0.95f, 0.2f, _config.cellSize * 0.95f);
        cube.name = $"Tile_{x}_{z}";
        
        // 添加 TileAuthoring 组件
        var authoring = cube.AddComponent<TileAuthoring>();
        authoring.Initialize(x, z, _config, _visualConfig);
        
        // 设置标签（用于笔刷射线检测）
        cube.tag = "EditorOnly";
    }
    
    /// <summary>
    /// 清理编辑网格
    /// </summary>
    private void ClearEditGrid() {
        if (_gridParent != null) {
            DestroyImmediate(_gridParent);
            _gridParent = null;
            Debug.Log("[LevelEditor] 清理了编辑网格");
        }
    }
    
    
    // ==================== Scene 视图交互 ====================
    
    /// <summary>
    /// Scene 视图 GUI 回调
    /// </summary>
    private void OnSceneGUI(SceneView sceneView) {
        if (!_isEditMode || !_brushEnabled) return;
        
        Event e = Event.current;
        
        // 处理快捷键
        HandleHotkeys(e);
        
        // 处理笔刷涂抹
        HandleBrushPaint(e);
        
        // 显示笔刷信息
        DrawBrushInfo();
    }
    
    /// <summary>
    /// 处理快捷键
    /// </summary>
    private void HandleHotkeys(Event e) {
        if (e.type == EventType.KeyDown) {
            switch (e.keyCode) {
                case KeyCode.Alpha1:
                case KeyCode.Keypad1:
                    _workspaceSceneTool = WorkspaceSceneTool.Terrain;
                    _brushType = TileType.Ground;
                    Repaint();
                    e.Use();
                    break;
                    
                case KeyCode.Alpha2:
                case KeyCode.Keypad2:
                    _workspaceSceneTool = WorkspaceSceneTool.Terrain;
                    _brushType = TileType.HighGround;
                    Repaint();
                    e.Use();
                    break;
                    
                case KeyCode.Alpha3:
                case KeyCode.Keypad3:
                    _workspaceSceneTool = WorkspaceSceneTool.Terrain;
                    _brushType = TileType.Forbidden;
                    Repaint();
                    e.Use();
                    break;
                    
                case KeyCode.Alpha4:
                case KeyCode.Keypad4:
                    _workspaceSceneTool = WorkspaceSceneTool.Terrain;
                    _brushType = TileType.Hole;
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.Alpha5:
                case KeyCode.Keypad5:
                    _workspaceSceneTool = WorkspaceSceneTool.Spawn;
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.Alpha6:
                case KeyCode.Keypad6:
                    _workspaceSceneTool = WorkspaceSceneTool.Goal;
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.Alpha7:
                case KeyCode.Keypad7:
                    _workspaceSceneTool = WorkspaceSceneTool.PortalEntrance;
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.Alpha8:
                case KeyCode.Keypad8:
                    _workspaceSceneTool = WorkspaceSceneTool.PortalExit;
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.Alpha9:
                case KeyCode.Keypad9:
                    _workspaceSceneTool = WorkspaceSceneTool.PathEdit;
                    Repaint();
                    e.Use();
                    break;
            }
        }
    }
    
    /// <summary>
    /// 处理笔刷涂抹
    /// </summary>
    private void HandleBrushPaint(Event e) {
        if (_workspaceSceneTool != WorkspaceSceneTool.Terrain) {
            if (e.type == EventType.MouseDown && e.button == 0) {
                PaintTile(e);
            }

            if (e.type == EventType.MouseUp && e.button == 0) {
                _isPainting = false;
            }

            return;
        }
        // 鼠标按下或拖动时涂抹
        if (e.type == EventType.MouseDown && e.button == 0) {
            _isPainting = true;
        }
        
        if (e.type == EventType.MouseUp && e.button == 0) {
            _isPainting = false;
        }
        
        if (_isPainting && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown)) {
            PaintTile(e);
        }
    }
    
    /// <summary>
    /// 涂抹格子
    /// </summary>
    private void PaintTile(Event e) {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            var authoring = hit.collider.GetComponent<TileAuthoring>();
            
            if (authoring != null) {
                if (_session.CurrentWorkspace != null) {
                    _workspaceMapController = _workspaceMapController ?? new WorkspaceMapController(_session.CurrentWorkspace);
                    switch (_workspaceSceneTool) {
                        case WorkspaceSceneTool.Spawn:
                            Undo.RecordObject(authoring, "Place Workspace Spawn Marker");
                            _workspaceMapController.PlaceSpawnMarker(authoring.X, authoring.Z);
                            MarkCurrentWorkspaceDirty();
                            break;

                        case WorkspaceSceneTool.Goal:
                            Undo.RecordObject(authoring, "Place Workspace Goal Marker");
                            _workspaceMapController.PlaceGoalMarker(authoring.X, authoring.Z);
                            MarkCurrentWorkspaceDirty();
                            break;

                        case WorkspaceSceneTool.PortalEntrance:
                            _pendingPortalEntrancePosition = new Vector2Int(authoring.X, authoring.Z);
                            Repaint();
                            break;

                        case WorkspaceSceneTool.PortalExit:
                            if (!_pendingPortalEntrancePosition.HasValue) {
                                Debug.LogWarning("[LevelEditor] Select a portal entrance before placing a portal exit.");
                                break;
                            }

                            if (_pendingPortalEntrancePosition.Value.x == authoring.X
                                && _pendingPortalEntrancePosition.Value.y == authoring.Z) {
                                Debug.LogWarning("[LevelEditor] Portal entrance and exit must be placed on different tiles.");
                                break;
                            }

                            Undo.RecordObject(authoring, "Place Workspace Portal Pair");
                            _workspaceMapController.PlacePortalPair(
                                _pendingPortalEntrancePosition.Value,
                                new Vector2Int(authoring.X, authoring.Z));
                            _pendingPortalEntrancePosition = null;
                            MarkCurrentWorkspaceDirty();
                            Repaint();
                            break;

                        case WorkspaceSceneTool.PathEdit:
                            Repaint();
                            break;

                        default:
                            Undo.RecordObject(authoring, "Paint Workspace Tile");
                            _workspaceMapController.PaintTile(authoring.X, authoring.Z, _brushType, _brushHeight);
                            MarkCurrentWorkspaceDirty();
                            break;
                    }
                    e.Use();
                    return;
                }

                // 记录 Undo
                Undo.RecordObject(authoring, "Paint Tile");
                Undo.RecordObject(_config, "Paint Tile");
                
                // 设置类型和高度
                authoring.SetTileType(_brushType);
                authoring.SetHeightLevel(_brushHeight);
                
                // 消耗事件，防止选中物体
                e.Use();
            }
        }
    }
    
    /// <summary>
    /// 显示笔刷信息
    /// </summary>
    private void DrawBrushInfo() {
        Handles.BeginGUI();
        
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.7f));
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        
        string info = GetSceneToolOverlayText();
        GUILayout.BeginArea(new Rect(10, 10, 320, 88));
        GUILayout.Box(info, style);
        GUILayout.EndArea();
        
        Handles.EndGUI();
    }
    
    /// <summary>
    /// 创建纯色贴图（用于 GUI 背景）
    /// </summary>
    private string GetSceneToolOverlayText() {
        if (_session.CurrentWorkspace == null) {
            return $"Tool: {_brushType}\nHotkeys: 1-4 terrain";
        }

        switch (_workspaceSceneTool) {
            case WorkspaceSceneTool.Spawn:
                return "Tool: Spawn\nClick to place a spawn marker\nHotkey: 5";

            case WorkspaceSceneTool.Goal:
                return "Tool: Goal\nClick to place a goal marker\nHotkey: 6";

            case WorkspaceSceneTool.PortalEntrance:
                return $"Tool: Portal In\nPending: {FormatPendingPortalEntrance()}\nHotkey: 7";

            case WorkspaceSceneTool.PortalExit:
                return $"Tool: Portal Out\nPending: {FormatPendingPortalEntrance()}\nHotkey: 8";

            case WorkspaceSceneTool.PathEdit:
                return $"Tool: Path\nSelected Wave: {GetSelectedWaveLabel()}\nHotkey: 9";

            default:
                return $"Tool: Terrain / {_brushType}\nHeight: {_brushHeight}\nHotkeys: 1-4 terrain, 5 spawn, 6 goal, 7-9 semantic";
        }
    }

    private Texture2D MakeTex(int width, int height, Color col) {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) {
            pix[i] = col;
        }
        
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
    
    
    // ==================== 关卡管理功能 ====================
    
    /// <summary>
    /// 创建新的工作区
    /// </summary>
    private void CreateWorkspace() {
        if (string.IsNullOrWhiteSpace(_newLevelName)) {
            EditorUtility.DisplayDialog("错误", "关卡名称不能为空", "确定");
            return;
        }

        var workspace = LevelEditorWorkspace.CreateNew(_newLevelName);
        string assetPath = LevelEditorWorkspaceRepository.SaveAsNewAsset(workspace);
        LoadWorkspaceAsset(assetPath);

        Debug.Log($"[LevelEditor] 创建了新的工作区: {workspace.LevelName}");
    }

    /// <summary>
    /// 从当前工作区生成或刷新白模
    /// </summary>
    private void OpenWorkspace() {
        string initialDirectory = GetDefaultWorkspaceFolderPath();
        string selectedPath = EditorUtility.OpenFilePanel("Open Workspace", initialDirectory, "asset");
        if (string.IsNullOrWhiteSpace(selectedPath)) {
            return;
        }

        if (!TryConvertAbsolutePathToAssetPath(selectedPath, out string assetPath)) {
            EditorUtility.DisplayDialog("閿欒", "鍙兘鎵撳紑褰撳墠椤圭洰鍐呯殑 Workspace 璧勬簮", "纭畾");
            return;
        }

        LoadWorkspaceAsset(assetPath);
    }

    private void LoadWorkspaceAsset(string assetPath) {
        var asset = LevelEditorWorkspaceRepository.LoadAsset(assetPath);
        if (asset == null) {
            EditorUtility.DisplayDialog("閿欒", "鏃犳硶鍔犺浇鎸囧畾鐨?Workspace 璧勬簮", "纭畾");
            return;
        }

        _session.SetWorkspaceAsset(asset);
        _workspaceMapController = new WorkspaceMapController(_session.CurrentWorkspace);
        _pendingPortalEntrancePosition = null;
        _newLevelName = _session.CurrentWorkspace?.LevelName ?? _newLevelName;

        if (_isEditMode || _whiteboxRoot != null) {
            GenerateWhiteboxFromWorkspace();
        }
    }

    private void SaveWorkspace() {
        if (_session.CurrentWorkspaceAsset == null) {
            EditorUtility.DisplayDialog("閿欒", "褰撳墠娌℃湁鍙繚瀛樼殑 Workspace 璧勬簮", "纭畾");
            return;
        }

        LevelEditorWorkspaceRepository.Save(_session.CurrentWorkspaceAsset);
    }

    private void SyncWorkspaceNaming() {
        if (_session.CurrentWorkspace == null) {
            return;
        }

        _session.CurrentWorkspace.ExportName = _session.CurrentWorkspace.LevelName;
        MarkCurrentWorkspaceDirty();
    }

    private void GenerateWhiteboxFromWorkspace() {
        var workspace = _session.CurrentWorkspace;
        if (workspace == null) {
            EditorUtility.DisplayDialog("错误", "请先创建工作区", "确定");
            return;
        }

        _workspaceMapController = _workspaceMapController ?? new WorkspaceMapController(workspace);
        _whiteboxRoot = WhiteboxGenerationService.GenerateIntoOpenScene(workspace, _visualConfig);
        _isEditMode = true;
        _pendingPortalEntrancePosition = null;
        _session.StartEditing();
        Debug.Log($"[LevelEditor] 已根据工作区生成白模: {workspace.LevelName}");
    }

    /// <summary>
    /// 导出当前工作区为 LevelConfig
    /// </summary>
    private void ExportCurrentWorkspace() {
        var workspace = _session.CurrentWorkspace;
        if (workspace == null) {
            EditorUtility.DisplayDialog("错误", "请先创建工作区", "确定");
            return;
        }

        _config = LevelConfigExportService.Export(workspace);
        _session.SetCurrentLevel(_config);

        Debug.Log($"[LevelEditor] 已导出配置: {_config.name}");
        EditorUtility.DisplayDialog("成功", $"已导出 {_config.name}", "确定");
        Selection.activeObject = _config;
        EditorGUIUtility.PingObject(_config);
    }

    /// <summary>
    /// 创建新的关卡配置
    /// </summary>
    private void MarkCurrentWorkspaceDirty() {
        if (_session.CurrentWorkspaceAsset != null) {
            EditorUtility.SetDirty(_session.CurrentWorkspaceAsset);
        }
    }

    private static bool TryConvertAbsolutePathToAssetPath(string absolutePath, out string assetPath) {
        assetPath = null;
        if (string.IsNullOrWhiteSpace(absolutePath)) {
            return false;
        }

        string projectAssetsPath = Path.GetFullPath(Application.dataPath).Replace("\\", "/");
        string normalizedAbsolutePath = Path.GetFullPath(absolutePath).Replace("\\", "/");
        if (!normalizedAbsolutePath.StartsWith(projectAssetsPath, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }

        assetPath = "Assets" + normalizedAbsolutePath.Substring(projectAssetsPath.Length);
        return true;
    }

    private static string GetDefaultWorkspaceFolderPath() {
        string relativeFolder = LevelEditorWorkspaceRepository.DefaultRootFolder.Replace('/', Path.DirectorySeparatorChar);
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
        string fullPath = Path.Combine(projectRoot, relativeFolder);
        return Directory.Exists(fullPath) ? fullPath : Application.dataPath;
    }

    private string FormatPendingPortalEntrance() {
        return _pendingPortalEntrancePosition.HasValue
            ? _pendingPortalEntrancePosition.Value.ToString()
            : "<none>";
    }

    private string GetSelectedWaveLabel() {
        if (_session.CurrentWorkspace == null
            || _selectedWaveIndex < 0
            || _selectedWaveIndex >= _session.CurrentWorkspace.Waves.Count
            || _session.CurrentWorkspace.Waves[_selectedWaveIndex] == null) {
            return "<none>";
        }

        return _session.CurrentWorkspace.Waves[_selectedWaveIndex].waveId;
    }

    private void CreateLevelConfig() {
        // 验证名称
        if (string.IsNullOrWhiteSpace(_newLevelName)) {
            EditorUtility.DisplayDialog("错误", "关卡名称不能为空", "确定");
            return;
        }
        
        // 构建路径
        string configPath = $"Assets/Resources/Levels/Configs/{_newLevelName}.asset";
        
        // 检查文件是否已存在
        if (File.Exists(configPath)) {
            if (!EditorUtility.DisplayDialog("确认", $"配置文件 {_newLevelName} 已存在，是否覆盖？", "覆盖", "取消")) {
                return;
            }
        }
        
        // 创建新的 LevelConfig 实例
        LevelConfig newConfig = ScriptableObject.CreateInstance<LevelConfig>();
        
        // 设置默认值
        newConfig.mapWidth = 10;
        newConfig.mapDepth = 10;
        newConfig.cellSize = 1.0f;
        newConfig.defaultTileType = TileType.Ground;
        newConfig.goalPoint = new Vector2Int(9, 9);
        newConfig.spawnPoints = new System.Collections.Generic.List<Vector2Int> { 
            new Vector2Int(0, 0) 
        };
        
        // 保存资源
        AssetDatabase.CreateAsset(newConfig, configPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 自动分配到编辑器
        _config = newConfig;
        _session.SetCurrentLevel(_config);
        
        Debug.Log($"[LevelEditor] 创建了新的关卡配置: {configPath}");
        EditorUtility.DisplayDialog("成功", $"关卡配置 {_newLevelName} 已创建", "确定");
        
        // 选中新创建的资源
        Selection.activeObject = newConfig;
        EditorGUIUtility.PingObject(newConfig);
    }
    
    /// <summary>
    /// 构建场景
    /// </summary>
    private void BuildScene() {
        // 保存当前场景
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }

        if (LevelSceneBuilder.TryBuild(_config, _visualConfig, out string scenePath, out string errorMessage)) {
            Debug.Log($"[LevelEditor] 场景已保存到: {scenePath}");
            EditorUtility.DisplayDialog("成功", $"场景已创建并保存到 {scenePath}", "确定");
        } else {
            EditorUtility.DisplayDialog("错误", errorMessage, "确定");
        }
    }
    
    /// <summary>
    /// 从当前场景加载配置
    /// </summary>
    private void LoadFromScene() {
        if (LevelSceneLoader.TryLoadFromOpenScene(out LevelConfig config, out GridVisualConfig visualConfig, out string errorMessage)) {
            _config = config;
            _visualConfig = visualConfig;
            _session.SetCurrentLevel(_config);

            Debug.Log($"[LevelEditor] 已从场景加载配置: {config.name}");
            EditorUtility.DisplayDialog("成功", $"已加载配置: {config.name}", "确定");
        } else {
            EditorUtility.DisplayDialog("错误", errorMessage, "确定");
        }
    }
}
