using ArknightsLite.Config;
using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class WorkspaceWaveExportTests {
        [Test]
        public void Export_CopiesWavePathAndReferencesIntoLevelConfig() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.Waves.Add(LevelEditorWorkspace.CreateDefaultWave("wave_01"));
            workspace.Waves[0].enemyId = "enemy_01";
            workspace.Waves[0].path.Add(new PathNodeDefinition { x = 1, y = 2, wait = 0f });

            var config = LevelConfigExportService.BuildTransientConfig(workspace);

            Assert.AreEqual(1, config.waves.Count);
            Assert.AreEqual("enemy_01", config.waves[0].enemyId);
            Assert.AreEqual(1, config.waves[0].path.Count);
        }

        [Test]
        public void TryBuildPathForWave_UsesWorkspaceSpawnAndGoalMarkers() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.MapWidth = 5;
            workspace.MapDepth = 5;
            workspace.SetSpawnPoint(new Vector2Int(1, 1));
            workspace.SetGoalPoint(new Vector2Int(3, 2));

            var wave = LevelEditorWorkspace.CreateDefaultWave("wave_01");

            bool success = PathAutoFillService.TryBuildPathForWave(workspace, wave, out var path);

            Assert.IsTrue(success);
            Assert.AreEqual(new Vector2Int(1, 1), new Vector2Int(path[0].x, path[0].y));
            Assert.AreEqual(new Vector2Int(3, 2), new Vector2Int(path[path.Count - 1].x, path[path.Count - 1].y));
        }

        public static void RunFromCommandLine() {
            new WorkspaceWaveExportTests().Export_CopiesWavePathAndReferencesIntoLevelConfig();
            new WorkspaceWaveExportTests().TryBuildPathForWave_UsesWorkspaceSpawnAndGoalMarkers();
            Debug.Log("[LevelEditorTests] WorkspaceWaveExportTests passed.");
        }
    }
}
