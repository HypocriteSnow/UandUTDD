namespace ArknightsLite.Editor.LevelEditor.Services {
    using System.Collections.Generic;
    using ArknightsLite.Config;
    using ArknightsLite.Editor.LevelEditor.Core;
    using UnityEngine;

    public static class PathAutoFillService {
        public static bool TryBuildPathToGoal(LevelEditorWorkspace workspace, out List<PathNodeDefinition> path) {
            var wave = workspace != null ? LevelEditorWorkspace.CreateDefaultWave("wave_preview") : null;
            return TryBuildPathForWave(workspace, wave, out path);
        }

        public static bool TryBuildPathForWave(LevelEditorWorkspace workspace, WaveDefinition wave, out List<PathNodeDefinition> path) {
            path = new List<PathNodeDefinition>();
            if (workspace == null) {
                return false;
            }

            if (!workspace.TryResolveSpawn(wave != null ? wave.spawnId : string.Empty, out Vector2Int start)) {
                return false;
            }

            if (!workspace.TryResolveGoal(wave != null ? wave.targetId : string.Empty, out Vector2Int goal)) {
                return false;
            }

            if (!workspace.IsTileWalkable(start.x, start.y) || !workspace.IsTileWalkable(goal.x, goal.y)) {
                return false;
            }

            var frontier = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();

            frontier.Enqueue(start);
            visited.Add(start);

            while (frontier.Count > 0) {
                Vector2Int current = frontier.Dequeue();
                if (current == goal) {
                    break;
                }

                foreach (Vector2Int next in GetReachableNeighbors(workspace, current)) {
                    if (visited.Contains(next)) {
                        continue;
                    }

                    visited.Add(next);
                    cameFrom[next] = current;
                    frontier.Enqueue(next);
                }
            }

            if (!visited.Contains(goal)) {
                return false;
            }

            var cursor = goal;
            var reversed = new List<Vector2Int> { cursor };
            while (cursor != start) {
                cursor = cameFrom[cursor];
                reversed.Add(cursor);
            }

            reversed.Reverse();
            foreach (Vector2Int point in reversed) {
                path.Add(new PathNodeDefinition {
                    x = point.x,
                    y = point.y,
                    wait = 0f
                });
            }

            return path.Count > 0;
        }

        private static IEnumerable<Vector2Int> GetReachableNeighbors(LevelEditorWorkspace workspace, Vector2Int current) {
            var directions = new[] {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down
            };

            foreach (Vector2Int direction in directions) {
                Vector2Int next = current + direction;
                if (workspace.IsTileWalkable(next.x, next.y)) {
                    yield return next;
                }
            }

            if (workspace.Portals == null) {
                yield break;
            }

            foreach (var portal in workspace.Portals) {
                if (portal == null || portal.inPos != current) {
                    continue;
                }

                if (workspace.IsTileWalkable(portal.outPos.x, portal.outPos.y)) {
                    yield return portal.outPos;
                }
            }
        }
    }
}
