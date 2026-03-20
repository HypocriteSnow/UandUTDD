namespace ArknightsLite.Editor.LevelEditor.Services {
    using System.Collections.Generic;
    using System.IO;
    using ArknightsLite.Config;
    using ArknightsLite.Editor.LevelEditor.Core;
    using UnityEditor;
    using UnityEngine;

    public static class LevelConfigExportService {
        private const string ExportDirectory = "Assets/Resources/Levels/Configs";

        public static string BuildAssetName(LevelEditorWorkspace workspace) {
            string levelName = workspace != null && !string.IsNullOrWhiteSpace(workspace.LevelName)
                ? workspace.LevelName
                : "NewLevel";
            return $"{levelName}_LevelConfig";
        }

        public static LevelConfig BuildTransientConfig(LevelEditorWorkspace workspace) {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.name = BuildAssetName(workspace);
            ApplyWorkspace(config, workspace);
            return config;
        }

        public static LevelConfig Export(LevelEditorWorkspace workspace) {
            string assetPath = BuildAssetPath(workspace);
            EnsureExportDirectory();

            var config = AssetDatabase.LoadAssetAtPath<LevelConfig>(assetPath);
            if (config == null) {
                config = ScriptableObject.CreateInstance<LevelConfig>();
                config.name = BuildAssetName(workspace);
                ApplyWorkspace(config, workspace);
                AssetDatabase.CreateAsset(config, assetPath);
            } else {
                ApplyWorkspace(config, workspace);
                EditorUtility.SetDirty(config);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return config;
        }

        private static string BuildAssetPath(LevelEditorWorkspace workspace) {
            return $"{ExportDirectory}/{BuildAssetName(workspace)}.asset";
        }

        private static void EnsureExportDirectory() {
            if (!Directory.Exists(ExportDirectory)) {
                Directory.CreateDirectory(ExportDirectory);
                AssetDatabase.Refresh();
            }
        }

        private static void ApplyWorkspace(LevelConfig config, LevelEditorWorkspace workspace) {
            if (config == null || workspace == null) {
                return;
            }

            config.mapWidth = workspace.MapWidth;
            config.mapDepth = workspace.MapDepth;
            config.cellSize = workspace.CellSize;
            config.defaultTileType = workspace.DefaultTileType;
            config.initialDp = workspace.Runtime.InitialDp;
            config.baseHealth = workspace.Runtime.BaseHealth;
            config.dpRecoveryInterval = workspace.Runtime.DpRecoveryInterval;
            config.dpRecoveryAmount = workspace.Runtime.DpRecoveryAmount;
            config.spawnPoints = new List<Vector2Int> { workspace.GetResolvedSpawnPoint() };
            config.goalPoint = workspace.GetResolvedGoalPoint();
            config.specialTiles = CloneTiles(workspace);
            config.portals = ClonePortals(workspace.Portals);
            config.waves = CloneWaves(workspace.Waves);
            config.enemies = CloneEnemies(workspace.Enemies);
            config.operators = CloneOperators(workspace.Operators);
        }

        private static List<TileData> CloneTiles(LevelEditorWorkspace workspace) {
            var result = new List<TileData>();
            if (workspace == null || workspace.TileOverrides == null) {
                return result;
            }

            foreach (var tile in workspace.TileOverrides) {
                if (tile == null) {
                    continue;
                }

                if (workspace.IsSpawnPoint(tile.x, tile.z) || workspace.IsGoalPoint(tile.x, tile.z)) {
                    continue;
                }

                result.Add(new TileData {
                    x = tile.x,
                    z = tile.z,
                    tileType = tile.tileType,
                    heightLevel = tile.heightLevel,
                    walkable = tile.walkable,
                    deployTag = tile.deployTag
                });
            }

            return result;
        }

        private static List<PortalDefinition> ClonePortals(List<PortalDefinition> portals) {
            var result = new List<PortalDefinition>();
            if (portals == null) {
                return result;
            }

            foreach (var portal in portals) {
                if (portal == null) {
                    continue;
                }

                result.Add(new PortalDefinition {
                    id = portal.id,
                    inPos = portal.inPos,
                    outPos = portal.outPos,
                    delay = portal.delay,
                    color = portal.color
                });
            }

            return result;
        }

        private static List<WaveDefinition> CloneWaves(List<WaveDefinition> waves) {
            var result = new List<WaveDefinition>();
            if (waves == null) {
                return result;
            }

            foreach (var wave in waves) {
                if (wave == null) {
                    continue;
                }

                var clonedWave = new WaveDefinition {
                    waveId = wave.waveId,
                    time = wave.time,
                    enemyId = wave.enemyId,
                    count = wave.count,
                    interval = wave.interval,
                    spawnId = wave.spawnId,
                    targetId = wave.targetId,
                    path = new List<PathNodeDefinition>()
                };

                if (wave.path != null) {
                    foreach (var node in wave.path) {
                        if (node == null) {
                            continue;
                        }

                        clonedWave.path.Add(new PathNodeDefinition {
                            x = node.x,
                            y = node.y,
                            wait = node.wait
                        });
                    }
                }

                result.Add(clonedWave);
            }

            return result;
        }

        private static List<EnemyTemplateData> CloneEnemies(List<EnemyTemplateData> enemies) {
            var result = new List<EnemyTemplateData>();
            if (enemies == null) {
                return result;
            }

            foreach (var enemy in enemies) {
                if (enemy == null) {
                    continue;
                }

                result.Add(new EnemyTemplateData {
                    id = enemy.id,
                    name = enemy.name,
                    movementType = enemy.movementType,
                    blockCost = enemy.blockCost,
                    specialMechanic = enemy.specialMechanic,
                    hp = enemy.hp,
                    atk = enemy.atk,
                    def = enemy.def,
                    attackSpeed = enemy.attackSpeed,
                    speed = enemy.speed,
                    color = enemy.color
                });
            }

            return result;
        }

        private static List<OperatorTemplateData> CloneOperators(List<OperatorTemplateData> operators) {
            var result = new List<OperatorTemplateData>();
            if (operators == null) {
                return result;
            }

            foreach (var op in operators) {
                if (op == null) {
                    continue;
                }

                result.Add(new OperatorTemplateData {
                    id = op.id,
                    name = op.name,
                    className = op.className,
                    combatType = op.combatType,
                    type = op.type,
                    cost = op.cost,
                    block = op.block,
                    hp = op.hp,
                    atk = op.atk,
                    def = op.def,
                    attackSpeed = op.attackSpeed,
                    range = op.range,
                    targetCount = op.targetCount,
                    color = op.color
                });
            }

            return result;
        }
    }
}
