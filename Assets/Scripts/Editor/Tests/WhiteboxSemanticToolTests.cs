using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Map;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class WhiteboxSemanticToolTests {
        [Test]
        public void PlaceGoalTool_ReplacesMarkerAtClickedTileOnly() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            var controller = new WorkspaceMapController(workspace);

            controller.PlaceGoalMarker(2, 2);
            controller.PlaceGoalMarker(4, 3);
            controller.PlaceGoalMarker(4, 3);

            Assert.AreEqual(2, workspace.GoalMarkers.Count);
            Assert.IsTrue(workspace.IsGoalMarker("B1", 2, 2));
            Assert.IsTrue(workspace.IsGoalMarker("B2", 4, 3));
        }

        public static void RunFromCommandLine() {
            new WhiteboxSemanticToolTests().PlaceGoalTool_ReplacesMarkerAtClickedTileOnly();
            Debug.Log("[LevelEditorTests] WhiteboxSemanticToolTests passed.");
        }
    }
}
