using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using ArknightsLite.Config;
using ArknightsLite.Model;
using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Map;
using ArknightsLite.Editor.LevelEditor.Panels;
using ArknightsLite.Editor.LevelEditor.Services;
using WindowText = ArknightsLite.Editor.LevelEditor.Core.LevelEditorText.Window;

/// <summary>
/// 閸忓啿宕辩紓鏍帆閸ｃ劎鐛ラ崣?
/// 閼卞矁鐭楅敍姘辨晸閹存劕褰茬紓鏍帆閻ㄥ嫮缍夐弽绗衡偓浣瑰絹娓氭稓鐟崚宄颁紣閸忔灚鈧胶顓搁悶鍡欑椽鏉堟垳绱扮拠?
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
    
    // ==================== 闁板秶鐤嗗鏇犳暏 ====================
    
    private LevelConfig _config;
    private GridVisualConfig _visualConfig;
    
    
    // ==================== 缂傛牞绶悩鑸碘偓?====================
    
    private WhiteboxRoot _whiteboxRoot;
    private WorkspaceMapController _workspaceMapController;
    private bool _isEditMode = false;
    private int _workspaceToolTab = 0;
    private int _selectedWaveIndex = -1;
    
    
    // ==================== 缁楁柨鍩涚拋鍓х枂 ====================
    
    private bool _brushEnabled = false;
    private TileType _brushType = TileType.Ground;
    private int _brushHeight = 0;
    private bool _isPainting = false;
    private WorkspaceSceneTool _workspaceSceneTool = WorkspaceSceneTool.Terrain;
    private WorkspaceSceneTool _lastTileSceneTool = WorkspaceSceneTool.Terrain;
    private Vector2Int? _pendingPortalEntrancePosition;
    
    
    // ==================== 閸忓啿宕辩粻锛勬倞 ====================
    
    private string _newLevelName = LevelEditorWorkspace.DefaultLevelName;
    private int _newMapWidth = 10;
    private int _newMapDepth = 10;
    private float _newCellSize = 1f;
    private readonly LevelEditorSession _session = new LevelEditorSession();
    
    
    // ==================== 缁愭褰涚粻锛勬倞 ====================
    
    [MenuItem(WindowText.MenuPath)]
    public static void ShowWindow() {
        var window = GetWindow<LevelEditorWindow>(WindowText.Title);
        window.minSize = new Vector2(300, 400);
    }
    
    private void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    private void OnDisable() {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    
    // ==================== UI 缂佹ê鍩?====================
    
    private void OnGUI() {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(WindowText.Title, EditorStyles.boldLabel);
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
    /// Workspace management
    /// </summary>
    private void DrawLevelManagementSection() {
        EditorGUILayout.LabelField(WindowText.WorkspaceSectionTitle, EditorStyles.boldLabel);

        var currentWorkspace = _session.CurrentWorkspace;
        bool hasCurrentWorkspace = currentWorkspace != null;

        if (hasCurrentWorkspace) {
            EditorGUI.BeginChangeCheck();
            string nextLevelName = EditorGUILayout.TextField(WindowText.WorkspaceNameLabel, currentWorkspace.LevelName);
            string nextExportName = EditorGUILayout.TextField(WindowText.ExportNameLabel, currentWorkspace.ExportName);
            int nextMapWidth = EditorGUILayout.IntField(WindowText.MapWidthLabel, currentWorkspace.MapWidth);
            int nextMapDepth = EditorGUILayout.IntField(WindowText.MapDepthLabel, currentWorkspace.MapDepth);
            float nextCellSize = EditorGUILayout.FloatField(WindowText.CellSizeLabel, currentWorkspace.CellSize);
            if (EditorGUI.EndChangeCheck()) {
                currentWorkspace.LevelName = nextLevelName;
                currentWorkspace.ExportName = nextExportName;
                currentWorkspace.MapWidth = Mathf.Max(1, nextMapWidth);
                currentWorkspace.MapDepth = Mathf.Max(1, nextMapDepth);
                currentWorkspace.CellSize = Mathf.Max(0.1f, nextCellSize);
                SyncDraftFieldsFromWorkspace(currentWorkspace);
                MarkCurrentWorkspaceDirty();
            }

            if (GUILayout.Button(WindowText.SyncNamingButton, GUILayout.Width(120))) {
                SyncWorkspaceNaming();
            }
        } else {
            _newLevelName = EditorGUILayout.TextField(WindowText.WorkspaceNameLabel, _newLevelName);
            _newMapWidth = Mathf.Max(1, EditorGUILayout.IntField(WindowText.MapWidthLabel, _newMapWidth));
            _newMapDepth = Mathf.Max(1, EditorGUILayout.IntField(WindowText.MapDepthLabel, _newMapDepth));
            _newCellSize = Mathf.Max(0.1f, EditorGUILayout.FloatField(WindowText.CellSizeLabel, _newCellSize));
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(WindowText.NewWorkspaceButton, GUILayout.Width(120))) {
            EditorApplication.delayCall += CreateWorkspace;
        }

        if (GUILayout.Button(WindowText.OpenWorkspaceButton, GUILayout.Width(120))) {
            EditorApplication.delayCall += OpenWorkspace;
        }

        GUI.enabled = hasCurrentWorkspace;
        if (GUILayout.Button(WindowText.SaveWorkspaceButton, GUILayout.Width(120))) {
            EditorApplication.delayCall += SaveWorkspace;
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        if (hasCurrentWorkspace) {
            EditorGUILayout.HelpBox(WindowText.CurrentWorkspaceSummary(currentWorkspace), MessageType.Info);
        } else {
            EditorGUILayout.HelpBox(WindowText.EmptyWorkspaceHelp, MessageType.Info);
        }

        GUI.enabled = hasCurrentWorkspace;
        if (GUILayout.Button(WindowText.GenerateWhiteboxButton, GUILayout.Height(30))) {
            EditorApplication.delayCall += GenerateWhiteboxFromWorkspace;
        }
        if (GUILayout.Button(WindowText.ExportLevelConfigButton, GUILayout.Height(30))) {
            EditorApplication.delayCall += ExportCurrentWorkspace;
        }
        GUI.enabled = true;
    }
    
    /// <summary>
    /// Export information
    /// </summary>
    private void DrawConfigSection() {
        EditorGUILayout.LabelField(WindowText.ExportInfoSectionTitle, EditorStyles.boldLabel);

        if (_session.CurrentWorkspace == null) {
            EditorGUILayout.HelpBox(WindowText.ExportInfoEmptyHelp, MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField(WindowText.ExportAssetLabel(LevelConfigExportService.BuildAssetName(_session.CurrentWorkspace)));
        if (_config != null) {
            EditorGUILayout.LabelField(WindowText.LastExportLabel(_config.name));
        }

        EditorGUILayout.HelpBox(WindowText.ExportInfoNotice, MessageType.Info);
    }

    /// <summary>
    /// Runtime parameters
    /// </summary>
    private void DrawRuntimeSection() {
        EditorGUI.BeginChangeCheck();
        LevelRuntimePanel.Draw(_session.CurrentWorkspace);
        if (EditorGUI.EndChangeCheck()) {
            MarkCurrentWorkspaceDirty();
        }
    }

    /// <summary>
    /// Workspace 濡€虫健閸栧搫鐓?
    /// </summary>
    private void DrawWorkspaceModulesSection() {
        if (_session.CurrentWorkspace == null) {
            return;
        }

        EditorGUILayout.LabelField(WindowText.WorkspaceModulesTitle, EditorStyles.boldLabel);
        _workspaceToolTab = GUILayout.Toolbar(_workspaceToolTab, WindowText.WorkspaceModuleTabs);

        EditorGUI.BeginChangeCheck();
        switch (_workspaceToolTab) {
            case 0:
                EditorGUILayout.HelpBox(WindowText.MapEditingHelp, MessageType.Info);
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
    /// Editing controls
    /// </summary>
    private void DrawEditSection() {
        EditorGUILayout.LabelField(WindowText.EditingSectionTitle, EditorStyles.boldLabel);

        GUI.enabled = _session.CurrentWorkspace != null;

        if (_isEditMode) {
            EditorGUILayout.HelpBox(WindowText.EditModeHelp, MessageType.Info);

            if (GUILayout.Button(WindowText.SaveAndLeaveEditModeButton, GUILayout.Height(40))) {
                ExitEditMode();
            }

            if (GUILayout.Button(WindowText.RegenerateWhiteboxButton)) {
                if (EditorUtility.DisplayDialog(
                    WindowText.ConfirmDialogTitle,
                    WindowText.RegenerateWhiteboxConfirmMessage,
                    WindowText.RegenerateButton,
                    WindowText.CancelButton)) {
                    GenerateWhiteboxFromWorkspace();
                }
            }
        } else {
            if (GUILayout.Button(WindowText.EnterWhiteboxEditModeButton, GUILayout.Height(40))) {
                EnterEditMode();
            }
        }

        GUI.enabled = true;
    }
    
    /// <summary>
    /// 缁楁柨鍩涢崠鍝勭厵
    /// </summary>
    private void DrawBrushSection() {
        EditorGUILayout.LabelField(WindowText.BrushSectionTitle, EditorStyles.boldLabel);
        
        GUI.enabled = _isEditMode;
        
        _brushEnabled = EditorGUILayout.Toggle(WindowText.EnableBrushLabel, _brushEnabled);
        
        if (_brushEnabled) {
            int selectedBrushTab = _workspaceSceneTool == WorkspaceSceneTool.PathEdit ? 1 : 0;
            int nextBrushTab = GUILayout.Toolbar(selectedBrushTab, WindowText.BrushToolTabs);
            if (nextBrushTab != selectedBrushTab) {
                if (nextBrushTab == 1) {
                    SelectPathEditTool();
                } else {
                    RestoreTileBrushTool();
                }
            }

            DrawWorkspaceToolHelp();

            if (_workspaceSceneTool != WorkspaceSceneTool.PathEdit) {
                EditorGUILayout.HelpBox(WindowText.BrushHelp, MessageType.Info);
                EditorGUILayout.LabelField(WindowText.BrushTypeLabel, EditorStyles.miniBoldLabel);

                EditorGUILayout.BeginHorizontal();
                DrawBrushSelectionButton(WindowText.GroundButton, IsTerrainBrushSelected(TileType.Ground), () => SelectTerrainBrush(TileType.Ground));
                DrawBrushSelectionButton(WindowText.HighGroundButton, IsTerrainBrushSelected(TileType.HighGround), () => SelectTerrainBrush(TileType.HighGround));
                DrawBrushSelectionButton(WindowText.ForbiddenButton, IsTerrainBrushSelected(TileType.Forbidden), () => SelectTerrainBrush(TileType.Forbidden));
                DrawBrushSelectionButton(WindowText.HoleButton, IsTerrainBrushSelected(TileType.Hole), () => SelectTerrainBrush(TileType.Hole));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DrawBrushSelectionButton(WindowText.SpawnButton, _workspaceSceneTool == WorkspaceSceneTool.Spawn, () => SelectTileBrushTool(WorkspaceSceneTool.Spawn));
                DrawBrushSelectionButton(WindowText.GoalButton, _workspaceSceneTool == WorkspaceSceneTool.Goal, () => SelectTileBrushTool(WorkspaceSceneTool.Goal));
                DrawBrushSelectionButton(WindowText.PortalEntranceButton, _workspaceSceneTool == WorkspaceSceneTool.PortalEntrance, () => SelectTileBrushTool(WorkspaceSceneTool.PortalEntrance));
                DrawBrushSelectionButton(WindowText.PortalExitButton, _workspaceSceneTool == WorkspaceSceneTool.PortalExit, () => SelectTileBrushTool(WorkspaceSceneTool.PortalExit));
                EditorGUILayout.EndHorizontal();

                if (_workspaceSceneTool == WorkspaceSceneTool.Terrain) {
                    _brushHeight = EditorGUILayout.IntSlider(WindowText.BrushHeightLabel, _brushHeight, 0, 3);
                }
            }
        }
        
        GUI.enabled = true;
    }
    
    /// <summary>
    /// 娣団剝浼呴崠鍝勭厵
    /// </summary>
    private void DrawWorkspaceToolHelp() {
        if (_session.CurrentWorkspace == null) {
            return;
        }

        switch (_workspaceSceneTool) {
            case WorkspaceSceneTool.Spawn:
                EditorGUILayout.HelpBox(WindowText.SpawnHelp, MessageType.Info);
                EditorGUILayout.LabelField(WindowText.SpawnMarkersLabel(_session.CurrentWorkspace.SpawnMarkers.Count));
                break;

            case WorkspaceSceneTool.Goal:
                EditorGUILayout.HelpBox(WindowText.GoalHelp, MessageType.Info);
                EditorGUILayout.LabelField(WindowText.GoalMarkersLabel(_session.CurrentWorkspace.GoalMarkers.Count));
                break;

            case WorkspaceSceneTool.PortalEntrance:
                EditorGUILayout.HelpBox(WindowText.PortalEntranceHelp, MessageType.Info);
                EditorGUILayout.LabelField(WindowText.PendingPortalEntranceLabel(FormatPendingPortalEntrance()));
                break;

            case WorkspaceSceneTool.PortalExit:
                EditorGUILayout.HelpBox(WindowText.PortalExitHelp, MessageType.Info);
                EditorGUILayout.LabelField(WindowText.PendingPortalEntranceLabel(FormatPendingPortalEntrance()));
                EditorGUILayout.LabelField(WindowText.PortalPairsLabel(_session.CurrentWorkspace.PortalPairs.Count));
                break;

            case WorkspaceSceneTool.PathEdit:
                EditorGUILayout.HelpBox(WindowText.PathEditHelp, MessageType.Info);
                EditorGUILayout.LabelField(WindowText.SelectedWaveLabel(GetSelectedWaveLabel()));
                break;

            default:
                if (_session.CurrentWorkspace != null) {
                    EditorGUILayout.LabelField(WindowText.SpawnMarkersLabel(_session.CurrentWorkspace.SpawnMarkers.Count));
                    EditorGUILayout.LabelField(WindowText.GoalMarkersLabel(_session.CurrentWorkspace.GoalMarkers.Count));
                    EditorGUILayout.LabelField(WindowText.PortalPairsLabel(_session.CurrentWorkspace.PortalPairs.Count));
                }
                break;
        }
    }

    private void DrawBrushSelectionButton(string label, bool isSelected, Action onSelect) {
        Color previousColor = GUI.backgroundColor;
        if (isSelected) {
            GUI.backgroundColor = new Color(0.34f, 0.62f, 0.95f, 1f);
        }

        if (GUILayout.Button(label, GUILayout.Height(28))) {
            onSelect?.Invoke();
        }

        GUI.backgroundColor = previousColor;
    }

    private bool IsTerrainBrushSelected(TileType tileType) {
        return _workspaceSceneTool == WorkspaceSceneTool.Terrain && _brushType == tileType;
    }

    private void SelectTerrainBrush(TileType tileType) {
        _brushType = tileType;
        SelectTileBrushTool(WorkspaceSceneTool.Terrain);
    }

    private void SelectTileBrushTool(WorkspaceSceneTool sceneTool) {
        _workspaceSceneTool = sceneTool;
        _lastTileSceneTool = sceneTool;
    }

    private void SelectPathEditTool() {
        _workspaceSceneTool = WorkspaceSceneTool.PathEdit;
    }

    private void RestoreTileBrushTool() {
        _workspaceSceneTool = _lastTileSceneTool == WorkspaceSceneTool.PathEdit
            ? WorkspaceSceneTool.Terrain
            : _lastTileSceneTool;
    }

    private void DrawInfoSection() {
        EditorGUILayout.LabelField(WindowText.StatusSectionTitle, EditorStyles.boldLabel);

        if (_session.CurrentWorkspace != null) {
            EditorGUILayout.LabelField(WindowText.CurrentWorkspaceLabel(_session.CurrentWorkspace.LevelName));
            EditorGUILayout.LabelField(WindowText.MapSizeLabel(_session.CurrentWorkspace.MapWidth, _session.CurrentWorkspace.MapDepth));
            EditorGUILayout.LabelField(WindowText.CellSizeValueLabel(_session.CurrentWorkspace.CellSize));
            EditorGUILayout.LabelField(WindowText.SpawnMarkersLabel(_session.CurrentWorkspace.SpawnMarkers.Count));
            EditorGUILayout.LabelField(WindowText.GoalMarkersLabel(_session.CurrentWorkspace.GoalMarkers.Count));
            EditorGUILayout.LabelField(WindowText.PortalPairsLabel(_session.CurrentWorkspace.PortalPairs.Count));
            EditorGUILayout.LabelField(WindowText.CurrentModeLabel(_session.Mode));
        } else {
            EditorGUILayout.HelpBox(WindowText.NoWorkspaceLoadedHelp, MessageType.Info);
        }

        if (_whiteboxRoot != null) {
            EditorGUILayout.LabelField(WindowText.WhiteboxTilesLabel(_whiteboxRoot.transform.childCount));
        }

        if (_config != null) {
            EditorGUILayout.LabelField(WindowText.LastExportLabel(_config.name));
        }
    }
    
    
    // ==================== 缂傛牞绶Ο鈥崇础缁狅紕鎮?====================
    
    /// <summary>
    /// Enter edit mode
    /// </summary>
    private void EnterEditMode() {
        if (_session.CurrentWorkspace == null) {
            EditorUtility.DisplayDialog(WindowText.MissingWorkspaceTitle, WindowText.MissingWorkspaceBeforeEditMessage, WindowText.OkButton);
            return;
        }

        GenerateWhiteboxFromWorkspace();
    }

    /// <summary>
    /// Exit edit mode
    /// </summary>
    private void ExitEditMode() {
        AssetDatabase.SaveAssets();

        _isEditMode = false;
        _brushEnabled = false;
        _pendingPortalEntrancePosition = null;
        _workspaceSceneTool = WorkspaceSceneTool.Terrain;
        _lastTileSceneTool = WorkspaceSceneTool.Terrain;
        _session.StopEditing();

        Debug.Log("[LevelEditor] 已退出编辑模式，并保留场景中的 Workspace 白模。");
        EditorUtility.DisplayDialog(WindowText.SuccessDialogTitle, WindowText.ExitEditModeSavedMessage, WindowText.OkButton);
    }
    
    
    // ==================== Scene 鐟欏棗娴樻禍銈勭鞍 ====================
    
    /// <summary>
    /// Scene 鐟欏棗娴?GUI 閸ョ偠鐨?
    /// </summary>
    private void OnSceneGUI(SceneView sceneView) {
        if (!_isEditMode || !_brushEnabled) return;
        
        Event e = Event.current;
        
        // 婢跺嫮鎮婅箛顐ｅ祹闁?
        HandleHotkeys(e);
        
        // 婢跺嫮鎮婄粭鏂垮煕濞戝倹濮?
        HandleBrushPaint(e);
        
        // 閺勫墽銇氱粭鏂垮煕娣団剝浼?
        DrawBrushInfo();
        DrawSelectedWavePathOverlay();
    }
    
    /// <summary>
    /// 婢跺嫮鎮婅箛顐ｅ祹闁?
    /// </summary>
    private void HandleHotkeys(Event e) {
        if (e.type == EventType.KeyDown) {
            switch (e.keyCode) {
                case KeyCode.Alpha1:
                case KeyCode.Keypad1:
                    SelectTerrainBrush(TileType.Ground);
                    Repaint();
                    e.Use();
                    break;
                    
                case KeyCode.Alpha2:
                case KeyCode.Keypad2:
                    SelectTerrainBrush(TileType.HighGround);
                    Repaint();
                    e.Use();
                    break;
                    
                case KeyCode.Alpha3:
                case KeyCode.Keypad3:
                    SelectTerrainBrush(TileType.Forbidden);
                    Repaint();
                    e.Use();
                    break;
                    
                case KeyCode.Alpha4:
                case KeyCode.Keypad4:
                    SelectTerrainBrush(TileType.Hole);
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.Alpha5:
                case KeyCode.Keypad5:
                    SelectTileBrushTool(WorkspaceSceneTool.Spawn);
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.Alpha6:
                case KeyCode.Keypad6:
                    SelectTileBrushTool(WorkspaceSceneTool.Goal);
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.Alpha7:
                case KeyCode.Keypad7:
                    SelectTileBrushTool(WorkspaceSceneTool.PortalEntrance);
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.Alpha8:
                case KeyCode.Keypad8:
                    SelectTileBrushTool(WorkspaceSceneTool.PortalExit);
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.Alpha9:
                case KeyCode.Keypad9:
                    SelectPathEditTool();
                    Repaint();
                    e.Use();
                    break;
            }
        }
    }
    
    /// <summary>
    /// 婢跺嫮鎮婄粭鏂垮煕濞戝倹濮?
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
        // 姒х姵鐖ｉ幐澶夌瑓閹存牗瀚嬮崝銊︽濞戝倹濮?
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
    /// Paint or place data on the clicked tile
    /// </summary>
    private void PaintTile(Event e) {
        if (_session.CurrentWorkspace == null) {
            return;
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) {
            return;
        }

        var authoring = hit.collider.GetComponent<TileAuthoring>();
        if (authoring == null) {
            return;
        }

        _workspaceMapController = _workspaceMapController ?? new WorkspaceMapController(_session.CurrentWorkspace);
        switch (_workspaceSceneTool) {
            case WorkspaceSceneTool.Spawn:
                Undo.RecordObject(authoring, WindowText.UndoPlaceSpawnMarker);
                _workspaceMapController.PlaceSpawnMarker(authoring.X, authoring.Z);
                MarkCurrentWorkspaceDirty();
                break;

            case WorkspaceSceneTool.Goal:
                Undo.RecordObject(authoring, WindowText.UndoPlaceGoalMarker);
                _workspaceMapController.PlaceGoalMarker(authoring.X, authoring.Z);
                MarkCurrentWorkspaceDirty();
                break;

            case WorkspaceSceneTool.PortalEntrance:
                _pendingPortalEntrancePosition = new Vector2Int(authoring.X, authoring.Z);
                Repaint();
                break;

            case WorkspaceSceneTool.PortalExit:
                if (!_pendingPortalEntrancePosition.HasValue) {
                    Debug.LogWarning(WindowText.PortalExitRequiresEntranceWarning);
                    break;
                }

                if (_pendingPortalEntrancePosition.Value.x == authoring.X
                    && _pendingPortalEntrancePosition.Value.y == authoring.Z) {
                    Debug.LogWarning(WindowText.PortalExitMustUseDifferentTileWarning);
                    break;
                }

                Undo.RecordObject(authoring, WindowText.UndoPlacePortalPair);
                _workspaceMapController.PlacePortalPair(
                    _pendingPortalEntrancePosition.Value,
                    new Vector2Int(authoring.X, authoring.Z));
                _pendingPortalEntrancePosition = null;
                MarkCurrentWorkspaceDirty();
                Repaint();
                break;

            case WorkspaceSceneTool.PathEdit:
                if (PathEditorPanel.TogglePathNodeForSelectedWave(
                    _session.CurrentWorkspace,
                    _selectedWaveIndex,
                    new Vector2Int(authoring.X, authoring.Z))) {
                    MarkCurrentWorkspaceDirty();
                }

                Repaint();
                break;

            default:
                Undo.RecordObject(authoring, WindowText.UndoPaintTile);
                _workspaceMapController.PaintTile(authoring.X, authoring.Z, _brushType, _brushHeight);
                MarkCurrentWorkspaceDirty();
                break;
        }

        e.Use();
    }
    
    /// <summary>
    /// 閺勫墽銇氱粭鏂垮煕娣団剝浼?
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
    /// 閸掓稑缂撶痪顖濆鐠愭潙娴橀敍鍫㈡暏娴?GUI 閼冲本娅欓敍?
    /// </summary>
    private string GetSceneToolOverlayText() {
        if (_session.CurrentWorkspace == null) {
            return WindowText.NoWorkspaceOverlay(_brushType);
        }

        switch (_workspaceSceneTool) {
            case WorkspaceSceneTool.Spawn:
                return WindowText.SpawnOverlay;

            case WorkspaceSceneTool.Goal:
                return WindowText.GoalOverlay;

            case WorkspaceSceneTool.PortalEntrance:
                return WindowText.PortalEntranceOverlay(FormatPendingPortalEntrance());

            case WorkspaceSceneTool.PortalExit:
                return WindowText.PortalExitOverlay(FormatPendingPortalEntrance());

            case WorkspaceSceneTool.PathEdit:
                return WindowText.PathOverlay(GetSelectedWaveLabel());

            default:
                return WindowText.TerrainOverlay(_brushType, _brushHeight);
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
    
    
    // ==================== 閸忓啿宕辩粻锛勬倞閸旂喕鍏?====================
    
    /// <summary>
    /// 閸掓稑缂撻弬鎵畱瀹搞儰缍旈崠?
    /// </summary>
    private void CreateWorkspace() {
        if (string.IsNullOrWhiteSpace(_newLevelName)) {
            EditorUtility.DisplayDialog(WindowText.ErrorDialogTitle, WindowText.EmptyWorkspaceNameMessage, WindowText.OkButton);
            return;
        }

        var workspace = LevelEditorWorkspace.CreateNew(_newLevelName);
        workspace.MapWidth = Mathf.Max(1, _newMapWidth);
        workspace.MapDepth = Mathf.Max(1, _newMapDepth);
        workspace.CellSize = Mathf.Max(0.1f, _newCellSize);
        string assetPath = LevelEditorWorkspaceRepository.SaveAsNewAsset(workspace);
        LoadWorkspaceAsset(assetPath);

        Debug.Log($"[LevelEditor] 已创建 Workspace: {workspace.LevelName}");
    }

    /// <summary>
    /// 娴犲骸缍嬮崜宥呬紣娴ｆ粌灏悽鐔稿灇閹存牕鍩涢弬鎵濡?
    /// </summary>
    private void OpenWorkspace() {
        string initialDirectory = GetDefaultWorkspaceFolderPath();
        string selectedPath = EditorUtility.OpenFilePanel(WindowText.OpenWorkspaceButton, initialDirectory, "asset");
        if (string.IsNullOrWhiteSpace(selectedPath)) {
            return;
        }

        if (!TryConvertAbsolutePathToAssetPath(selectedPath, out string assetPath)) {
            EditorUtility.DisplayDialog(WindowText.ErrorDialogTitle, WindowText.OpenWorkspacePathErrorMessage, WindowText.OkButton);
            return;
        }

        LoadWorkspaceAsset(assetPath);
    }

    private void LoadWorkspaceAsset(string assetPath) {
        var asset = LevelEditorWorkspaceRepository.LoadAsset(assetPath);
        if (asset == null) {
            EditorUtility.DisplayDialog(WindowText.ErrorDialogTitle, WindowText.LoadWorkspaceFailedMessage, WindowText.OkButton);
            return;
        }

        _session.SetWorkspaceAsset(asset);
        _workspaceMapController = new WorkspaceMapController(_session.CurrentWorkspace);
        _pendingPortalEntrancePosition = null;
        SyncDraftFieldsFromWorkspace(_session.CurrentWorkspace);

        if (_isEditMode || _whiteboxRoot != null) {
            GenerateWhiteboxFromWorkspace();
        }
    }

    private void SaveWorkspace() {
        if (_session.CurrentWorkspaceAsset == null) {
            EditorUtility.DisplayDialog(WindowText.ErrorDialogTitle, WindowText.SaveWorkspaceMissingMessage, WindowText.OkButton);
            return;
        }

        LevelEditorWorkspaceRepository.Save(_session.CurrentWorkspaceAsset);
    }

    private void SyncWorkspaceNaming() {
        if (_session.CurrentWorkspace == null) {
            return;
        }

        _session.CurrentWorkspace.ExportName = _session.CurrentWorkspace.LevelName;
        SyncDraftFieldsFromWorkspace(_session.CurrentWorkspace);
        MarkCurrentWorkspaceDirty();
    }

    private void SyncDraftFieldsFromWorkspace(LevelEditorWorkspace workspace) {
        if (workspace == null) {
            return;
        }

        _newLevelName = workspace.LevelName;
        _newMapWidth = Mathf.Max(1, workspace.MapWidth);
        _newMapDepth = Mathf.Max(1, workspace.MapDepth);
        _newCellSize = Mathf.Max(0.1f, workspace.CellSize);
    }

    private void GenerateWhiteboxFromWorkspace() {
        var workspace = _session.CurrentWorkspace;
        if (workspace == null) {
            EditorUtility.DisplayDialog(WindowText.ErrorDialogTitle, WindowText.MissingWorkspaceMessage, WindowText.OkButton);
            return;
        }

        _workspaceMapController = _workspaceMapController ?? new WorkspaceMapController(workspace);
        _whiteboxRoot = WhiteboxGenerationService.GenerateIntoOpenScene(workspace, _visualConfig);
        _isEditMode = true;
        _pendingPortalEntrancePosition = null;
        _session.StartEditing();
        Debug.Log($"[LevelEditor] 已根据 Workspace 生成白模: {workspace.LevelName}");
    }

    /// <summary>
    /// 鐎电厧鍤ぐ鎾冲瀹搞儰缍旈崠杞拌礋 LevelConfig
    /// </summary>
    private void ExportCurrentWorkspace() {
        var workspace = _session.CurrentWorkspace;
        if (workspace == null) {
            EditorUtility.DisplayDialog(WindowText.ErrorDialogTitle, WindowText.MissingWorkspaceMessage, WindowText.OkButton);
            return;
        }

        LevelValidationResult workspaceValidation = LevelValidationService.ValidateWorkspace(workspace);
        if (!workspaceValidation.IsValid) {
            ShowValidationDialog(WindowText.WorkspaceValidationFailedTitle, workspaceValidation);
            return;
        }

        LevelConfig transientConfig = LevelConfigExportService.BuildTransientConfig(workspace);
        try {
            LevelValidationResult configValidation = LevelValidationService.Validate(transientConfig);
            if (!configValidation.IsValid) {
                ShowValidationDialog(WindowText.ExportValidationFailedTitle, configValidation);
                return;
            }
        } finally {
            DestroyImmediate(transientConfig);
        }

        if (LevelConfigExportService.AssetExists(workspace)
            && !EditorUtility.DisplayDialog(
                WindowText.OverwriteExportTitle,
                WindowText.OverwriteExportMessage(LevelConfigExportService.BuildAssetName(workspace)),
                WindowText.OverwriteButton,
                WindowText.CancelButton)) {
            return;
        }

        _config = LevelConfigExportService.Export(workspace);
        _session.SetCurrentLevel(_config);

        Debug.Log($"[LevelEditor] 已导出 LevelConfig: {_config.name}");
        EditorUtility.DisplayDialog(WindowText.SuccessDialogTitle, WindowText.ExportSuccessMessage(_config.name), WindowText.OkButton);
        Selection.activeObject = _config;
        EditorGUIUtility.PingObject(_config);
    }

    /// <summary>
    /// 閸掓稑缂撻弬鎵畱閸忓啿宕遍柊宥囩枂
    /// </summary>
    private void MarkCurrentWorkspaceDirty() {
        if (_session.CurrentWorkspaceAsset != null) {
            EditorUtility.SetDirty(_session.CurrentWorkspaceAsset);
        }
    }

    private static void ShowValidationDialog(string title, LevelValidationResult result) {
        if (result == null || result.IsValid) {
            return;
        }

        EditorUtility.DisplayDialog(title, string.Join("\n", result.Errors), WindowText.OkButton);
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
            : WindowText.NoneValue;
    }

    private string GetSelectedWaveLabel() {
        if (_session.CurrentWorkspace == null
            || _selectedWaveIndex < 0
            || _selectedWaveIndex >= _session.CurrentWorkspace.Waves.Count
            || _session.CurrentWorkspace.Waves[_selectedWaveIndex] == null) {
            return WindowText.NoneValue;
        }

        return _session.CurrentWorkspace.Waves[_selectedWaveIndex].waveId;
    }

    private void DrawSelectedWavePathOverlay() {
        if (!TryGetSelectedWave(out WaveDefinition wave) || wave.path == null || wave.path.Count == 0) {
            return;
        }

        Color previousColor = Handles.color;
        Handles.color = new Color(1f, 0.85f, 0.2f, 0.95f);

        Vector3? previousPoint = null;
        for (int i = 0; i < wave.path.Count; i++) {
            PathNodeDefinition node = wave.path[i];
            if (node == null) {
                continue;
            }

            Vector3 point = GetPathNodeWorldPosition(node);
            Handles.DrawSolidDisc(point, Vector3.up, Mathf.Max(0.08f, _session.CurrentWorkspace.CellSize * 0.12f));
            Handles.Label(point + Vector3.up * 0.12f, $"P{i + 1}");

            if (previousPoint.HasValue) {
                Handles.DrawAAPolyLine(4f, previousPoint.Value, point);
            }

            previousPoint = point;
        }

        Handles.color = previousColor;
    }

    private bool TryGetSelectedWave(out WaveDefinition wave) {
        wave = null;
        if (_session.CurrentWorkspace == null
            || _selectedWaveIndex < 0
            || _selectedWaveIndex >= _session.CurrentWorkspace.Waves.Count) {
            return false;
        }

        wave = _session.CurrentWorkspace.Waves[_selectedWaveIndex];
        return wave != null;
    }

    private Vector3 GetPathNodeWorldPosition(PathNodeDefinition node) {
        float cellSize = _session.CurrentWorkspace != null ? _session.CurrentWorkspace.CellSize : 1f;
        float height = 0.25f;

        if (_session.CurrentWorkspace != null && node != null) {
            height += _session.CurrentWorkspace.GetTileOverride(node.x, node.y).heightLevel * cellSize;
        }

        return new Vector3(node.x * cellSize, height, node.y * cellSize);
    }

}
