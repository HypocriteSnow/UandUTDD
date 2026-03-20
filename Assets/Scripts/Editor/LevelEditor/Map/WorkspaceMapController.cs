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

        private void RefreshTileVisual(int x, int z) {
            foreach (var authoring in Object.FindObjectsOfType<TileAuthoring>()) {
                if (authoring.X != x || authoring.Z != z) {
                    continue;
                }

                authoring.ApplyTileData(_workspace.GetTileOverride(x, z), _workspace.CellSize);
                authoring.ApplySemanticMarkers(
                    _workspace.IsSpawnPoint(x, z),
                    _workspace.SpawnId,
                    _workspace.IsGoalPoint(x, z),
                    _workspace.GoalId
                );
                break;
            }
        }
    }
}
