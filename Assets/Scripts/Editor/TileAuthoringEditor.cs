using UnityEngine;
using UnityEditor;
using ArknightsLite.Model;

namespace ArknightsLite.Editor {
    
    /// <summary>
    /// TileAuthoring 的自定义 Inspector
    /// 职责：提供更智能的编辑体验，显示特殊状态，自动联动属性
    /// </summary>
    [CustomEditor(typeof(TileAuthoring))]
    [CanEditMultipleObjects]
    public class TileAuthoringEditor : UnityEditor.Editor {
        
        private SerializedProperty _tileTypeProp;
        private SerializedProperty _heightLevelProp;
        private SerializedProperty _walkableProp;
        private SerializedProperty _deployTagProp;
        
        private void OnEnable() {
            _tileTypeProp = serializedObject.FindProperty("tileType");
            _heightLevelProp = serializedObject.FindProperty("heightLevel");
            _walkableProp = serializedObject.FindProperty("walkable");
            _deployTagProp = serializedObject.FindProperty("deployTag");
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            
            TileAuthoring script = (TileAuthoring)target;
            
            // 1. 显示坐标和特殊状态
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("基本信息", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.LabelField($"坐标: ({script.X}, {script.Z})");
                
                // 检查是否为特殊点
                if (script.config != null) {
                    if (script.config.IsSpawnPoint(script.X, script.Z)) {
                        EditorGUILayout.LabelField("🚩 起点 (Spawn Point)", EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox("这是一个敌人出生点，运行时将强制为 Ground 类型且可通行。", MessageType.Info);
                    }
                    else if (script.config.IsGoalPoint(script.X, script.Z)) {
                        EditorGUILayout.LabelField("🏁 终点 (Goal Point)", EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox("这是一个敌人目标点，运行时将强制为 Ground 类型且可通行。", MessageType.Info);
                    }
                }
            }
            
            EditorGUILayout.Space();
            
            // 2. 绘制属性（带智能联动）
            EditorGUILayout.LabelField("格子属性", EditorStyles.boldLabel);
            
            // 绘制 TileType 并检测变化
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_tileTypeProp);
            if (EditorGUI.EndChangeCheck()) {
                // 智能联动：类型改变时自动设置通行性
                TileType newType = (TileType)_tileTypeProp.enumValueIndex;
                bool shouldBeWalkable = (newType != TileType.Forbidden && newType != TileType.Hole);
                _walkableProp.boolValue = shouldBeWalkable;
            }
            
            EditorGUILayout.PropertyField(_heightLevelProp);
            EditorGUILayout.PropertyField(_walkableProp);
            EditorGUILayout.PropertyField(_deployTagProp);
            
            serializedObject.ApplyModifiedProperties();
            
            // 确保视觉更新
            if (GUI.changed) {
                script.ForceSync();
            }
        }
    }
}
