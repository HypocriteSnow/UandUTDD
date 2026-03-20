namespace ArknightsLite.Editor.LevelEditor.Panels {
    using System;
    using ArknightsLite.Config;
    using ArknightsLite.Editor.LevelEditor.Core;
    using UnityEditor;
    using UnityEngine;

    public static class WaveEditorPanel {
        public static int Draw(LevelEditorWorkspace workspace, int selectedWaveIndex) {
            EditorGUILayout.LabelField("波次", EditorStyles.boldLabel);

            if (workspace == null) {
                EditorGUILayout.HelpBox("创建工作区后可编辑波次。", MessageType.Info);
                return -1;
            }

            if (GUILayout.Button("添加波次")) {
                workspace.Waves.Add(LevelEditorWorkspace.CreateDefaultWave($"wave_{workspace.Waves.Count + 1:00}"));
                selectedWaveIndex = workspace.Waves.Count - 1;
            }

            if (workspace.Waves.Count == 0) {
                EditorGUILayout.HelpBox("当前还没有波次。", MessageType.Info);
                return -1;
            }

            selectedWaveIndex = selectedWaveIndex < 0 ? 0 : selectedWaveIndex;
            selectedWaveIndex = selectedWaveIndex >= workspace.Waves.Count ? workspace.Waves.Count - 1 : selectedWaveIndex;

            string[] waveOptions = new string[workspace.Waves.Count];
            for (int i = 0; i < workspace.Waves.Count; i++) {
                WaveDefinition wave = workspace.Waves[i];
                waveOptions[i] = wave != null && !string.IsNullOrWhiteSpace(wave.waveId) ? wave.waveId : $"wave_{i + 1:00}";
            }

            selectedWaveIndex = EditorGUILayout.Popup("当前波次", selectedWaveIndex, waveOptions);
            WaveDefinition selectedWave = workspace.Waves[selectedWaveIndex];
            if (selectedWave == null) {
                return selectedWaveIndex;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                selectedWave.waveId = EditorGUILayout.TextField("波次 ID", selectedWave.waveId);
                selectedWave.enemyId = EditorGUILayout.TextField("敌人 ID", selectedWave.enemyId);
                selectedWave.time = EditorGUILayout.FloatField("开始时间", selectedWave.time);
                selectedWave.count = EditorGUILayout.IntField("数量", selectedWave.count);
                selectedWave.interval = EditorGUILayout.FloatField("间隔", selectedWave.interval);
                selectedWave.spawnId = EditorGUILayout.TextField("出生点 ID", selectedWave.spawnId);
                selectedWave.targetId = EditorGUILayout.TextField("目标点 ID", selectedWave.targetId);
                EditorGUILayout.LabelField($"路径节点: {selectedWave.path.Count}");

                if (GUILayout.Button("删除当前波次")) {
                    workspace.Waves.RemoveAt(selectedWaveIndex);
                    return workspace.Waves.Count == 0 ? -1 : selectedWaveIndex - 1;
                }
            }

            return selectedWaveIndex;
        }
    }
}
