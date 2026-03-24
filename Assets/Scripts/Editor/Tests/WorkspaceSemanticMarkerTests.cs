using ArknightsLite.Editor.LevelEditor.Core;
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

        public static void RunFromCommandLine() {
            new WorkspaceSemanticMarkerTests().AddSpawnMarker_AssignsStableReadableId();
            new WorkspaceSemanticMarkerTests().AddGoalMarker_AssignsStableReadableId();
            new WorkspaceSemanticMarkerTests().AddPortalPair_AssignsStableReadableIds();
            Debug.Log("[LevelEditorTests] WorkspaceSemanticMarkerTests passed.");
        }
    }
}
