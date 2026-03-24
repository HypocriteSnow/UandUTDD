using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Map;
using ArknightsLite.Editor.LevelEditor.Services;
using ArknightsLite.Model;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class WorkspaceMapControllerTests {
        [Test]
        public void PaintTile_WritesOverrideIntoWorkspace() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            var controller = new WorkspaceMapController(workspace);

            controller.PaintTile(2, 1, TileType.HighGround, 1);

            var tile = workspace.GetTileOverride(2, 1);
            Assert.AreEqual(TileType.HighGround, tile.tileType);
            Assert.AreEqual(1, tile.heightLevel);
        }

        [Test]
        public void PlaceSpawnMarker_RendersSemanticVisualOnSelectedTile() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            var controller = new WorkspaceMapController(workspace);

            var existingRoot = Object.FindObjectOfType<WhiteboxRoot>();
            if (existingRoot != null) {
                Object.DestroyImmediate(existingRoot.gameObject);
            }

            var root = WhiteboxGenerationService.GenerateIntoOpenScene(workspace);

            try {
                var updatedTile = FindTile(root, 2, 1);

                Assert.IsNotNull(updatedTile);
                Assert.IsNull(updatedTile.transform.Find("SpawnMarkerVisual"));

                controller.PlaceSpawnMarker(2, 1);

                Assert.IsNotNull(updatedTile.transform.Find("SpawnMarkerVisual"));
                Assert.AreEqual("R1", updatedTile.SemanticLabel);
            } finally {
                if (root != null) {
                    Object.DestroyImmediate(root.gameObject);
                }
            }
        }

        [Test]
        public void PlaceGoalMarker_RendersSemanticVisualOnSelectedTile() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            var controller = new WorkspaceMapController(workspace);

            var existingRoot = Object.FindObjectOfType<WhiteboxRoot>();
            if (existingRoot != null) {
                Object.DestroyImmediate(existingRoot.gameObject);
            }

            var root = WhiteboxGenerationService.GenerateIntoOpenScene(workspace);

            try {
                var semanticTile = FindTile(root, 2, 1);
                Assert.IsNotNull(semanticTile);
                Assert.IsNull(semanticTile.transform.Find("GoalMarkerVisual"));

                controller.PlaceGoalMarker(2, 1);

                Assert.IsNotNull(semanticTile.transform.Find("GoalMarkerVisual"));
                Assert.AreEqual("B1", semanticTile.SemanticLabel);
            } finally {
                if (root != null) {
                    Object.DestroyImmediate(root.gameObject);
                }
            }
        }

        private static TileAuthoring FindTile(WhiteboxRoot root, int x, int z) {
            foreach (var tile in root.GetComponentsInChildren<TileAuthoring>()) {
                if (tile.X == x && tile.Z == z) {
                    return tile;
                }
            }

            return null;
        }

        public static void RunFromCommandLine() {
            new WorkspaceMapControllerTests().PaintTile_WritesOverrideIntoWorkspace();
            new WorkspaceMapControllerTests().PlaceSpawnMarker_RendersSemanticVisualOnSelectedTile();
            new WorkspaceMapControllerTests().PlaceGoalMarker_RendersSemanticVisualOnSelectedTile();
            Debug.Log("[LevelEditorTests] WorkspaceMapControllerTests passed.");
        }
    }
}
