namespace ArknightsLite.Editor.LevelEditor.Panels {
    using System;
    using ArknightsLite.Config;
    using ArknightsLite.Editor.LevelEditor.Core;
    using UnityEditor;
    using UnityEngine;

    public static class WaveEditorPanel {
        public static int Draw(LevelEditorWorkspace workspace, int selectedWaveIndex) {
            EditorGUILayout.LabelField(LevelEditorText.WavePanel.SectionTitle, EditorStyles.boldLabel);

            if (workspace == null) {
                EditorGUILayout.HelpBox(LevelEditorText.WavePanel.EmptyWorkspaceHelp, MessageType.Info);
                return -1;
            }

            if (GUILayout.Button(LevelEditorText.WavePanel.AddButton)) {
                var nextWave = LevelEditorWorkspace.CreateDefaultWave($"wave_{workspace.Waves.Count + 1:00}");
                InitializeWaveSemanticIds(workspace, nextWave);
                workspace.Waves.Add(nextWave);
                selectedWaveIndex = workspace.Waves.Count - 1;
            }

            if (workspace.Waves.Count == 0) {
                EditorGUILayout.HelpBox(LevelEditorText.WavePanel.EmptyHelp, MessageType.Info);
                return -1;
            }

            selectedWaveIndex = selectedWaveIndex < 0 ? 0 : selectedWaveIndex;
            selectedWaveIndex = selectedWaveIndex >= workspace.Waves.Count ? workspace.Waves.Count - 1 : selectedWaveIndex;

            string[] waveOptions = new string[workspace.Waves.Count];
            for (int i = 0; i < workspace.Waves.Count; i++) {
                WaveDefinition wave = workspace.Waves[i];
                waveOptions[i] = wave != null && !string.IsNullOrWhiteSpace(wave.waveId) ? wave.waveId : $"wave_{i + 1:00}";
            }

            selectedWaveIndex = EditorGUILayout.Popup(LevelEditorText.WavePanel.CurrentWaveLabel, selectedWaveIndex, waveOptions);
            WaveDefinition selectedWave = workspace.Waves[selectedWaveIndex];
            if (selectedWave == null) {
                return selectedWaveIndex;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                selectedWave.waveId = EditorGUILayout.TextField(LevelEditorText.WavePanel.WaveIdLabel, selectedWave.waveId);
                selectedWave.enemyId = EditorGUILayout.TextField(LevelEditorText.WavePanel.EnemyIdLabel, selectedWave.enemyId);
                selectedWave.time = EditorGUILayout.FloatField(LevelEditorText.WavePanel.StartTimeLabel, selectedWave.time);
                selectedWave.count = EditorGUILayout.IntField(LevelEditorText.WavePanel.CountLabel, selectedWave.count);
                selectedWave.interval = EditorGUILayout.FloatField(LevelEditorText.WavePanel.IntervalLabel, selectedWave.interval);
                DrawSemanticSelectionField(LevelEditorText.WavePanel.SpawnIdLabel, GetSpawnOptions(workspace), ref selectedWave.spawnId);
                DrawSemanticSelectionField(LevelEditorText.WavePanel.TargetIdLabel, GetGoalOptions(workspace), ref selectedWave.targetId);
                EditorGUILayout.LabelField(LevelEditorText.WavePanel.PathNodesLabel(selectedWave.path.Count));

                if (GUILayout.Button(LevelEditorText.WavePanel.DeleteButton)) {
                    workspace.Waves.RemoveAt(selectedWaveIndex);
                    return workspace.Waves.Count == 0 ? -1 : selectedWaveIndex - 1;
                }
            }

            return selectedWaveIndex;
        }

        public static string[] GetSpawnOptions(LevelEditorWorkspace workspace) {
            return GetMarkerOptions(workspace?.SpawnMarkers);
        }

        public static string[] GetGoalOptions(LevelEditorWorkspace workspace) {
            return GetMarkerOptions(workspace?.GoalMarkers);
        }

        private static void InitializeWaveSemanticIds(LevelEditorWorkspace workspace, WaveDefinition wave) {
            if (wave == null) {
                return;
            }

            string[] spawnOptions = GetSpawnOptions(workspace);
            string[] goalOptions = GetGoalOptions(workspace);

            if (spawnOptions.Length > 0) {
                wave.spawnId = spawnOptions[0];
            }

            if (goalOptions.Length > 0) {
                wave.targetId = goalOptions[0];
            }
        }

        private static void DrawSemanticSelectionField(string label, string[] options, ref string selectedValue) {
            if (options == null || options.Length == 0) {
                selectedValue = EditorGUILayout.TextField(label, selectedValue);
                return;
            }

            int selectedIndex = Array.IndexOf(options, selectedValue);
            selectedIndex = Mathf.Max(0, selectedIndex);
            int nextIndex = EditorGUILayout.Popup(label, selectedIndex, options);
            selectedValue = options[Mathf.Clamp(nextIndex, 0, options.Length - 1)];
        }

        private static string[] GetMarkerOptions(System.Collections.Generic.List<LevelEditorWorkspace.SemanticMarker> markers) {
            if (markers != null && markers.Count > 0) {
                string[] options = new string[markers.Count];
                for (int i = 0; i < markers.Count; i++) {
                    options[i] = markers[i] != null && !string.IsNullOrWhiteSpace(markers[i].Id)
                        ? markers[i].Id
                        : $"marker_{i + 1:00}";
                }

                return options;
            }

            return Array.Empty<string>();
        }
    }
}
