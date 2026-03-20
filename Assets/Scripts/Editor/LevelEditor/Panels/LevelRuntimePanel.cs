namespace ArknightsLite.Editor.LevelEditor.Panels {
    using ArknightsLite.Editor.LevelEditor.Core;
    using UnityEditor;
    using UnityEngine;

    public static class LevelRuntimePanel {
        public static void Draw(LevelEditorWorkspace workspace) {
            EditorGUILayout.LabelField("关卡运行时参数", EditorStyles.boldLabel);

            if (workspace == null) {
                EditorGUILayout.HelpBox("创建工作区后可直接编辑初始 DP、基地生命和回费参数。", MessageType.Info);
                return;
            }

            int initialDp = EditorGUILayout.IntField("初始 DP", workspace.Runtime.InitialDp);
            int baseHealth = EditorGUILayout.IntField("基地生命", workspace.Runtime.BaseHealth);
            float dpRecoveryInterval = EditorGUILayout.FloatField("回费间隔", workspace.Runtime.DpRecoveryInterval);
            int dpRecoveryAmount = EditorGUILayout.IntField("单次回费", workspace.Runtime.DpRecoveryAmount);

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
