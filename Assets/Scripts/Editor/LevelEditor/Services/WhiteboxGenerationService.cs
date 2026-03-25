namespace ArknightsLite.Editor.LevelEditor.Services {
    using System;
    using System.Collections.Generic;
    using ArknightsLite.Config;
    using ArknightsLite.Editor.LevelEditor.Core;
    using ArknightsLite.Model;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class WhiteboxGenerationService {
        public enum WhiteboxVisualType {
            Forbidden,
            Ground,
            HighGround,
            Hole,
            Spawn,
            Goal,
            PortalEntrance,
            PortalExit
        }

        public struct WhiteboxPreviewTile {
            public WhiteboxPreviewTile(int x, int z, TileType tileType, int heightLevel, string semanticLabel, WhiteboxVisualType visualType) {
                X = x;
                Z = z;
                TileType = tileType;
                HeightLevel = heightLevel;
                SemanticLabel = semanticLabel ?? string.Empty;
                VisualType = visualType;
            }

            public int X { get; }
            public int Z { get; }
            public TileType TileType { get; }
            public int HeightLevel { get; }
            public string SemanticLabel { get; }
            public WhiteboxVisualType VisualType { get; }
        }

        public struct WhiteboxPreviewResult {
            private readonly Dictionary<Vector2Int, WhiteboxPreviewTile> _tiles;

            public WhiteboxPreviewResult(Dictionary<Vector2Int, WhiteboxPreviewTile> tiles) {
                _tiles = tiles ?? new Dictionary<Vector2Int, WhiteboxPreviewTile>();
            }

            public int TileCount => _tiles != null ? _tiles.Count : 0;

            public bool TryGetTile(int x, int z, out WhiteboxPreviewTile tile) {
                if (_tiles != null) {
                    return _tiles.TryGetValue(new Vector2Int(x, z), out tile);
                }

                tile = default;
                return false;
            }
        }

        public static WhiteboxPreviewResult BuildPreview(LevelEditorWorkspace workspace) {
            if (workspace == null) {
                return new WhiteboxPreviewResult(null);
            }

            workspace.EnsureDefaults();
            var tiles = new Dictionary<Vector2Int, WhiteboxPreviewTile>();

            for (int x = 0; x < workspace.MapWidth; x++) {
                for (int z = 0; z < workspace.MapDepth; z++) {
                    var key = new Vector2Int(x, z);
                    tiles[key] = BuildPreviewTile(workspace, x, z);
                }
            }

            return new WhiteboxPreviewResult(tiles);
        }

        public static WhiteboxRoot GenerateIntoOpenScene(LevelEditorWorkspace workspace, GridVisualConfig visualConfig = null) {
            if (workspace == null) {
                return null;
            }

            var root = UnityEngine.Object.FindObjectOfType<WhiteboxRoot>();
            if (root == null) {
                root = new GameObject().AddComponent<WhiteboxRoot>();
            }

            root.ApplyLayout(workspace.LevelName, workspace.MapWidth, workspace.MapDepth, workspace.CellSize);
            RebuildTiles(root.transform, workspace, visualConfig);
            Selection.activeGameObject = root.gameObject;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return root;
        }

        private static void RebuildTiles(Transform rootTransform, LevelEditorWorkspace workspace, GridVisualConfig visualConfig) {
            while (rootTransform.childCount > 0) {
                UnityEngine.Object.DestroyImmediate(rootTransform.GetChild(0).gameObject);
            }

            for (int x = 0; x < workspace.MapWidth; x++) {
                for (int z = 0; z < workspace.MapDepth; z++) {
                    CreateTile(rootTransform, workspace, visualConfig, x, z);
                }
            }
        }

        private static void CreateTile(Transform rootTransform, LevelEditorWorkspace workspace, GridVisualConfig visualConfig, int x, int z) {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Tile_{x}_{z}";
            cube.transform.SetParent(rootTransform, false);
            cube.transform.localScale = new Vector3(workspace.CellSize * 0.95f, 0.2f, workspace.CellSize * 0.95f);
            cube.transform.localPosition = new Vector3(x * workspace.CellSize, 0f, z * workspace.CellSize);

            var authoring = cube.AddComponent<TileAuthoring>();
            authoring.Initialize(x, z, workspace.GetTileOverride(x, z), workspace.CellSize, visualConfig);
            authoring.ApplySemanticMarkers(workspace.GetSemanticLabelsAt(x, z));
        }

        private static WhiteboxPreviewTile BuildPreviewTile(LevelEditorWorkspace workspace, int x, int z) {
            var tile = workspace.GetTileOverride(x, z);
            var semanticLabels = workspace.GetSemanticLabelsAt(x, z);
            return new WhiteboxPreviewTile(
                x,
                z,
                tile.tileType,
                tile.heightLevel,
                semanticLabels != null && semanticLabels.Count > 0 ? string.Join(" / ", semanticLabels) : string.Empty,
                ResolveVisualType(tile.tileType, semanticLabels)
            );
        }

        private static WhiteboxVisualType ResolveVisualType(TileType tileType, List<string> semanticLabels) {
            if (ContainsSemanticLabelPrefix(semanticLabels, "R")) {
                return WhiteboxVisualType.Spawn;
            }

            if (ContainsSemanticLabelPrefix(semanticLabels, "B")) {
                return WhiteboxVisualType.Goal;
            }

            if (ContainsSemanticLabelPrefix(semanticLabels, "IN")) {
                return WhiteboxVisualType.PortalEntrance;
            }

            if (ContainsSemanticLabelPrefix(semanticLabels, "OUT")) {
                return WhiteboxVisualType.PortalExit;
            }

            switch (tileType) {
                case TileType.HighGround:
                    return WhiteboxVisualType.HighGround;
                case TileType.Hole:
                    return WhiteboxVisualType.Hole;
                case TileType.Forbidden:
                    return WhiteboxVisualType.Forbidden;
                default:
                    return WhiteboxVisualType.Ground;
            }
        }

        private static bool ContainsSemanticLabelPrefix(List<string> semanticLabels, string prefix) {
            if (semanticLabels == null || string.IsNullOrWhiteSpace(prefix)) {
                return false;
            }

            foreach (var semanticLabel in semanticLabels) {
                if (!string.IsNullOrWhiteSpace(semanticLabel)
                    && semanticLabel.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }
    }
}
