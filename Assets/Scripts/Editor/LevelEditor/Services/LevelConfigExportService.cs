namespace ArknightsLite.Editor.LevelEditor.Services {
    using System.Collections.Generic;
    using System.IO;
    using ArknightsLite.Config;
    using ArknightsLite.Editor.LevelEditor.Core;
    using ArknightsLite.Model;
    using UnityEditor;
    using UnityEngine;

    public static class LevelConfigExportService {
        private const string ExportDirectory = "Assets/Resources/Levels/Configs";

        public static string BuildAssetName(LevelEditorWorkspace workspace) {
            return $"{ResolveExportName(workspace)}_LevelConfig";
        }

        public static string ResolveExportName(LevelEditorWorkspace workspace) {
            if (workspace != null) {
                if (!string.IsNullOrWhiteSpace(workspace.ExportName)) {
                    return workspace.ExportName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(workspace.LevelName)) {
                    return workspace.LevelName.Trim();
                }
            }

            return LevelEditorWorkspace.DefaultLevelName;
        }

        public static bool AssetExists(LevelEditorWorkspace workspace) {
            return AssetDatabase.LoadAssetAtPath<LevelConfig>(BuildAssetPath(workspace)) != null;
        }

        public static LevelConfig BuildTransientConfig(LevelEditorWorkspace workspace) {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.name = BuildAssetName(workspace);
            ApplyWorkspace(config, workspace);
            return config;
        }

        public static List<TileData> BuildSpecialTilesForExport(LevelEditorWorkspace workspace) {
            var specialTiles = CloneTiles(workspace);
            EnsureSemanticEndpointTiles(specialTiles, workspace);
            return specialTiles;
        }

        public static LevelConfig Export(LevelEditorWorkspace workspace) {
            string assetPath = BuildAssetPath(workspace);
            EnsureExportDirectory();

            var config = AssetDatabase.LoadAssetAtPath<LevelConfig>(assetPath);
            if (config == null) {
                config = ScriptableObject.CreateInstance<LevelConfig>();
                AssetDatabase.CreateAsset(config, assetPath);
            }

            config.name = BuildAssetName(workspace);
            ApplyWorkspace(config, workspace);
            EditorUtility.SetDirty(config);

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

            workspace.EnsureDefaults();

            config.mapWidth = workspace.MapWidth;
            config.mapDepth = workspace.MapDepth;
            config.cellSize = workspace.CellSize;
            config.defaultTileType = workspace.DefaultTileType;
            config.initialDp = workspace.Runtime.InitialDp;
            config.baseHealth = workspace.Runtime.BaseHealth;
            config.dpRecoveryInterval = workspace.Runtime.DpRecoveryInterval;
            config.dpRecoveryAmount = workspace.Runtime.DpRecoveryAmount;
            config.spawnPoints = ResolveExportSpawnPoints(workspace);
            config.goalPoint = ResolveExportGoalPoint(workspace);
            config.specialTiles = BuildSpecialTilesForExport(workspace);
            config.portals = ClonePortals(workspace.Portals);
            config.waves = CloneWaves(workspace.Waves);
            config.enemies = CloneEnemies(workspace.Enemies);
            config.operators = CloneOperators(workspace.Operators);
        }

        private static List<Vector2Int> ResolveExportSpawnPoints(LevelEditorWorkspace workspace) {
            var result = new List<Vector2Int>();
            if (workspace == null) {
                return result;
            }

            if (workspace.SpawnMarkers != null && workspace.SpawnMarkers.Count > 0) {
                foreach (var marker in workspace.SpawnMarkers) {
                    if (marker == null || result.Contains(marker.Position)) {
                        continue;
                    }

                    result.Add(marker.Position);
                }
            }

            return result;
        }

        private static Vector2Int ResolveExportGoalPoint(LevelEditorWorkspace workspace) {
            if (workspace != null && workspace.GoalMarkers != null && workspace.GoalMarkers.Count > 0) {
                foreach (var marker in workspace.GoalMarkers) {
                    if (marker != null) {
                        return marker.Position;
                    }
                }
            }

            return new Vector2Int(-1, -1);
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

        private static void EnsureSemanticEndpointTiles(List<TileData> specialTiles, LevelEditorWorkspace workspace) {
            if (specialTiles == null || workspace == null) {
                return;
            }

            if (workspace.SpawnMarkers != null) {
                foreach (var marker in workspace.SpawnMarkers) {
                    if (marker != null) {
                        EnsureExplicitWalkableTile(specialTiles, workspace, marker.Position);
                    }
                }
            }

            if (workspace.GoalMarkers != null) {
                foreach (var marker in workspace.GoalMarkers) {
                    if (marker != null) {
                        EnsureExplicitWalkableTile(specialTiles, workspace, marker.Position);
                    }
                }
            }

            if (workspace.PortalPairs != null) {
                foreach (var portalPair in workspace.PortalPairs) {
                    if (portalPair == null) {
                        continue;
                    }

                    EnsureExplicitWalkableTile(specialTiles, workspace, portalPair.EntrancePosition);
                    EnsureExplicitWalkableTile(specialTiles, workspace, portalPair.ExitPosition);
                }
            }
        }

        private static void EnsureExplicitWalkableTile(List<TileData> specialTiles, LevelEditorWorkspace workspace, Vector2Int position) {
            if (specialTiles == null || workspace == null) {
                return;
            }

            TileData explicitTile = BuildExplicitSemanticTile(workspace.GetTileOverride(position.x, position.y), position.x, position.y);
            TileData existing = specialTiles.Find(tile => tile != null && tile.x == position.x && tile.z == position.y);

            if (existing != null) {
                existing.tileType = explicitTile.tileType;
                existing.heightLevel = explicitTile.heightLevel;
                existing.walkable = explicitTile.walkable;
                existing.deployTag = explicitTile.deployTag;
                return;
            }

            specialTiles.Add(explicitTile);
        }

        private static TileData BuildExplicitSemanticTile(TileData source, int x, int z) {
            TileType resolvedType = source != null ? source.tileType : TileType.Ground;
            if (resolvedType == TileType.Forbidden || resolvedType == TileType.Hole) {
                resolvedType = TileType.Ground;
            }

            return new TileData {
                x = x,
                z = z,
                tileType = resolvedType,
                heightLevel = source != null && resolvedType == TileType.HighGround ? source.heightLevel : 0,
                walkable = true,
                deployTag = source != null && !string.IsNullOrWhiteSpace(source.deployTag) ? source.deployTag : "All"
            };
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
