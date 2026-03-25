namespace ArknightsLite.Editor.LevelEditor.Map {
    using System.Collections.Generic;
    using ArknightsLite.Editor.LevelEditor.Core;
    using ArknightsLite.Model;
    using UnityEngine;

    public sealed class WorkspaceMapController {
        private readonly LevelEditorWorkspace _workspace;

        public WorkspaceMapController(LevelEditorWorkspace workspace) {
            _workspace = workspace;
        }

        public void PaintTile(int x, int z, TileType tileType, int heightLevel) {
            if (_workspace == null) {
                return;
            }

            var refreshPositions = CollectRefreshPositionsForSemanticRemoval(x, z);
            _workspace.RemoveSemanticAt(x, z);
            var tile = _workspace.GetTileOverride(x, z);
            tile.tileType = tileType;
            tile.heightLevel = Mathf.Clamp(heightLevel, 0, 3);
            tile.walkable = tileType != TileType.Forbidden && tileType != TileType.Hole;
            tile.deployTag = string.IsNullOrWhiteSpace(tile.deployTag) ? "All" : tile.deployTag;

            _workspace.SetTileOverride(x, z, tile);
            RefreshTileVisuals(refreshPositions);
        }

        public LevelEditorWorkspace.SemanticMarker PlaceSpawnMarker(int x, int z) {
            if (_workspace == null) {
                return null;
            }

            var existing = FindMarkerAt(_workspace.SpawnMarkers, x, z);
            if (existing != null) {
                RefreshTileVisual(x, z);
                return existing;
            }

            var refreshPositions = CollectRefreshPositionsForSemanticRemoval(x, z);
            _workspace.RemoveSemanticAt(x, z);
            var marker = _workspace.AddSpawnMarker(new Vector2Int(x, z));
            RefreshTileVisuals(refreshPositions);
            return marker;
        }

        public LevelEditorWorkspace.SemanticMarker PlaceGoalMarker(int x, int z) {
            if (_workspace == null) {
                return null;
            }

            var existing = FindMarkerAt(_workspace.GoalMarkers, x, z);
            if (existing != null) {
                RefreshTileVisual(x, z);
                return existing;
            }

            var refreshPositions = CollectRefreshPositionsForSemanticRemoval(x, z);
            _workspace.RemoveSemanticAt(x, z);
            var marker = _workspace.AddGoalMarker(new Vector2Int(x, z));
            RefreshTileVisuals(refreshPositions);
            return marker;
        }

        public LevelEditorWorkspace.PortalPairDefinition PlacePortalPair(Vector2Int entrancePosition, Vector2Int exitPosition) {
            if (_workspace == null) {
                return null;
            }

            var existing = FindPortalPair(entrancePosition, exitPosition);
            if (existing != null) {
                RefreshTileVisual(existing.EntrancePosition.x, existing.EntrancePosition.y);
                RefreshTileVisual(existing.ExitPosition.x, existing.ExitPosition.y);
                return existing;
            }

            var refreshPositions = CollectRefreshPositionsForSemanticRemoval(entrancePosition.x, entrancePosition.y);
            MergeRefreshPositions(refreshPositions, CollectRefreshPositionsForSemanticRemoval(exitPosition.x, exitPosition.y));
            _workspace.RemoveSemanticAt(entrancePosition.x, entrancePosition.y);
            _workspace.RemoveSemanticAt(exitPosition.x, exitPosition.y);
            var pair = _workspace.AddPortalPair(entrancePosition, exitPosition);
            AddRefreshPosition(refreshPositions, pair.EntrancePosition.x, pair.EntrancePosition.y);
            AddRefreshPosition(refreshPositions, pair.ExitPosition.x, pair.ExitPosition.y);
            RefreshTileVisuals(refreshPositions);
            return pair;
        }

        private List<Vector2Int> CollectRefreshPositionsForSemanticRemoval(int x, int z) {
            var positions = new List<Vector2Int>();
            AddRefreshPosition(positions, x, z);

            if (_workspace?.PortalPairs == null) {
                return positions;
            }

            foreach (var pair in _workspace.PortalPairs) {
                if (pair == null) {
                    continue;
                }

                bool matchesEntrance = pair.EntrancePosition.x == x && pair.EntrancePosition.y == z;
                bool matchesExit = pair.ExitPosition.x == x && pair.ExitPosition.y == z;
                if (!matchesEntrance && !matchesExit) {
                    continue;
                }

                AddRefreshPosition(positions, pair.EntrancePosition.x, pair.EntrancePosition.y);
                AddRefreshPosition(positions, pair.ExitPosition.x, pair.ExitPosition.y);
            }

            return positions;
        }

        private static void MergeRefreshPositions(List<Vector2Int> target, List<Vector2Int> source) {
            if (target == null || source == null) {
                return;
            }

            foreach (var position in source) {
                AddRefreshPosition(target, position.x, position.y);
            }
        }

        private static void AddRefreshPosition(List<Vector2Int> positions, int x, int z) {
            if (positions == null) {
                return;
            }

            var candidate = new Vector2Int(x, z);
            if (!positions.Contains(candidate)) {
                positions.Add(candidate);
            }
        }

        private void RefreshTileVisuals(List<Vector2Int> positions) {
            if (positions == null || positions.Count == 0) {
                return;
            }

            foreach (var position in positions) {
                RefreshTileVisual(position.x, position.y);
            }
        }

        private static LevelEditorWorkspace.SemanticMarker FindMarkerAt(System.Collections.Generic.List<LevelEditorWorkspace.SemanticMarker> markers, int x, int z) {
            if (markers == null) {
                return null;
            }

            foreach (var marker in markers) {
                if (marker != null && marker.Position.x == x && marker.Position.y == z) {
                    return marker;
                }
            }

            return null;
        }

        private LevelEditorWorkspace.PortalPairDefinition FindPortalPair(Vector2Int entrancePosition, Vector2Int exitPosition) {
            if (_workspace.PortalPairs == null) {
                return null;
            }

            foreach (var pair in _workspace.PortalPairs) {
                if (pair == null) {
                    continue;
                }

                if (pair.EntrancePosition == entrancePosition && pair.ExitPosition == exitPosition) {
                    return pair;
                }
            }

            return null;
        }

        private void RefreshTileVisual(int x, int z) {
            try {
                foreach (var authoring in Object.FindObjectsOfType<TileAuthoring>()) {
                    if (authoring.X != x || authoring.Z != z) {
                        continue;
                    }

                    authoring.ApplyTileData(_workspace.GetTileOverride(x, z), _workspace.CellSize);
                    authoring.ApplySemanticMarkers(_workspace.GetSemanticLabelsAt(x, z));
                    break;
                }
            } catch (System.MissingMethodException) {
                // Offline test runners do not host the Unity object runtime.
            }
        }
    }
}
