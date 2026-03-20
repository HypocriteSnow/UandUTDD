namespace ArknightsLite.Editor.LevelEditor.Panels {
    using System.Collections.Generic;
    using ArknightsLite.Config;
    using ArknightsLite.Editor.LevelEditor.Core;
    using ArknightsLite.Editor.LevelEditor.Services;
    using UnityEditor;
    using UnityEngine;

    public static class PathEditorPanel {
        public static void Draw(LevelEditorWorkspace workspace, int selectedWaveIndex) {
            EditorGUILayout.LabelField("路径", EditorStyles.boldLabel);

            if (workspace == null) {
                EditorGUILayout.HelpBox("创建工作区后可编辑路径。", MessageType.Info);
                return;
            }

            if (workspace.Waves.Count == 0 || selectedWaveIndex < 0 || selectedWaveIndex >= workspace.Waves.Count) {
                EditorGUILayout.HelpBox("先在波次面板里创建一个波次。", MessageType.Info);
                return;
            }

            var wave = workspace.Waves[selectedWaveIndex];
            if (wave == null) {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.LabelField($"当前波次: {wave.waveId}");

                if (GUILayout.Button("自动填充到终点")) {
                    if (PathAutoFillService.TryBuildPathForWave(workspace, wave, out List<PathNodeDefinition> generatedPath)) {
                        wave.path = generatedPath;
                    }
                }

                if (GUILayout.Button("添加路径节点")) {
                    wave.path.Add(new PathNodeDefinition());
                }

                for (int i = 0; i < wave.path.Count; i++) {
                    var node = wave.path[i];
                    if (node == null) {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    node.x = EditorGUILayout.IntField("X", node.x);
                    node.y = EditorGUILayout.IntField("Y", node.y);
                    node.wait = EditorGUILayout.FloatField("Wait", node.wait);
                    if (GUILayout.Button("删", GUILayout.Width(32))) {
                        wave.path.RemoveAt(i);
                        EditorGUILayout.EndHorizontal();
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}
