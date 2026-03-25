using ArknightsLite.Model;
using ArknightsLite.Editor.LevelEditor.Core;
using UnityEditor;
using UnityEngine;

namespace ArknightsLite.Editor {
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

            var script = (TileAuthoring)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(LevelEditorText.TileInspector.SectionTitle, EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.LabelField(LevelEditorText.TileInspector.GridLabel(script.X, script.Z));
                EditorGUILayout.HelpBox(LevelEditorText.TileInspector.WhiteboxDrivenHelp, MessageType.Info);

                if (!string.IsNullOrWhiteSpace(script.SemanticLabel)) {
                    EditorGUILayout.LabelField(LevelEditorText.TileInspector.SemanticLabelsLabel(script.SemanticLabel), EditorStyles.boldLabel);
                }

                EditorGUILayout.LabelField("最终主类型", script.VisualTypeName, EditorStyles.boldLabel);

                if (script.HasSpawnMarker) {
                    EditorGUILayout.LabelField(LevelEditorText.TileInspector.SpawnMarkerLabel(script.SpawnMarkerId), EditorStyles.boldLabel);
                }

                if (script.HasGoalMarker) {
                    EditorGUILayout.LabelField(LevelEditorText.TileInspector.GoalMarkerLabel(script.GoalMarkerId), EditorStyles.boldLabel);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(LevelEditorText.TileInspector.DataSectionTitle, EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_tileTypeProp, new GUIContent(LevelEditorText.TileInspector.TileTypeLabel));
            if (EditorGUI.EndChangeCheck()) {
                TileType newType = (TileType)_tileTypeProp.enumValueIndex;
                _walkableProp.boolValue = newType != TileType.Forbidden && newType != TileType.Hole;
            }

            EditorGUILayout.PropertyField(_heightLevelProp, new GUIContent(LevelEditorText.TileInspector.HeightLevelLabel));
            EditorGUILayout.PropertyField(_walkableProp, new GUIContent(LevelEditorText.TileInspector.WalkableLabel));
            EditorGUILayout.PropertyField(_deployTagProp, new GUIContent(LevelEditorText.TileInspector.DeployTagLabel));

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed) {
                script.ForceSync();
            }
        }
    }
}
