using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;
using NUnit.Framework;
using UnityEditor;
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
            workspace.AddSpawnMarker(new Vector2Int(1, 1));
            workspace.AddGoalMarker(new Vector2Int(3, 2));

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

        [Test]
        public void GenerateIntoOpenScene_NewWorkspaceDoesNotRenderImplicitLegacySpawnOrGoal() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            var existingRoot = Object.FindObjectOfType<WhiteboxRoot>();
            if (existingRoot != null) {
                Object.DestroyImmediate(existingRoot.gameObject);
            }

            var root = WhiteboxGenerationService.GenerateIntoOpenScene(workspace);

            try {
                var originTile = FindTile(root, 0, 0);
                var cornerTile = FindTile(root, workspace.MapWidth - 1, workspace.MapDepth - 1);

                Assert.IsNotNull(originTile);
                Assert.IsNotNull(cornerTile);
                Assert.IsNull(originTile.transform.Find("SpawnMarkerVisual"));
                Assert.IsNull(cornerTile.transform.Find("GoalMarkerVisual"));
                Assert.IsTrue(string.IsNullOrWhiteSpace(originTile.SemanticLabel));
                Assert.IsTrue(string.IsNullOrWhiteSpace(cornerTile.SemanticLabel));
            } finally {
                if (root != null) {
                    Object.DestroyImmediate(root.gameObject);
                }
            }
        }

        [Test]
        public void GenerateIntoOpenScene_FromReloadedWorkspace_RendersSemanticAndPortalLabels() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.MapWidth = 6;
            workspace.MapDepth = 5;
            workspace.AddSpawnMarker(new Vector2Int(1, 1));
            workspace.AddGoalMarker(new Vector2Int(4, 3));
            workspace.AddPortalPair(new Vector2Int(2, 1), new Vector2Int(3, 3));

            string assetPath = null;
            WhiteboxRoot root = null;
            try {
                assetPath = LevelEditorWorkspaceRepository.SaveAsNewAsset(workspace);
                var restored = LevelEditorWorkspaceRepository.LoadAsset(assetPath);
                root = WhiteboxGenerationService.GenerateIntoOpenScene(restored.Workspace);

                var spawnTile = FindTile(root, 1, 1);
                var goalTile = FindTile(root, 4, 3);
                var inTile = FindTile(root, 2, 1);
                var outTile = FindTile(root, 3, 3);

                Assert.AreEqual("R1", spawnTile.SemanticLabel);
                Assert.AreEqual("B1", goalTile.SemanticLabel);
                StringAssert.Contains("IN1", inTile.SemanticLabel);
                StringAssert.Contains("OUT1", outTile.SemanticLabel);
            } finally {
                if (root != null) {
                    Object.DestroyImmediate(root.gameObject);
                }

                if (!string.IsNullOrWhiteSpace(assetPath)) {
                    AssetDatabase.DeleteAsset(assetPath);
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
            new WhiteboxGenerationServiceTests().EnsureWhitebox_CreatesExpectedTileCountFromWorkspaceSize();
            new WhiteboxGenerationServiceTests().GenerateIntoOpenScene_AssignsSpawnAndGoalMarkersToMatchingTiles();
            new WhiteboxGenerationServiceTests().GenerateIntoOpenScene_NewWorkspaceDoesNotRenderImplicitLegacySpawnOrGoal();
            new WhiteboxGenerationServiceTests().GenerateIntoOpenScene_FromReloadedWorkspace_RendersSemanticAndPortalLabels();
            Debug.Log("[LevelEditorTests] WhiteboxGenerationServiceTests passed.");
        }
    }
}
