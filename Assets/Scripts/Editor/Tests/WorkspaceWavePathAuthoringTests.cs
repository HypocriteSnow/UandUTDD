using System.Reflection;
using ArknightsLite.Config;
using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Panels;
using ArknightsLite.Model;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class WorkspaceWavePathAuthoringTests {
        [Test]
        public void GetWaveSemanticOptions_ReturnsCurrentSpawnAndGoalIds() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.AddSpawnMarker(new Vector2Int(0, 0));
            workspace.AddSpawnMarker(new Vector2Int(1, 0));
            workspace.AddGoalMarker(new Vector2Int(5, 0));

            MethodInfo spawnOptionsMethod = typeof(WaveEditorPanel).GetMethod(
                "GetSpawnOptions",
                BindingFlags.Public | BindingFlags.Static);
            MethodInfo goalOptionsMethod = typeof(WaveEditorPanel).GetMethod(
                "GetGoalOptions",
                BindingFlags.Public | BindingFlags.Static);

            Assert.NotNull(spawnOptionsMethod, "WaveEditorPanel.GetSpawnOptions should exist.");
            Assert.NotNull(goalOptionsMethod, "WaveEditorPanel.GetGoalOptions should exist.");

            string[] spawnOptions = (string[])spawnOptionsMethod.Invoke(null, new object[] { workspace });
            string[] goalOptions = (string[])goalOptionsMethod.Invoke(null, new object[] { workspace });

            CollectionAssert.AreEqual(new[] { "R1", "R2" }, spawnOptions);
            CollectionAssert.AreEqual(new[] { "B1" }, goalOptions);
        }

        [Test]
        public void GenerateWavePath_UsesSelectedSpawnAndGoalIdsWithPortalAwareAStar() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.MapWidth = 6;
            workspace.MapDepth = 1;

            var spawn = workspace.AddSpawnMarker(new Vector2Int(0, 0));
            var goal = workspace.AddGoalMarker(new Vector2Int(5, 0));
            workspace.AddPortalPair(new Vector2Int(1, 0), new Vector2Int(4, 0));
            workspace.SetTileOverride(2, 0, CreateBlockedTile(2, 0));
            workspace.SetTileOverride(3, 0, CreateBlockedTile(3, 0));

            var wave = LevelEditorWorkspace.CreateDefaultWave("wave_01");
            wave.spawnId = spawn.Id;
            wave.targetId = goal.Id;
            workspace.Waves.Add(wave);

            MethodInfo generateMethod = typeof(PathEditorPanel).GetMethod(
                "TryGeneratePathForSelectedWave",
                BindingFlags.Public | BindingFlags.Static);

            Assert.NotNull(generateMethod, "PathEditorPanel.TryGeneratePathForSelectedWave should exist.");

            bool success = (bool)generateMethod.Invoke(null, new object[] { workspace, 0 });

            Assert.IsTrue(success);
            Assert.AreEqual(4, wave.path.Count);
            Assert.AreEqual(new Vector2Int(0, 0), new Vector2Int(wave.path[0].x, wave.path[0].y));
            Assert.AreEqual(new Vector2Int(1, 0), new Vector2Int(wave.path[1].x, wave.path[1].y));
            Assert.AreEqual(new Vector2Int(4, 0), new Vector2Int(wave.path[2].x, wave.path[2].y));
            Assert.AreEqual(new Vector2Int(5, 0), new Vector2Int(wave.path[3].x, wave.path[3].y));
        }

        [Test]
        public void ClearAndToggleWavePath_UpdatesSelectedWaveNodes() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            var wave = LevelEditorWorkspace.CreateDefaultWave("wave_01");
            wave.path.Add(new PathNodeDefinition { x = 1, y = 0, wait = 0f });
            wave.path.Add(new PathNodeDefinition { x = 2, y = 0, wait = 0f });
            workspace.Waves.Add(wave);

            MethodInfo clearMethod = typeof(PathEditorPanel).GetMethod(
                "ClearPathForSelectedWave",
                BindingFlags.Public | BindingFlags.Static);
            MethodInfo toggleMethod = typeof(PathEditorPanel).GetMethod(
                "TogglePathNodeForSelectedWave",
                BindingFlags.Public | BindingFlags.Static);

            Assert.NotNull(clearMethod, "PathEditorPanel.ClearPathForSelectedWave should exist.");
            Assert.NotNull(toggleMethod, "PathEditorPanel.TogglePathNodeForSelectedWave should exist.");

            bool cleared = (bool)clearMethod.Invoke(null, new object[] { workspace, 0 });
            Assert.IsTrue(cleared);
            Assert.AreEqual(0, wave.path.Count);

            bool added = (bool)toggleMethod.Invoke(null, new object[] { workspace, 0, new Vector2Int(3, 0) });
            Assert.IsTrue(added);
            Assert.AreEqual(1, wave.path.Count);
            Assert.AreEqual(3, wave.path[0].x);

            bool removed = (bool)toggleMethod.Invoke(null, new object[] { workspace, 0, new Vector2Int(3, 0) });
            Assert.IsTrue(removed);
            Assert.AreEqual(0, wave.path.Count);
        }

        public static void RunFromCommandLine() {
            var tests = new WorkspaceWavePathAuthoringTests();
            tests.GetWaveSemanticOptions_ReturnsCurrentSpawnAndGoalIds();
            tests.GenerateWavePath_UsesSelectedSpawnAndGoalIdsWithPortalAwareAStar();
            tests.ClearAndToggleWavePath_UpdatesSelectedWaveNodes();
            Debug.Log("[LevelEditorTests] WorkspaceWavePathAuthoringTests passed.");
        }

        private static TileData CreateBlockedTile(int x, int z) {
            return new TileData {
                x = x,
                z = z,
                tileType = TileType.Forbidden,
                heightLevel = 0,
                walkable = false,
                deployTag = "All"
            };
        }
    }
}
