namespace ArknightsLite.Editor {
    using UnityEngine;
    using UnityEditor;
    using ArknightsLite.Config;
    using ArknightsLite.Model;

    /// <summary>
    /// LevelConfig 自定义编辑器
    /// 提供便捷的可视化编辑功能
    /// </summary>
    [CustomEditor(typeof(LevelConfig))]
    public class LevelConfigEditor : UnityEditor.Editor {
        
        private LevelConfig _config;
        private Vector2 _scrollPos;
        
        private void OnEnable() {
            _config = (LevelConfig)target;
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("关卡配置编辑器", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // 基础配置
            DrawBasicSettings();
            
            EditorGUILayout.Space(10);
            
            // 特殊格子配置
            DrawSpecialTiles();
            
            EditorGUILayout.Space(10);
            
            // 寻路配置
            DrawPathfindingSettings();
            
            EditorGUILayout.Space(10);
            
            // 工具按钮
            DrawUtilityButtons();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawBasicSettings() {
            EditorGUILayout.LabelField("地图尺寸（俯视图）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("宽度=X轴方向，长度=Z轴方向（纵深）", MessageType.Info);
            _config.mapWidth = EditorGUILayout.IntSlider("宽度 (X轴)", _config.mapWidth, 5, 30);
            _config.mapDepth = EditorGUILayout.IntSlider("长度 (Z轴)", _config.mapDepth, 5, 30);
            _config.cellSize = EditorGUILayout.Slider("单元格尺寸", _config.cellSize, 0.5f, 2f);
        }
        
        private void DrawSpecialTiles() {
            EditorGUILayout.LabelField("特殊格子配置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("在这里配置非默认的格子类型", MessageType.Info);
            
            // 显示特殊格子数量
            EditorGUILayout.LabelField($"特殊格子数量: {_config.specialTiles.Count}");
            
            // 滚动视图
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(200));
            
            for (int i = 0; i < _config.specialTiles.Count; i++) {
                EditorGUILayout.BeginVertical("box");
                
                var tile = _config.specialTiles[i];
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"格子 #{i}", EditorStyles.boldLabel);
                
                // 删除按钮
                if (GUILayout.Button("删除", GUILayout.Width(60))) {
                    _config.specialTiles.RemoveAt(i);
                    EditorUtility.SetDirty(_config);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                
                tile.x = EditorGUILayout.IntField("X 坐标", tile.x);
                tile.z = EditorGUILayout.IntField("Z 坐标", tile.z);
                tile.tileType = (TileType)EditorGUILayout.EnumPopup("格子类型", tile.tileType);
                tile.heightLevel = EditorGUILayout.IntSlider("高度等级", tile.heightLevel, 0, 3);
                tile.walkable = EditorGUILayout.Toggle("可通行", tile.walkable);
                tile.deployTag = EditorGUILayout.TextField("部署标签", tile.deployTag);
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            
            EditorGUILayout.EndScrollView();
            
            // 添加新格子按钮
            if (GUILayout.Button("添加特殊格子")) {
                _config.specialTiles.Add(new TileData {
                    x = 0,
                    z = 0,
                    tileType = TileType.Forbidden
                });
                EditorUtility.SetDirty(_config);
            }
        }
        
        private void DrawPathfindingSettings() {
            EditorGUILayout.LabelField("关卡必需配置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("终点唯一，起点可多个", MessageType.Info);
            
            // 终点
            _config.goalPoint = EditorGUILayout.Vector2IntField("终点坐标（唯一）", _config.goalPoint);
            
            EditorGUILayout.Space(5);
            
            // 起点列表
            EditorGUILayout.LabelField($"起点列表 (共{_config.spawnPoints.Count}个)", EditorStyles.boldLabel);
            
            for (int i = 0; i < _config.spawnPoints.Count; i++) {
                EditorGUILayout.BeginHorizontal();
                _config.spawnPoints[i] = EditorGUILayout.Vector2IntField($"起点 #{i}", _config.spawnPoints[i]);
                
                if (GUILayout.Button("删除", GUILayout.Width(60))) {
                    _config.spawnPoints.RemoveAt(i);
                    EditorUtility.SetDirty(_config);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("添加起点")) {
                _config.spawnPoints.Add(new Vector2Int(0, 0));
                EditorUtility.SetDirty(_config);
            }
        }
        
        private void DrawUtilityButtons() {
            EditorGUILayout.LabelField("工具", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            // 验证配置
            if (GUILayout.Button("验证配置")) {
                if (_config.Validate()) {
                    EditorUtility.DisplayDialog("验证成功", "配置有效！", "确定");
                } else {
                    EditorUtility.DisplayDialog("验证失败", "配置无效，请检查 Console", "确定");
                }
            }
            
            // 清空特殊格子
            if (GUILayout.Button("清空特殊格子")) {
                if (EditorUtility.DisplayDialog("确认", "确定要清空所有特殊格子吗？", "确定", "取消")) {
                    _config.specialTiles.Clear();
                    EditorUtility.SetDirty(_config);
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
