using UnityEngine;
using UnityEditor;
using ArknightsLite.Model;

namespace ArknightsLite.Editor {
    
    /// <summary>
    /// TileAuthoring 的自定义 Inspector
    /// 职责：提供更清晰的格子编辑体验，显示语义状态，并智能联动属性
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
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("基本信息", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.LabelField($"坐标: ({script.X}, {script.Z})");
                
                if (script.config != null) {
                    if (script.config.IsSpawnPoint(script.X, script.Z)) {
                        EditorGUILayout.LabelField("起点 (Spawn Point)", EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox("这是敌人的出生点，运行时会按可通行入口处理。", MessageType.Info);
                    }
                    else if (script.config.IsGoalPoint(script.X, script.Z)) {
                        EditorGUILayout.LabelField("终点 (Goal Point)", EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox("这是敌人的目标点，运行时会按可通行终点处理。", MessageType.Info);
                    }
                } else {
                    EditorGUILayout.HelpBox("当前格子由工作区白模驱动，不直接绑定 LevelConfig。", MessageType.Info);
                    if (script.HasSpawnMarker) {
                        EditorGUILayout.LabelField($"Spawn Marker: {script.SpawnMarkerId}", EditorStyles.boldLabel);
                    }

                    if (script.HasGoalMarker) {
                        EditorGUILayout.LabelField($"Goal Marker: {script.GoalMarkerId}", EditorStyles.boldLabel);
                    }
                }
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("格子属性", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_tileTypeProp);
            if (EditorGUI.EndChangeCheck()) {
                TileType newType = (TileType)_tileTypeProp.enumValueIndex;
                bool shouldBeWalkable = newType != TileType.Forbidden && newType != TileType.Hole;
                _walkableProp.boolValue = shouldBeWalkable;
            }
            
            EditorGUILayout.PropertyField(_heightLevelProp);
            EditorGUILayout.PropertyField(_walkableProp);
            EditorGUILayout.PropertyField(_deployTagProp);
            
            serializedObject.ApplyModifiedProperties();
            
            if (GUI.changed) {
                script.ForceSync();
            }
        }
    }
}
