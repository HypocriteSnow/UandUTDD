using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Map;
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

        public static void RunFromCommandLine() {
            new WorkspaceMapControllerTests().PaintTile_WritesOverrideIntoWorkspace();
            Debug.Log("[LevelEditorTests] WorkspaceMapControllerTests passed.");
        }
    }
}
