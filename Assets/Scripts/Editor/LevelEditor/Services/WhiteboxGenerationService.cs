namespace ArknightsLite.Editor.LevelEditor.Services {
    using ArknightsLite.Config;
    using ArknightsLite.Editor.LevelEditor.Core;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class WhiteboxGenerationService {
        public readonly struct WhiteboxPreviewResult {
            public WhiteboxPreviewResult(int tileCount) {
                TileCount = tileCount;
            }

            public int TileCount { get; }
        }

        public static WhiteboxPreviewResult BuildPreview(LevelEditorWorkspace workspace) {
            if (workspace == null) {
                return new WhiteboxPreviewResult(0);
            }

            return new WhiteboxPreviewResult(workspace.MapWidth * workspace.MapDepth);
        }

        public static WhiteboxRoot GenerateIntoOpenScene(LevelEditorWorkspace workspace, GridVisualConfig visualConfig = null) {
            if (workspace == null) {
                return null;
            }

            var root = Object.FindObjectOfType<WhiteboxRoot>();
            if (root == null) {
                root = new GameObject().AddComponent<WhiteboxRoot>();
            }

            root.ApplyWorkspace(workspace);
            RebuildTiles(root.transform, workspace, visualConfig);
            Selection.activeGameObject = root.gameObject;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return root;
        }

        private static void RebuildTiles(Transform rootTransform, LevelEditorWorkspace workspace, GridVisualConfig visualConfig) {
            while (rootTransform.childCount > 0) {
                Object.DestroyImmediate(rootTransform.GetChild(0).gameObject);
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
        }
    }
}
