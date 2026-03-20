using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;
using ArknightsLite.Config;
using ArknightsLite.Model;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelConfigExportServiceTests {
        [Test]
        public void BuildAssetName_UsesLevelNameSuffixLevelConfig() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            string assetName = LevelConfigExportService.BuildAssetName(workspace);

            Assert.AreEqual("Tutorial_01_LevelConfig", assetName);
        }

        [Test]
        public void BuildTransientConfig_CopiesWorkspaceDataIntoLevelConfig() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.MapWidth = 8;
            workspace.MapDepth = 6;
            workspace.CellSize = 1.25f;
            workspace.DefaultTileType = TileType.Ground;
            workspace.Runtime.InitialDp = 25;
            workspace.Runtime.BaseHealth = 4;
            workspace.Runtime.DpRecoveryInterval = 0.75f;
            workspace.Runtime.DpRecoveryAmount = 2;
            workspace.SetTileOverride(2, 1, new TileData {
                tileType = TileType.HighGround,
                heightLevel = 1,
                walkable = true,
                deployTag = "All"
            });

            LevelConfig config = LevelConfigExportService.BuildTransientConfig(workspace);

            Assert.AreEqual(8, config.mapWidth);
            Assert.AreEqual(6, config.mapDepth);
            Assert.AreEqual(1.25f, config.cellSize);
            Assert.AreEqual(1, config.specialTiles.Count);
            Assert.AreEqual(25, config.initialDp);
            Assert.AreEqual(4, config.baseHealth);
            Assert.AreEqual(0.75f, config.dpRecoveryInterval);
            Assert.AreEqual(2, config.dpRecoveryAmount);
        }

        [Test]
        public void BuildTransientConfig_UsesWorkspaceSpawnAndGoalMarkers() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.SetSpawnPoint(new Vector2Int(2, 1));
            workspace.SetGoalPoint(new Vector2Int(6, 4));

            LevelConfig config = LevelConfigExportService.BuildTransientConfig(workspace);

            Assert.AreEqual(1, config.spawnPoints.Count);
            Assert.AreEqual(new Vector2Int(2, 1), config.spawnPoints[0]);
            Assert.AreEqual(new Vector2Int(6, 4), config.goalPoint);
        }

        [Test]
        public void Export_CreatesNamedAssetUnderLevelsConfigDirectory() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_Export_Test");
            const string expectedPath = "Assets/Resources/Levels/Configs/Tutorial_Export_Test_LevelConfig.asset";

            try {
                LevelConfig exported = LevelConfigExportService.Export(workspace);

                Assert.IsNotNull(exported);
                Assert.AreSame(exported, AssetDatabase.LoadAssetAtPath<LevelConfig>(expectedPath));
            } finally {
                AssetDatabase.DeleteAsset(expectedPath);
            }
        }

        public static void RunFromCommandLine() {
            new LevelConfigExportServiceTests().BuildAssetName_UsesLevelNameSuffixLevelConfig();
            new LevelConfigExportServiceTests().BuildTransientConfig_CopiesWorkspaceDataIntoLevelConfig();
            new LevelConfigExportServiceTests().BuildTransientConfig_UsesWorkspaceSpawnAndGoalMarkers();
            new LevelConfigExportServiceTests().Export_CreatesNamedAssetUnderLevelsConfigDirectory();
            Debug.Log("[LevelEditorTests] LevelConfigExportServiceTests passed.");
        }
    }
}
