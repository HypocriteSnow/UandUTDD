using ArknightsLite.Config;
using ArknightsLite.Model;
using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelEditorWorkspaceTests {
        [Test]
        public void CreateNewWorkspace_DefaultsMapToForbiddenTerrain() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            Assert.AreEqual(TileType.Forbidden, workspace.DefaultTileType);
            Assert.AreEqual(TileType.Forbidden, workspace.GetTileOverride(0, 0).tileType);
        }

        [Test]
        public void CreateNewWorkspace_UsesLevelNameAndInitializesRuntimeParameters() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            Assert.AreEqual("Tutorial_01", workspace.LevelName);
            Assert.AreEqual(10, workspace.MapWidth);
            Assert.AreEqual(10, workspace.MapDepth);
            Assert.AreEqual(20, workspace.Runtime.InitialDp);
            Assert.AreEqual(3, workspace.Runtime.BaseHealth);
        }

        [Test]
        public void CreateNewWorkspace_DoesNotResolveImplicitSpawnOrGoalFallback() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            Assert.IsFalse(workspace.TryResolveSpawn(string.Empty, out _));
            Assert.IsFalse(workspace.TryResolveGoal(string.Empty, out _));
        }

        [Test]
        public void CreateDefaultWave_StartsWithoutSemanticReferences() {
            var wave = LevelEditorWorkspace.CreateDefaultWave("wave_01");

            Assert.IsTrue(string.IsNullOrWhiteSpace(wave.spawnId));
            Assert.IsTrue(string.IsNullOrWhiteSpace(wave.targetId));
        }

        [Test]
        public void SavedWorkspace_ReopensAndCanExportWithoutManualRepair() {
            var workspace = LevelEditorWorkspace.CreateNew("Workspace_T7");
            workspace.ExportName = "Episode_T7";
            workspace.MapWidth = 7;
            workspace.MapDepth = 6;
            workspace.Runtime.InitialDp = 33;
            workspace.Runtime.BaseHealth = 5;
            workspace.Runtime.DpRecoveryInterval = 0.75f;
            workspace.Runtime.DpRecoveryAmount = 2;
            workspace.AddSpawnMarker(new Vector2Int(1, 1));
            workspace.AddSpawnMarker(new Vector2Int(2, 1));
            var goal = workspace.AddGoalMarker(new Vector2Int(5, 4));
            workspace.AddPortalPair(new Vector2Int(3, 1), new Vector2Int(4, 3));
            workspace.Enemies.Add(new EnemyTemplateData { id = "enemy_01", name = "Enemy" });

            var wave = LevelEditorWorkspace.CreateDefaultWave("wave_01");
            wave.enemyId = "enemy_01";
            wave.spawnId = "R1";
            wave.targetId = goal.Id;
            wave.path.Add(new PathNodeDefinition { x = 1, y = 1, wait = 0f });
            wave.path.Add(new PathNodeDefinition { x = 3, y = 1, wait = 0f });
            wave.path.Add(new PathNodeDefinition { x = 4, y = 3, wait = 0f });
            wave.path.Add(new PathNodeDefinition { x = 5, y = 4, wait = 0f });
            workspace.Waves.Add(wave);

            string assetPath = null;
            try {
                assetPath = LevelEditorWorkspaceRepository.SaveAsNewAsset(workspace);
                var restored = LevelEditorWorkspaceRepository.LoadAsset(assetPath);
                var config = LevelConfigExportService.BuildTransientConfig(restored.Workspace);

                Assert.AreEqual("Episode_T7", restored.Workspace.ExportName);
                Assert.AreEqual(2, restored.Workspace.SpawnMarkers.Count);
                Assert.AreEqual(1, restored.Workspace.GoalMarkers.Count);
                Assert.AreEqual(1, restored.Workspace.PortalPairs.Count);
                Assert.AreEqual(1, restored.Workspace.Waves.Count);
                Assert.AreEqual("Episode_T7_LevelConfig", config.name);
                Assert.AreEqual(2, config.spawnPoints.Count);
                Assert.AreEqual(goal.Position, config.goalPoint);
                Assert.AreEqual("R1", config.waves[0].spawnId);
                Assert.AreEqual(goal.Id, config.waves[0].targetId);
                Assert.AreEqual(4, config.waves[0].path.Count);
                Object.DestroyImmediate(config);
            } finally {
                if (!string.IsNullOrWhiteSpace(assetPath)) {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
        }

        public static void RunFromCommandLine() {
            new LevelEditorWorkspaceTests().CreateNewWorkspace_DefaultsMapToForbiddenTerrain();
            new LevelEditorWorkspaceTests().CreateNewWorkspace_UsesLevelNameAndInitializesRuntimeParameters();
            new LevelEditorWorkspaceTests().CreateNewWorkspace_DoesNotResolveImplicitSpawnOrGoalFallback();
            new LevelEditorWorkspaceTests().CreateDefaultWave_StartsWithoutSemanticReferences();
            new LevelEditorWorkspaceTests().SavedWorkspace_ReopensAndCanExportWithoutManualRepair();
            Debug.Log("[LevelEditorTests] LevelEditorWorkspaceTests passed.");
        }
    }
}
