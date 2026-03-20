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
        public void SetSpawnPoint_MovesSpawnMarkerVisualToNewTile() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            var controller = new WorkspaceMapController(workspace);

            var existingRoot = Object.FindObjectOfType<WhiteboxRoot>();
            if (existingRoot != null) {
                Object.DestroyImmediate(existingRoot.gameObject);
            }

            var root = WhiteboxGenerationService.GenerateIntoOpenScene(workspace);

            try {
                var originalTile = FindTile(root, 0, 0);
                var updatedTile = FindTile(root, 2, 1);

                Assert.IsNotNull(originalTile);
                Assert.IsNotNull(updatedTile);
                Assert.IsNotNull(originalTile.transform.Find("SpawnMarkerVisual"));
                Assert.IsNull(updatedTile.transform.Find("SpawnMarkerVisual"));

                controller.SetSpawnPoint(2, 1);

                Assert.IsNull(originalTile.transform.Find("SpawnMarkerVisual"));
                Assert.IsNotNull(updatedTile.transform.Find("SpawnMarkerVisual"));
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
            new WorkspaceMapControllerTests().SetSpawnPoint_MovesSpawnMarkerVisualToNewTile();
            Debug.Log("[LevelEditorTests] WorkspaceMapControllerTests passed.");
        }
    }
}
