namespace ArknightsLite.Editor {
    using UnityEngine;
    using UnityEditor;
    using System.IO;

    /// <summary>
    /// 网格设置助手 - 自动创建材质和预制体
    /// </summary>
    public class GridSetupHelper : EditorWindow {
        
        [MenuItem("ArknightsLite/Setup Grid Materials")]
        public static void ShowWindow() {
            GetWindow<GridSetupHelper>("Grid Setup");
        }
        
        private void OnGUI() {
            GUILayout.Label("网格材质快速设置", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            EditorGUILayout.HelpBox("点击下方按钮自动创建所有必需的材质文件", MessageType.Info);
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("创建标准材质", GUILayout.Height(40))) {
                CreateStandardMaterials();
            }
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("创建格子预制体", GUILayout.Height(40))) {
                CreateTilePrefab();
            }
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("创建完整场景", GUILayout.Height(40))) {
                CreateCompleteScene();
            }
        }
        
        /// <summary>
        /// 创建标准材质
        /// </summary>
        private void CreateStandardMaterials() {
            string materialPath = "Assets/Resources/Materials";
            
            // 确保文件夹存在
            if (!Directory.Exists(materialPath)) {
                Directory.CreateDirectory(materialPath);
            }
            
            // 创建材质（方案A：匹配新的颜色描述）
            CreateMaterial(materialPath, "Mat_Ground", new Color(0.39f, 0.78f, 0.39f));        // 绿色
            CreateMaterial(materialPath, "Mat_HighGround", new Color(0.16f, 0.31f, 0.16f));    // 墨绿色
            CreateMaterial(materialPath, "Mat_Forbidden", new Color(1f, 1f, 1f));              // 白色
            CreateMaterial(materialPath, "Mat_Hole", new Color(0.2f, 0.2f, 0.2f));             // 黑色
            CreateMaterial(materialPath, "Mat_SpawnPoint", new Color(1f, 0.2f, 0.2f), true);   // 红色 + 发光
            CreateMaterial(materialPath, "Mat_GoalPoint", new Color(0.2f, 0.59f, 1f), true);   // 蓝色 + 发光
            CreateMaterial(materialPath, "Mat_Occupied", new Color(1f, 0.7f, 0.4f));           // 橙色
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完成", "材质创建成功！\n位置：Assets/Resources/Materials", "确定");
        }
        
        /// <summary>
        /// 创建单个材质
        /// </summary>
        private void CreateMaterial(string path, string name, Color color, bool emission = false) {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            
            if (emission) {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 0.5f);
            }
            
            AssetDatabase.CreateAsset(mat, $"{path}/{name}.mat");
        }
        
        /// <summary>
        /// 创建格子预制体
        /// </summary>
        private void CreateTilePrefab() {
            string prefabPath = "Assets/Resources/Prefabs";
            
            // 确保文件夹存在
            if (!Directory.Exists(prefabPath)) {
                Directory.CreateDirectory(prefabPath);
            }
            
            // 创建 Cube
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = "TilePrefab";
            tile.transform.localScale = new Vector3(1, 0.2f, 1);
            
            // 添加 TileRenderer 组件
            tile.AddComponent<ArknightsLite.View.TileRenderer>();
            
            // 保存为预制体
            string prefabFullPath = $"{prefabPath}/TilePrefab.prefab";
            PrefabUtility.SaveAsPrefabAsset(tile, prefabFullPath);
            
            // 删除场景中的临时对象
            DestroyImmediate(tile);
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完成", "预制体创建成功！\n位置：Assets/Resources/Prefabs/TilePrefab.prefab", "确定");
        }
        
        /// <summary>
        /// 创建完整场景
        /// </summary>
        private void CreateCompleteScene() {
            // 创建 GameManager
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<ArknightsLite.GameMain>();
            
            // 创建 GridVisualizer
            GameObject gridVisualizer = new GameObject("GridVisualizer");
            gridVisualizer.AddComponent<ArknightsLite.View.GridRenderer>();
            
            // 调整相机
            Camera mainCamera = Camera.main;
            if (mainCamera != null) {
                mainCamera.transform.position = new Vector3(5, 15, -5);
                mainCamera.transform.rotation = Quaternion.Euler(60, 0, 0);
            }
            
            EditorUtility.DisplayDialog("完成", "场景结构创建成功！\n\n下一步：\n1. 选中 GameManager，拖入 LevelConfig\n2. 选中 GridVisualizer，拖入 GridVisualConfig 和 LevelConfig\n3. 点击 Play", "确定");
        }
    }
}
