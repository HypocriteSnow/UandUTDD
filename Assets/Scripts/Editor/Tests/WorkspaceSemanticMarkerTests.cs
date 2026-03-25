using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Map;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class WorkspaceSemanticMarkerTests {
        [Test]
        public void AddSpawnMarker_AssignsStableReadableId() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            var marker = workspace.AddSpawnMarker(new Vector2Int(1, 1));

            Assert.AreEqual("R1", marker.Id);
            Assert.AreEqual(new Vector2Int(1, 1), marker.Position);
        }

        [Test]
        public void AddGoalMarker_AssignsStableReadableId() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            var marker = workspace.AddGoalMarker(new Vector2Int(2, 2));

            Assert.AreEqual("B1", marker.Id);
            Assert.AreEqual(new Vector2Int(2, 2), marker.Position);
        }

        [Test]
        public void AddPortalPair_AssignsStableReadableIds() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            var portalPair = workspace.AddPortalPair(new Vector2Int(3, 3), new Vector2Int(4, 4));

            Assert.AreEqual("IN1", portalPair.EntranceId);
            Assert.AreEqual("OUT1", portalPair.ExitId);
            Assert.AreEqual(new Vector2Int(3, 3), portalPair.EntrancePosition);
            Assert.AreEqual(new Vector2Int(4, 4), portalPair.ExitPosition);
        }

        [Test]
        public void PlacePortalPair_OverwritesExistingSpawnMarkerAndUsesNextStablePairIds() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            var controller = new WorkspaceMapController(workspace);

            var firstPair = controller.PlacePortalPair(new Vector2Int(4, 4), new Vector2Int(5, 5));
            controller.PlaceSpawnMarker(1, 1);

            var replacedPair = controller.PlacePortalPair(new Vector2Int(1, 1), new Vector2Int(2, 2));

            Assert.AreEqual("IN1", firstPair.EntranceId);
            Assert.AreEqual("IN2", replacedPair.EntranceId);
            Assert.AreEqual("OUT2", replacedPair.ExitId);
            Assert.IsFalse(workspace.IsSpawnMarker("R1", 1, 1));
            CollectionAssert.AreEqual(new[] { "IN2" }, workspace.GetSemanticLabelsAt(1, 1));
        }

        public static void RunFromCommandLine() {
            new WorkspaceSemanticMarkerTests().AddSpawnMarker_AssignsStableReadableId();
            new WorkspaceSemanticMarkerTests().AddGoalMarker_AssignsStableReadableId();
            new WorkspaceSemanticMarkerTests().AddPortalPair_AssignsStableReadableIds();
            new WorkspaceSemanticMarkerTests().PlacePortalPair_OverwritesExistingSpawnMarkerAndUsesNextStablePairIds();
            Debug.Log("[LevelEditorTests] WorkspaceSemanticMarkerTests passed.");
        }
    }
}
