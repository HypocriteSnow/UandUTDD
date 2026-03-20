using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class WhiteboxGenerationServiceTests {
        [Test]
        public void EnsureWhitebox_CreatesExpectedTileCountFromWorkspaceSize() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.MapWidth = 4;
            workspace.MapDepth = 3;

            var result = WhiteboxGenerationService.BuildPreview(workspace);

            Assert.AreEqual(12, result.TileCount);
        }

        [Test]
        public void GenerateIntoOpenScene_AssignsSpawnAndGoalMarkersToMatchingTiles() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.SetSpawnPoint(new Vector2Int(1, 1));
            workspace.SetGoalPoint(new Vector2Int(3, 2));

            var existingRoot = Object.FindObjectOfType<WhiteboxRoot>();
            if (existingRoot != null) {
                Object.DestroyImmediate(existingRoot.gameObject);
            }

            var root = WhiteboxGenerationService.GenerateIntoOpenScene(workspace);

            try {
                TileAuthoring spawnTile = null;
                TileAuthoring goalTile = null;
                foreach (var tile in root.GetComponentsInChildren<TileAuthoring>()) {
                    if (tile.X == 1 && tile.Z == 1) {
                        spawnTile = tile;
                    }

                    if (tile.X == 3 && tile.Z == 2) {
                        goalTile = tile;
                    }
                }

                Assert.IsNotNull(spawnTile);
                Assert.IsNotNull(goalTile);
                Assert.IsTrue(spawnTile.HasSpawnMarker);
                Assert.IsTrue(goalTile.HasGoalMarker);
                Assert.IsNotNull(spawnTile.transform.Find("SpawnMarkerVisual"));
                Assert.IsNotNull(goalTile.transform.Find("GoalMarkerVisual"));
            } finally {
                if (root != null) {
                    Object.DestroyImmediate(root.gameObject);
                }
            }
        }

        public static void RunFromCommandLine() {
            new WhiteboxGenerationServiceTests().EnsureWhitebox_CreatesExpectedTileCountFromWorkspaceSize();
            new WhiteboxGenerationServiceTests().GenerateIntoOpenScene_AssignsSpawnAndGoalMarkersToMatchingTiles();
            Debug.Log("[LevelEditorTests] WhiteboxGenerationServiceTests passed.");
        }
    }
}
