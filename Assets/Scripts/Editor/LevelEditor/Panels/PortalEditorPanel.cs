namespace ArknightsLite.Editor.LevelEditor.Panels {
    using ArknightsLite.Editor.LevelEditor.Core;
    using UnityEditor;
    using UnityEngine;

    public static class PortalEditorPanel {
        public static void Draw(LevelEditorWorkspace workspace) {
            EditorGUILayout.LabelField("传送门", EditorStyles.boldLabel);

            if (workspace == null) {
                EditorGUILayout.HelpBox("创建工作区后可编辑传送门。", MessageType.Info);
                return;
            }

            if (GUILayout.Button("添加传送门")) {
                workspace.Portals.Add(LevelEditorWorkspace.CreateDefaultPortal($"portal_{workspace.Portals.Count + 1:00}"));
            }

            for (int i = 0; i < workspace.Portals.Count; i++) {
                var portal = workspace.Portals[i];
                if (portal == null) {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    portal.id = EditorGUILayout.TextField("ID", portal.id);
                    portal.inPos = EditorGUILayout.Vector2IntField("入口", portal.inPos);
                    portal.outPos = EditorGUILayout.Vector2IntField("出口", portal.outPos);
                    portal.delay = EditorGUILayout.FloatField("延迟", portal.delay);
                    portal.color = EditorGUILayout.TextField("颜色", portal.color);

                    if (GUILayout.Button("删除传送门")) {
                        workspace.Portals.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}
