namespace ArknightsLite.Editor.LevelEditor.Panels {
    using System.Collections.Generic;
    using ArknightsLite.Config;
    using ArknightsLite.Editor.LevelEditor.Core;
    using ArknightsLite.Editor.LevelEditor.Services;
    using UnityEditor;
    using UnityEngine;

    public static class PathEditorPanel {
        public static void Draw(LevelEditorWorkspace workspace, int selectedWaveIndex) {
            EditorGUILayout.LabelField("Paths", EditorStyles.boldLabel);

            if (workspace == null) {
                EditorGUILayout.HelpBox("Create a workspace before editing paths.", MessageType.Info);
                return;
            }

            if (!TryGetSelectedWave(workspace, selectedWaveIndex, out WaveDefinition wave)) {
                EditorGUILayout.HelpBox("Create and select a wave before editing paths.", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.LabelField($"Current Wave: {wave.waveId}");

                if (GUILayout.Button("Generate Path")) {
                    TryGeneratePathForSelectedWave(workspace, selectedWaveIndex);
                }

                if (GUILayout.Button("Clear Path")) {
                    ClearPathForSelectedWave(workspace, selectedWaveIndex);
                }

                if (GUILayout.Button("Add Path Node")) {
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
                    if (GUILayout.Button("Delete", GUILayout.Width(56))) {
                        wave.path.RemoveAt(i);
                        EditorGUILayout.EndHorizontal();
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        public static bool TryGeneratePathForSelectedWave(LevelEditorWorkspace workspace, int selectedWaveIndex) {
            if (!TryGetSelectedWave(workspace, selectedWaveIndex, out WaveDefinition wave)) {
                return false;
            }

            if (!PathAutoFillService.TryBuildPathForWave(workspace, wave, out List<PathNodeDefinition> generatedPath)) {
                return false;
            }

            wave.path = generatedPath;
            return true;
        }

        public static bool ClearPathForSelectedWave(LevelEditorWorkspace workspace, int selectedWaveIndex) {
            if (!TryGetSelectedWave(workspace, selectedWaveIndex, out WaveDefinition wave)) {
                return false;
            }

            if (wave.path == null) {
                wave.path = new List<PathNodeDefinition>();
            } else {
                wave.path.Clear();
            }

            return true;
        }

        public static bool TogglePathNodeForSelectedWave(LevelEditorWorkspace workspace, int selectedWaveIndex, Vector2Int position) {
            if (!TryGetSelectedWave(workspace, selectedWaveIndex, out WaveDefinition wave)) {
                return false;
            }

            wave.path = wave.path ?? new List<PathNodeDefinition>();

            for (int i = 0; i < wave.path.Count; i++) {
                var node = wave.path[i];
                if (node == null) {
                    continue;
                }

                if (node.x == position.x && node.y == position.y) {
                    wave.path.RemoveAt(i);
                    return true;
                }
            }

            wave.path.Add(new PathNodeDefinition {
                x = position.x,
                y = position.y,
                wait = 0f
            });
            return true;
        }

        private static bool TryGetSelectedWave(LevelEditorWorkspace workspace, int selectedWaveIndex, out WaveDefinition wave) {
            wave = null;
            if (workspace == null || workspace.Waves == null || selectedWaveIndex < 0 || selectedWaveIndex >= workspace.Waves.Count) {
                return false;
            }

            wave = workspace.Waves[selectedWaveIndex];
            return wave != null;
        }
    }
}
