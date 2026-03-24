using System.Reflection;
using ArknightsLite.Config;
using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelEditorValidationWorkflowTests {
        [Test]
        public void Export_UsesExportNameInsteadOfWorkspaceName() {
            var workspace = LevelEditorWorkspace.CreateNew("Workspace_A");
            workspace.ExportName = "Episode_01";

            string assetName = LevelConfigExportService.BuildAssetName(workspace);

            Assert.AreEqual("Episode_01_LevelConfig", assetName);
        }

        [Test]
        public void BuildTransientConfig_UsesSemanticMarkersForRuntimeSpawnAndGoal() {
            var workspace = LevelEditorWorkspace.CreateNew("Workspace_A");
            workspace.AddSpawnMarker(new Vector2Int(2, 1));
            workspace.AddGoalMarker(new Vector2Int(6, 4));

            LevelConfig config = LevelConfigExportService.BuildTransientConfig(workspace);

            Assert.AreEqual(1, config.spawnPoints.Count);
            Assert.AreEqual(new Vector2Int(2, 1), config.spawnPoints[0]);
            Assert.AreEqual(new Vector2Int(6, 4), config.goalPoint);
        }

        [Test]
        public void ValidateWorkspace_FailsWhenExportNameSemanticReferencesOrPathAreInvalid() {
            var workspace = LevelEditorWorkspace.CreateNew("Workspace_A");
            workspace.LevelName = string.Empty;
            workspace.ExportName = string.Empty;

            var wave = LevelEditorWorkspace.CreateDefaultWave("wave_01");
            wave.spawnId = "R99";
            wave.targetId = "B99";
            wave.path.Clear();
            workspace.Waves.Add(wave);

            MethodInfo validateWorkspaceMethod = typeof(LevelValidationService).GetMethod(
                "ValidateWorkspace",
                BindingFlags.Public | BindingFlags.Static);

            Assert.NotNull(validateWorkspaceMethod, "LevelValidationService.ValidateWorkspace should exist.");

            var result = (LevelValidationResult)validateWorkspaceMethod.Invoke(null, new object[] { workspace });

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("Export"));
            Assert.That(result.Errors, Has.Some.Contains("R99"));
            Assert.That(result.Errors, Has.Some.Contains("B99"));
            Assert.That(result.Errors, Has.Some.Contains("path"));
        }

        public static void RunFromCommandLine() {
            var tests = new LevelEditorValidationWorkflowTests();
            tests.Export_UsesExportNameInsteadOfWorkspaceName();
            tests.BuildTransientConfig_UsesSemanticMarkersForRuntimeSpawnAndGoal();
            tests.ValidateWorkspace_FailsWhenExportNameSemanticReferencesOrPathAreInvalid();
            Debug.Log("[LevelEditorTests] LevelEditorValidationWorkflowTests passed.");
        }
    }
}
