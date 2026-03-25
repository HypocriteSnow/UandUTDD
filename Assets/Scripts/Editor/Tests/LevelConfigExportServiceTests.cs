using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;
using ArknightsLite.Config;
using ArknightsLite.Model;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
            workspace.AddSpawnMarker(new Vector2Int(2, 1));
            workspace.AddGoalMarker(new Vector2Int(6, 4));

            LevelConfig config = LevelConfigExportService.BuildTransientConfig(workspace);

            Assert.AreEqual(1, config.spawnPoints.Count);
            Assert.AreEqual(new Vector2Int(2, 1), config.spawnPoints[0]);
            Assert.AreEqual(new Vector2Int(6, 4), config.goalPoint);
        }

        [Test]
        public void BuildTransientConfig_ExportsSpawnAndGoalTilesAsExplicitGroundSpecialTiles() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.AddSpawnMarker(new Vector2Int(2, 1));
            workspace.AddGoalMarker(new Vector2Int(6, 4));

            LevelConfig config = LevelConfigExportService.BuildTransientConfig(workspace);

            TileData spawnTile = config.specialTiles.Find(tile => tile.x == 2 && tile.z == 1);
            TileData goalTile = config.specialTiles.Find(tile => tile.x == 6 && tile.z == 4);

            Assert.IsNotNull(spawnTile);
            Assert.IsNotNull(goalTile);
            Assert.AreEqual(TileType.Ground, spawnTile.tileType);
            Assert.AreEqual(TileType.Ground, goalTile.tileType);
            Assert.IsTrue(spawnTile.walkable);
            Assert.IsTrue(goalTile.walkable);
        }

        [Test]
        public void BuildSpecialTilesForExport_ExportsSemanticEndpointsAsExplicitWalkableTiles() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.AddSpawnMarker(new Vector2Int(2, 1));
            workspace.AddGoalMarker(new Vector2Int(6, 4));
            workspace.AddPortalPair(new Vector2Int(3, 2), new Vector2Int(5, 3));

            var specialTiles = LevelConfigExportService.BuildSpecialTilesForExport(workspace);

            TileData spawnTile = specialTiles.Find(tile => tile.x == 2 && tile.z == 1);
            TileData goalTile = specialTiles.Find(tile => tile.x == 6 && tile.z == 4);
            TileData portalEntranceTile = specialTiles.Find(tile => tile.x == 3 && tile.z == 2);
            TileData portalExitTile = specialTiles.Find(tile => tile.x == 5 && tile.z == 3);

            Assert.IsNotNull(spawnTile);
            Assert.IsNotNull(goalTile);
            Assert.IsNotNull(portalEntranceTile);
            Assert.IsNotNull(portalExitTile);
            Assert.AreEqual(TileType.Ground, spawnTile.tileType);
            Assert.AreEqual(TileType.Ground, goalTile.tileType);
            Assert.AreEqual(TileType.Ground, portalEntranceTile.tileType);
            Assert.AreEqual(TileType.Ground, portalExitTile.tileType);
            Assert.IsTrue(spawnTile.walkable);
            Assert.IsTrue(goalTile.walkable);
            Assert.IsTrue(portalEntranceTile.walkable);
            Assert.IsTrue(portalExitTile.walkable);
        }

        [Test]
        public void LevelConfig_GetTileData_DoesNotTreatSpawnOrGoalAsImplicitGround() {
            var config = CreateOfflineLevelConfig();
            config.spawnPoints.Add(new Vector2Int(2, 1));
            config.goalPoint = new Vector2Int(6, 4);

            TileData spawnTile = config.GetTileData(2, 1);
            TileData goalTile = config.GetTileData(6, 4);

            Assert.AreEqual(TileType.Forbidden, spawnTile.tileType);
            Assert.AreEqual(TileType.Forbidden, goalTile.tileType);
            Assert.IsFalse(spawnTile.walkable);
            Assert.IsFalse(goalTile.walkable);
        }

        [Test]
        public void LevelConfig_CollectValidationErrors_FailsWhenSpawnOrGoalLacksExplicitWalkableTileData() {
            var config = CreateOfflineLevelConfig();
            config.spawnPoints.Add(new Vector2Int(2, 1));
            config.goalPoint = new Vector2Int(6, 4);

            var errors = config.CollectValidationErrors();

            Assert.That(errors, Has.Some.Contains("Spawn point"));
            Assert.That(errors, Has.Some.Contains("Goal point"));
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

        [Test]
        public void BuildTransientConfig_FromReloadedWorkspace_PreservesSemanticWaveReferences() {
            var workspace = LevelEditorWorkspace.CreateNew("Workspace_T7_Export");
            workspace.ExportName = "Episode_T7_Export";
            var spawn = workspace.AddSpawnMarker(new Vector2Int(1, 1));
            var goal = workspace.AddGoalMarker(new Vector2Int(5, 4));
            workspace.AddPortalPair(new Vector2Int(2, 1), new Vector2Int(4, 3));

            var wave = LevelEditorWorkspace.CreateDefaultWave("wave_01");
            wave.spawnId = spawn.Id;
            wave.targetId = goal.Id;
            wave.path.Add(new PathNodeDefinition { x = 1, y = 1, wait = 0f });
            wave.path.Add(new PathNodeDefinition { x = 2, y = 1, wait = 0f });
            wave.path.Add(new PathNodeDefinition { x = 4, y = 3, wait = 0f });
            wave.path.Add(new PathNodeDefinition { x = 5, y = 4, wait = 0f });
            workspace.Waves.Add(wave);

            string assetPath = null;
            try {
                assetPath = LevelEditorWorkspaceRepository.SaveAsNewAsset(workspace);
                var restored = LevelEditorWorkspaceRepository.LoadAsset(assetPath);
                LevelConfig config = LevelConfigExportService.BuildTransientConfig(restored.Workspace);

                Assert.AreEqual("Episode_T7_Export_LevelConfig", config.name);
                Assert.AreEqual(1, config.waves.Count);
                Assert.AreEqual(spawn.Id, config.waves[0].spawnId);
                Assert.AreEqual(goal.Id, config.waves[0].targetId);
                Assert.AreEqual(4, config.waves[0].path.Count);
                Assert.AreEqual(1, config.portals.Count);

                Object.DestroyImmediate(config);
            } finally {
                if (!string.IsNullOrWhiteSpace(assetPath)) {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
        }

        public static void RunFromCommandLine() {
            new LevelConfigExportServiceTests().BuildAssetName_UsesLevelNameSuffixLevelConfig();
            new LevelConfigExportServiceTests().BuildTransientConfig_CopiesWorkspaceDataIntoLevelConfig();
            new LevelConfigExportServiceTests().BuildTransientConfig_UsesWorkspaceSpawnAndGoalMarkers();
            new LevelConfigExportServiceTests().BuildTransientConfig_ExportsSpawnAndGoalTilesAsExplicitGroundSpecialTiles();
            new LevelConfigExportServiceTests().BuildSpecialTilesForExport_ExportsSemanticEndpointsAsExplicitWalkableTiles();
            new LevelConfigExportServiceTests().LevelConfig_GetTileData_DoesNotTreatSpawnOrGoalAsImplicitGround();
            new LevelConfigExportServiceTests().LevelConfig_CollectValidationErrors_FailsWhenSpawnOrGoalLacksExplicitWalkableTileData();
            new LevelConfigExportServiceTests().Export_CreatesNamedAssetUnderLevelsConfigDirectory();
            new LevelConfigExportServiceTests().BuildTransientConfig_FromReloadedWorkspace_PreservesSemanticWaveReferences();
            Debug.Log("[LevelEditorTests] LevelConfigExportServiceTests passed.");
        }

        private static LevelConfig CreateOfflineLevelConfig() {
            var config = (LevelConfig)FormatterServices.GetUninitializedObject(typeof(LevelConfig));
            config.mapWidth = 8;
            config.mapDepth = 6;
            config.cellSize = 1f;
            config.defaultTileType = TileType.Forbidden;
            config.goalPoint = new Vector2Int(-1, -1);
            config.spawnPoints = new List<Vector2Int>();
            config.specialTiles = new List<TileData>();
            config.portals = new List<PortalDefinition>();
            config.waves = new List<WaveDefinition>();
            config.enemies = new List<EnemyTemplateData>();
            config.operators = new List<OperatorTemplateData>();
            return config;
        }
    }
}
