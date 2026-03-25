namespace ArknightsLite.Editor.LevelEditor.Services {
    using System;
    using System.Collections.Generic;
    using ArknightsLite.Config;
    using ArknightsLite.Editor.LevelEditor.Core;
    using ArknightsLite.Model;
    using UnityEngine;

    public static class LevelValidationService {
        public static LevelValidationResult Validate(LevelConfig config) {
            var result = new LevelValidationResult();
            if (config == null) {
                result.Errors.Add("LevelConfig is null.");
                return result;
            }

            if (config.mapWidth <= 0 || config.mapDepth <= 0) {
                result.Errors.Add("Map size must be positive.");
            }

            if (config.spawnPoints == null || config.spawnPoints.Count == 0) {
                result.Errors.Add("At least one spawn point is required.");
            }

            if (config.goalPoint.x < 0 || config.goalPoint.y < 0) {
                result.Errors.Add("Goal point is invalid.");
            }

            ValidateExplicitEndpointTiles(result, config);

            var enemyIds = CollectEnemyIds(config.enemies);
            if (config.waves != null) {
                foreach (var wave in config.waves) {
                    ValidateConfigWave(result, wave, enemyIds);
                }
            }

            return result;
        }

        public static LevelValidationResult ValidateWorkspace(LevelEditorWorkspace workspace) {
            var result = new LevelValidationResult();
            if (workspace == null) {
                result.Errors.Add("Workspace is null.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(workspace.LevelName) && string.IsNullOrWhiteSpace(workspace.ExportName)) {
                result.Errors.Add("Export name or workspace name is required.");
            }

            workspace.EnsureDefaults();

            bool hasSpawnMarkers = workspace.SpawnMarkers != null && workspace.SpawnMarkers.Count > 0;
            bool hasGoalMarkers = workspace.GoalMarkers != null && workspace.GoalMarkers.Count > 0;
            if (!hasSpawnMarkers) {
                result.Errors.Add("At least one spawn marker is required.");
            }

            if (!hasGoalMarkers) {
                result.Errors.Add("At least one goal marker is required.");
            }

            ValidatePortalPairs(result, workspace.PortalPairs);

            var enemyIds = CollectEnemyIds(workspace.Enemies);
            if (workspace.Waves != null) {
                foreach (var wave in workspace.Waves) {
                    ValidateWorkspaceWave(result, workspace, wave, enemyIds);
                }
            }

            return result;
        }

        public static List<string> CollectExplicitEndpointTileErrors(
            List<Vector2Int> spawnPoints,
            Vector2Int goalPoint,
            List<PortalDefinition> portals,
            List<TileData> specialTiles) {
            var errors = new List<string>();

            if (spawnPoints != null) {
                foreach (var spawnPoint in spawnPoints) {
                    if (!HasExplicitWalkableTile(specialTiles, spawnPoint.x, spawnPoint.y)) {
                        errors.Add($"Spawn point ({spawnPoint.x}, {spawnPoint.y}) must have explicit walkable tile data instead of implicit fallback.");
                    }
                }
            }

            if (goalPoint.x >= 0 && goalPoint.y >= 0
                && !HasExplicitWalkableTile(specialTiles, goalPoint.x, goalPoint.y)) {
                errors.Add($"Goal point ({goalPoint.x}, {goalPoint.y}) must have explicit walkable tile data instead of implicit fallback.");
            }

            if (portals == null) {
                return errors;
            }

            foreach (var portal in portals) {
                if (portal == null) {
                    continue;
                }

                if (!HasExplicitWalkableTile(specialTiles, portal.inPos.x, portal.inPos.y)) {
                    errors.Add($"Portal entrance ({portal.inPos.x}, {portal.inPos.y}) must have explicit walkable tile data instead of implicit fallback.");
                }

                if (!HasExplicitWalkableTile(specialTiles, portal.outPos.x, portal.outPos.y)) {
                    errors.Add($"Portal exit ({portal.outPos.x}, {portal.outPos.y}) must have explicit walkable tile data instead of implicit fallback.");
                }
            }

            return errors;
        }

        private static void ValidateConfigWave(LevelValidationResult result, WaveDefinition wave, HashSet<string> enemyIds) {
            string waveId = string.IsNullOrWhiteSpace(wave?.waveId) ? "<unnamed>" : wave.waveId;
            if (wave == null) {
                result.Errors.Add($"Wave {waveId} is null.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(wave.enemyId) && !enemyIds.Contains(wave.enemyId)) {
                result.Errors.Add($"Wave {waveId} references missing enemy '{wave.enemyId}'.");
            }

            if (string.IsNullOrWhiteSpace(wave.spawnId)) {
                result.Errors.Add($"Wave {waveId} is missing a spawn reference.");
            }

            if (string.IsNullOrWhiteSpace(wave.targetId)) {
                result.Errors.Add($"Wave {waveId} is missing a target reference.");
            }

            if (wave.path == null || wave.path.Count == 0) {
                result.Errors.Add($"Wave {waveId} is missing path data.");
            }
        }

        private static void ValidateExplicitEndpointTiles(LevelValidationResult result, LevelConfig config) {
            if (config == null) {
                return;
            }

            result.Errors.AddRange(CollectExplicitEndpointTileErrors(
                config.spawnPoints,
                config.goalPoint,
                config.portals,
                config.specialTiles));
        }

        private static bool HasExplicitWalkableTile(List<TileData> specialTiles, int x, int z) {
            if (specialTiles == null) {
                return false;
            }

            foreach (var tile in specialTiles) {
                if (tile == null || tile.x != x || tile.z != z) {
                    continue;
                }

                return tile.walkable && tile.tileType != TileType.Forbidden && tile.tileType != TileType.Hole;
            }

            return false;
        }

        private static void ValidateWorkspaceWave(
            LevelValidationResult result,
            LevelEditorWorkspace workspace,
            WaveDefinition wave,
            HashSet<string> enemyIds) {
            string waveId = string.IsNullOrWhiteSpace(wave?.waveId) ? "<unnamed>" : wave.waveId;
            if (wave == null) {
                result.Errors.Add($"Wave {waveId} is null.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(wave.enemyId) && !enemyIds.Contains(wave.enemyId)) {
                result.Errors.Add($"Wave {waveId} references missing enemy '{wave.enemyId}'.");
            }

            bool hasSpawn = workspace.TryResolveSpawn(wave.spawnId, out Vector2Int spawnPosition);
            if (!hasSpawn) {
                result.Errors.Add($"Wave {waveId} references missing spawn '{wave.spawnId}'.");
            }

            bool hasGoal = workspace.TryResolveGoal(wave.targetId, out Vector2Int goalPosition);
            if (!hasGoal) {
                result.Errors.Add($"Wave {waveId} references missing goal '{wave.targetId}'.");
            }

            if (wave.path == null || wave.path.Count == 0) {
                result.Errors.Add($"Wave {waveId} is missing path data.");
                return;
            }

            if (hasSpawn) {
                var firstNode = wave.path[0];
                if (firstNode == null || firstNode.x != spawnPosition.x || firstNode.y != spawnPosition.y) {
                    result.Errors.Add($"Wave {waveId} path must start at spawn '{wave.spawnId}'.");
                }
            }

            if (hasGoal) {
                var lastNode = wave.path[wave.path.Count - 1];
                if (lastNode == null || lastNode.x != goalPosition.x || lastNode.y != goalPosition.y) {
                    result.Errors.Add($"Wave {waveId} path must end at goal '{wave.targetId}'.");
                }
            }
        }

        private static void ValidatePortalPairs(LevelValidationResult result, List<LevelEditorWorkspace.PortalPairDefinition> portalPairs) {
            if (portalPairs == null) {
                return;
            }

            var seenEntrances = new HashSet<Vector2Int>();
            var seenExits = new HashSet<Vector2Int>();
            foreach (var pair in portalPairs) {
                if (pair == null) {
                    result.Errors.Add("Portal pair entry is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(pair.EntranceId) || string.IsNullOrWhiteSpace(pair.ExitId)) {
                    result.Errors.Add("Portal pair is missing readable endpoint IDs.");
                }

                if (pair.EntrancePosition == pair.ExitPosition) {
                    result.Errors.Add($"Portal pair {pair.PairId} must use different entrance and exit tiles.");
                }

                if (!seenEntrances.Add(pair.EntrancePosition)) {
                    result.Errors.Add($"Portal entrance {pair.EntranceId} overlaps another entrance.");
                }

                if (!seenExits.Add(pair.ExitPosition)) {
                    result.Errors.Add($"Portal exit {pair.ExitId} overlaps another exit.");
                }
            }
        }

        private static HashSet<string> CollectEnemyIds(List<EnemyTemplateData> enemies) {
            var enemyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (enemies == null) {
                return enemyIds;
            }

            foreach (var enemy in enemies) {
                if (enemy == null || string.IsNullOrEmpty(enemy.id)) {
                    continue;
                }

                enemyIds.Add(enemy.id);
            }

            return enemyIds;
        }
    }
}
