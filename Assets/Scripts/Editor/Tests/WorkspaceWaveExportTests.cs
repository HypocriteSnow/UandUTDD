using ArknightsLite.Config;
using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;
using ArknightsLite.Model;
using NUnit.Framework;
using UnityEditor;
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

        [Test]
        public void ReopenedWorkspace_TryBuildPathForWave_UsesPersistedSemanticIdsAndPortalGraph() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.MapWidth = 6;
            workspace.MapDepth = 1;
            var spawn = workspace.AddSpawnMarker(new Vector2Int(0, 0));
            var goal = workspace.AddGoalMarker(new Vector2Int(5, 0));
            workspace.AddPortalPair(new Vector2Int(1, 0), new Vector2Int(4, 0));
            workspace.SetTileOverride(2, 0, new TileData {
                x = 2,
                z = 0,
                tileType = TileType.Forbidden,
                heightLevel = 0,
                walkable = false,
                deployTag = "All"
            });
            workspace.SetTileOverride(3, 0, new TileData {
                x = 3,
                z = 0,
                tileType = TileType.Forbidden,
                heightLevel = 0,
                walkable = false,
                deployTag = "All"
            });

            var wave = LevelEditorWorkspace.CreateDefaultWave("wave_01");
            wave.spawnId = spawn.Id;
            wave.targetId = goal.Id;
            workspace.Waves.Add(wave);

            string assetPath = null;
            try {
                assetPath = LevelEditorWorkspaceRepository.SaveAsNewAsset(workspace);
                var restored = LevelEditorWorkspaceRepository.LoadAsset(assetPath);

                bool success = PathAutoFillService.TryBuildPathForWave(restored.Workspace, restored.Workspace.Waves[0], out var path);

                Assert.IsTrue(success);
                Assert.AreEqual(4, path.Count);
                Assert.AreEqual(new Vector2Int(0, 0), new Vector2Int(path[0].x, path[0].y));
                Assert.AreEqual(new Vector2Int(1, 0), new Vector2Int(path[1].x, path[1].y));
                Assert.AreEqual(new Vector2Int(4, 0), new Vector2Int(path[2].x, path[2].y));
                Assert.AreEqual(new Vector2Int(5, 0), new Vector2Int(path[3].x, path[3].y));
            } finally {
                if (!string.IsNullOrWhiteSpace(assetPath)) {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
        }

        public static void RunFromCommandLine() {
            new WorkspaceWaveExportTests().Export_CopiesWavePathAndReferencesIntoLevelConfig();
            new WorkspaceWaveExportTests().TryBuildPathForWave_UsesWorkspaceSpawnAndGoalMarkers();
            new WorkspaceWaveExportTests().ReopenedWorkspace_TryBuildPathForWave_UsesPersistedSemanticIdsAndPortalGraph();
            Debug.Log("[LevelEditorTests] WorkspaceWaveExportTests passed.");
        }
    }
}
