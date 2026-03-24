namespace ArknightsLite.Editor.LevelEditor.Map {
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

            var tile = _workspace.GetTileOverride(x, z);
            tile.tileType = tileType;
            tile.heightLevel = Mathf.Clamp(heightLevel, 0, 3);
            tile.walkable = tileType != TileType.Forbidden && tileType != TileType.Hole;
            tile.deployTag = string.IsNullOrWhiteSpace(tile.deployTag) ? "All" : tile.deployTag;

            _workspace.SetTileOverride(x, z, tile);
            RefreshTileVisual(x, z);
        }

        public void SetSpawnPoint(int x, int z) {
            if (_workspace == null) {
                return;
            }

            Vector2Int previous = _workspace.GetResolvedSpawnPoint();
            _workspace.SetSpawnPoint(new Vector2Int(x, z));
            RefreshTileVisual(previous.x, previous.y);
            RefreshTileVisual(x, z);
        }

        public void SetGoalPoint(int x, int z) {
            if (_workspace == null) {
                return;
            }

            Vector2Int previous = _workspace.GetResolvedGoalPoint();
            _workspace.SetGoalPoint(new Vector2Int(x, z));
            RefreshTileVisual(previous.x, previous.y);
            RefreshTileVisual(x, z);
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

            bool refreshLegacyFallback = _workspace.SpawnMarkers == null || _workspace.SpawnMarkers.Count == 0;
            Vector2Int previous = _workspace.GetResolvedSpawnPoint();
            var marker = _workspace.AddSpawnMarker(new Vector2Int(x, z));
            if (refreshLegacyFallback) {
                RefreshTileVisual(previous.x, previous.y);
            }

            RefreshTileVisual(x, z);
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

            bool refreshLegacyFallback = _workspace.GoalMarkers == null || _workspace.GoalMarkers.Count == 0;
            Vector2Int previous = _workspace.GetResolvedGoalPoint();
            var marker = _workspace.AddGoalMarker(new Vector2Int(x, z));
            if (refreshLegacyFallback) {
                RefreshTileVisual(previous.x, previous.y);
            }

            RefreshTileVisual(x, z);
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

            var pair = _workspace.AddPortalPair(entrancePosition, exitPosition);
            RefreshTileVisual(pair.EntrancePosition.x, pair.EntrancePosition.y);
            RefreshTileVisual(pair.ExitPosition.x, pair.ExitPosition.y);
            return pair;
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
            foreach (var authoring in Object.FindObjectsOfType<TileAuthoring>()) {
                if (authoring.X != x || authoring.Z != z) {
                    continue;
                }

                authoring.ApplyTileData(_workspace.GetTileOverride(x, z), _workspace.CellSize);
                authoring.ApplySemanticMarkers(_workspace.GetSemanticLabelsAt(x, z));
                break;
            }
        }
    }
}
