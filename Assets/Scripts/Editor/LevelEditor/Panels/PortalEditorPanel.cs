namespace ArknightsLite.Editor.LevelEditor.Panels {
    using ArknightsLite.Editor.LevelEditor.Core;
    using UnityEditor;
    using UnityEngine;

    public static class PortalEditorPanel {
        public static void Draw(LevelEditorWorkspace workspace) {
            EditorGUILayout.LabelField(LevelEditorText.PortalPanel.SectionTitle, EditorStyles.boldLabel);

            if (workspace == null) {
                EditorGUILayout.HelpBox(LevelEditorText.PortalPanel.EmptyHelp, MessageType.Info);
                return;
            }

            if (GUILayout.Button(LevelEditorText.PortalPanel.AddButton)) {
                workspace.Portals.Add(LevelEditorWorkspace.CreateDefaultPortal($"portal_{workspace.Portals.Count + 1:00}"));
            }

            for (int i = 0; i < workspace.Portals.Count; i++) {
                var portal = workspace.Portals[i];
                if (portal == null) {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    portal.id = EditorGUILayout.TextField("ID", portal.id);
                    portal.inPos = EditorGUILayout.Vector2IntField(LevelEditorText.PortalPanel.EntranceLabel, portal.inPos);
                    portal.outPos = EditorGUILayout.Vector2IntField(LevelEditorText.PortalPanel.ExitLabel, portal.outPos);
                    portal.delay = EditorGUILayout.FloatField(LevelEditorText.PortalPanel.DelayLabel, portal.delay);
                    portal.color = EditorGUILayout.TextField(LevelEditorText.PortalPanel.ColorLabel, portal.color);

                    if (GUILayout.Button(LevelEditorText.PortalPanel.DeleteButton)) {
                        workspace.Portals.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}
