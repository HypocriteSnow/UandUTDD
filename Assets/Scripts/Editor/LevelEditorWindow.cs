using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using ArknightsLite.Config;
using ArknightsLite.Model;

/// <summary>
/// 关卡编辑器窗口
/// 职责：生成可编辑的网格、提供笔刷工具、管理编辑会话
/// </summary>
public class LevelEditorWindow : EditorWindow {
    
    // ==================== 配置引用 ====================
    
    private LevelConfig _config;
    private GridVisualConfig _visualConfig;
    
    
    // ==================== 编辑状态 ====================
    
    private GameObject _gridParent;
    private bool _isEditMode = false;
    
    
    // ==================== 笔刷设置 ====================
    
    private bool _brushEnabled = false;
    private TileType _brushType = TileType.Ground;
    private int _brushHeight = 0;
    private bool _isPainting = false;
    
    
    // ==================== 关卡管理 ====================
    
    private string _newLevelName = "NewLevel";
    
    
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
        
        DrawConfigSection();
        EditorGUILayout.Space(10);
        
        DrawEditSection();
        EditorGUILayout.Space(10);
        
        DrawBrushSection();
        EditorGUILayout.Space(10);
        
        DrawInfoSection();
    }
    
    /// <summary>
    /// 关卡管理区域
    /// </summary>
    private void DrawLevelManagementSection() {
        EditorGUILayout.LabelField("关卡管理", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        _newLevelName = EditorGUILayout.TextField("新关卡名称", _newLevelName);
        if (GUILayout.Button("创建配置", GUILayout.Width(80))) {
            EditorApplication.delayCall += CreateLevelConfig;
        }
        EditorGUILayout.EndHorizontal();
        
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
        EditorGUILayout.LabelField("配置文件", EditorStyles.boldLabel);
        
        _config = (LevelConfig)EditorGUILayout.ObjectField("Level Config", _config, typeof(LevelConfig), false);
        _visualConfig = (GridVisualConfig)EditorGUILayout.ObjectField("Visual Config", _visualConfig, typeof(GridVisualConfig), false);
        
        if (_config == null || _visualConfig == null) {
            EditorGUILayout.HelpBox("请先拖入 LevelConfig 和 GridVisualConfig", MessageType.Warning);
        }
    }
    
    /// <summary>
    /// 编辑区域
    /// </summary>
    private void DrawEditSection() {
        EditorGUILayout.LabelField("编辑控制", EditorStyles.boldLabel);
        
        GUI.enabled = (_config != null && _visualConfig != null);
        
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
    private void DrawInfoSection() {
        EditorGUILayout.LabelField("统计信息", EditorStyles.boldLabel);
        
        if (_config != null) {
            EditorGUILayout.LabelField($"地图尺寸: {_config.mapWidth}x{_config.mapDepth}");
            EditorGUILayout.LabelField($"特殊格子数量: {_config.specialTiles.Count}");
            EditorGUILayout.LabelField($"起点数量: {_config.spawnPoints.Count}");
        }
        
        if (_isEditMode && _gridParent != null) {
            int tileCount = _gridParent.transform.childCount;
            EditorGUILayout.LabelField($"编辑网格: {tileCount} 个格子");
        }
    }
    
    
    // ==================== 编辑模式管理 ====================
    
    /// <summary>
    /// 进入编辑模式
    /// </summary>
    private void EnterEditMode() {
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
                    _brushType = TileType.Ground;
                    Repaint();
                    e.Use();
                    break;
                    
                case KeyCode.Alpha2:
                case KeyCode.Keypad2:
                    _brushType = TileType.HighGround;
                    Repaint();
                    e.Use();
                    break;
                    
                case KeyCode.Alpha3:
                case KeyCode.Keypad3:
                    _brushType = TileType.Forbidden;
                    Repaint();
                    e.Use();
                    break;
                    
                case KeyCode.Alpha4:
                case KeyCode.Keypad4:
                    _brushType = TileType.Hole;
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
        
        string info = $"笔刷: {_brushType} (高度 {_brushHeight})\n按 1-4 切换类型";
        GUILayout.BeginArea(new Rect(10, 10, 250, 60));
        GUILayout.Box(info, style);
        GUILayout.EndArea();
        
        Handles.EndGUI();
    }
    
    /// <summary>
    /// 创建纯色贴图（用于 GUI 背景）
    /// </summary>
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
    /// 创建新的关卡配置
    /// </summary>
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
        if (_config == null || _visualConfig == null) {
            EditorUtility.DisplayDialog("错误", "请先配置 LevelConfig 和 GridVisualConfig", "确定");
            return;
        }
        
        // 检查配置有效性
        if (!_config.Validate()) {
            EditorUtility.DisplayDialog("错误", "LevelConfig 配置无效，请检查 Console", "确定");
            return;
        }
        
        // 保存当前场景
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }
        
        // 创建新场景
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // 创建 GameManager
        GameObject gameManagerObj = new GameObject("GameManager");
        ArknightsLite.GameMain gameMain = gameManagerObj.AddComponent<ArknightsLite.GameMain>();
        
        // 使用反射设置私有字段
        var gameMainType = typeof(ArknightsLite.GameMain);
        var levelConfigField = gameMainType.GetField("_levelConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (levelConfigField != null) {
            levelConfigField.SetValue(gameMain, _config);
        }
        
        // 创建 GridVisualizer
        GameObject gridVisualizerObj = new GameObject("GridVisualizer");
        ArknightsLite.View.GridRenderer gridRenderer = gridVisualizerObj.AddComponent<ArknightsLite.View.GridRenderer>();
        
        // 使用反射设置私有字段
        var gridRendererType = typeof(ArknightsLite.View.GridRenderer);
        var visualConfigField = gridRendererType.GetField("_visualConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var levelConfigField2 = gridRendererType.GetField("_levelConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (visualConfigField != null) {
            visualConfigField.SetValue(gridRenderer, _visualConfig);
        }
        if (levelConfigField2 != null) {
            levelConfigField2.SetValue(gridRenderer, _config);
        }
        
        // 创建主相机
        GameObject cameraObj = new GameObject("Main Camera");
        Camera camera = cameraObj.AddComponent<Camera>();
        cameraObj.tag = "MainCamera";
        cameraObj.AddComponent<AudioListener>();
        
        // 设置相机位置（俯视角度）
        float mapCenterX = _config.mapWidth * _config.cellSize * 0.5f;
        float mapCenterZ = _config.mapDepth * _config.cellSize * 0.5f;
        float mapMaxSize = Mathf.Max(_config.mapWidth, _config.mapDepth) * _config.cellSize;
        
        cameraObj.transform.position = new Vector3(mapCenterX, mapMaxSize * 0.8f, mapCenterZ - mapMaxSize * 0.5f);
        cameraObj.transform.rotation = Quaternion.Euler(45, 0, 0);
        
        // 创建方向光
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        
        // 保存场景
        string scenePath = $"Assets/Scenes/Levels/{_config.name}.unity";
        
        // 确保目录存在
        string sceneDir = Path.GetDirectoryName(scenePath);
        if (!Directory.Exists(sceneDir)) {
            Directory.CreateDirectory(sceneDir);
        }
        
        bool saved = EditorSceneManager.SaveScene(newScene, scenePath);
        
        if (saved) {
            Debug.Log($"[LevelEditor] 场景已保存到: {scenePath}");
            
            // 将场景添加到构建设置（可选）
            AddSceneToBuildSettings(scenePath);
            
            EditorUtility.DisplayDialog("成功", $"场景已创建并保存到 {scenePath}", "确定");
        } else {
            EditorUtility.DisplayDialog("错误", "场景保存失败", "确定");
        }
    }
    
    /// <summary>
    /// 从当前场景加载配置
    /// </summary>
    private void LoadFromScene() {
        // 查找 GameMain 组件
        ArknightsLite.GameMain gameMain = UnityEngine.Object.FindObjectOfType<ArknightsLite.GameMain>();
        
        if (gameMain == null) {
            EditorUtility.DisplayDialog("错误", "当前场景中未找到 GameMain 组件", "确定");
            return;
        }
        
        // 使用反射读取私有字段
        var gameMainType = typeof(ArknightsLite.GameMain);
        var levelConfigField = gameMainType.GetField("_levelConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (levelConfigField != null) {
            LevelConfig config = levelConfigField.GetValue(gameMain) as LevelConfig;
            
            if (config != null) {
                _config = config;
                Debug.Log($"[LevelEditor] 已从场景加载配置: {config.name}");
                EditorUtility.DisplayDialog("成功", $"已加载配置: {config.name}", "确定");
            } else {
                EditorUtility.DisplayDialog("错误", "GameMain 中的 LevelConfig 为空", "确定");
            }
        } else {
            EditorUtility.DisplayDialog("错误", "无法读取 GameMain 的 _levelConfig 字段", "确定");
        }
        
        // 同时尝试读取 GridRenderer 的配置
        ArknightsLite.View.GridRenderer gridRenderer = UnityEngine.Object.FindObjectOfType<ArknightsLite.View.GridRenderer>();
        if (gridRenderer != null) {
            var gridRendererType = typeof(ArknightsLite.View.GridRenderer);
            var visualConfigField = gridRendererType.GetField("_visualConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (visualConfigField != null) {
                GridVisualConfig visualConfig = visualConfigField.GetValue(gridRenderer) as GridVisualConfig;
                if (visualConfig != null) {
                    _visualConfig = visualConfig;
                    Debug.Log($"[LevelEditor] 已从场景加载视觉配置: {visualConfig.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 将场景添加到构建设置
    /// </summary>
    private void AddSceneToBuildSettings(string scenePath) {
        // 获取当前的场景列表
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        // 检查场景是否已存在
        bool exists = scenes.Exists(s => s.path == scenePath);
        
        if (!exists) {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[LevelEditor] 场景已添加到构建设置: {scenePath}");
        }
    }
}
