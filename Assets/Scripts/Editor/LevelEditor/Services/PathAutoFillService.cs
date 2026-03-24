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

            var openSet = new List<Vector2Int> { start };
            var closedSet = new HashSet<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
            var fScore = new Dictionary<Vector2Int, int> { [start] = Heuristic(start, goal) };

            while (openSet.Count > 0) {
                Vector2Int current = ExtractLowestScoreNode(openSet, fScore);
                if (current == goal) {
                    BuildPathFromRoute(start, goal, cameFrom, path);
                    return path.Count > 0;
                }

                closedSet.Add(current);

                foreach (Vector2Int next in GetReachableNeighbors(workspace, current)) {
                    if (closedSet.Contains(next)) {
                        continue;
                    }

                    int tentativeScore = GetScore(gScore, current) + 1;
                    if (tentativeScore >= GetScore(gScore, next)) {
                        continue;
                    }

                    cameFrom[next] = current;
                    gScore[next] = tentativeScore;
                    fScore[next] = tentativeScore + Heuristic(next, goal);

                    if (!openSet.Contains(next)) {
                        openSet.Add(next);
                    }
                }
            }

            return false;
        }

        private static void BuildPathFromRoute(
            Vector2Int start,
            Vector2Int goal,
            Dictionary<Vector2Int, Vector2Int> cameFrom,
            List<PathNodeDefinition> path) {
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
        }

        private static Vector2Int ExtractLowestScoreNode(List<Vector2Int> openSet, Dictionary<Vector2Int, int> fScore) {
            int bestIndex = 0;
            int bestScore = GetScore(fScore, openSet[0]);

            for (int i = 1; i < openSet.Count; i++) {
                int currentScore = GetScore(fScore, openSet[i]);
                if (currentScore < bestScore) {
                    bestIndex = i;
                    bestScore = currentScore;
                }
            }

            Vector2Int current = openSet[bestIndex];
            openSet.RemoveAt(bestIndex);
            return current;
        }

        private static int GetScore(Dictionary<Vector2Int, int> scoreMap, Vector2Int position) {
            return scoreMap.TryGetValue(position, out int score) ? score : int.MaxValue;
        }

        private static int Heuristic(Vector2Int current, Vector2Int goal) {
            return Mathf.Abs(goal.x - current.x) + Mathf.Abs(goal.y - current.y);
        }

        private static IEnumerable<Vector2Int> GetReachableNeighbors(LevelEditorWorkspace workspace, Vector2Int current) {
            workspace.EnsureDefaults();
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

            var portals = new List<PortalDefinition>(workspace.Portals);
            foreach (var portal in portals) {
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
