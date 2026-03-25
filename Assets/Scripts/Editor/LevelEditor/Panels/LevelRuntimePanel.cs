namespace ArknightsLite.Editor.LevelEditor.Panels {
    using ArknightsLite.Editor.LevelEditor.Core;
    using UnityEditor;
    using UnityEngine;

    public static class LevelRuntimePanel {
        public static void Draw(LevelEditorWorkspace workspace) {
            EditorGUILayout.LabelField(LevelEditorText.RuntimePanel.SectionTitle, EditorStyles.boldLabel);

            if (workspace == null) {
                EditorGUILayout.HelpBox(LevelEditorText.RuntimePanel.EmptyHelp, MessageType.Info);
                return;
            }

            int initialDp = EditorGUILayout.IntField(LevelEditorText.RuntimePanel.InitialDpLabel, workspace.Runtime.InitialDp);
            int baseHealth = EditorGUILayout.IntField(LevelEditorText.RuntimePanel.BaseHealthLabel, workspace.Runtime.BaseHealth);
            float dpRecoveryInterval = EditorGUILayout.FloatField(LevelEditorText.RuntimePanel.DpRecoveryIntervalLabel, workspace.Runtime.DpRecoveryInterval);
            int dpRecoveryAmount = EditorGUILayout.IntField(LevelEditorText.RuntimePanel.DpRecoveryAmountLabel, workspace.Runtime.DpRecoveryAmount);

            ApplyEdits(workspace, initialDp, baseHealth, dpRecoveryInterval, dpRecoveryAmount);
        }

        public static void ApplyEdits(LevelEditorWorkspace workspace, int initialDp, int baseHealth, float dpRecoveryInterval, int dpRecoveryAmount) {
            if (workspace == null || workspace.Runtime == null) {
                return;
            }

            workspace.Runtime.InitialDp = Mathf.Max(0, initialDp);
            workspace.Runtime.BaseHealth = Mathf.Max(1, baseHealth);
            workspace.Runtime.DpRecoveryInterval = Mathf.Max(0.1f, dpRecoveryInterval);
            workspace.Runtime.DpRecoveryAmount = Mathf.Max(1, dpRecoveryAmount);
        }
    }
}
