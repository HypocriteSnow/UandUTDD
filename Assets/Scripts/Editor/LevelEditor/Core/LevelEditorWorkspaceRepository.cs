namespace ArknightsLite.Editor.LevelEditor.Core {
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public static class LevelEditorWorkspaceRepository {
        public const string DefaultRootFolder = "Assets/Editor/LevelEditor/Workspaces";

        public static string SaveAsNewAsset(LevelEditorWorkspace workspace) {
            if (workspace == null) {
                throw new ArgumentNullException(nameof(workspace));
            }

            workspace.EnsureDefaults();
            EnsureFolderExists(DefaultRootFolder);

            string baseName = SanitizeAssetName(workspace.LevelName);
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{DefaultRootFolder}/{baseName}.asset");
            var asset = ScriptableObject.CreateInstance<LevelEditorWorkspaceAsset>();
            asset.Workspace = workspace;
            asset.EnsureInitialized();

            AssetDatabase.CreateAsset(asset, assetPath);
            Save(asset);
            return assetPath;
        }

        public static void Save(LevelEditorWorkspaceAsset asset) {
            if (asset == null) {
                throw new ArgumentNullException(nameof(asset));
            }

            asset.EnsureInitialized();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static LevelEditorWorkspaceAsset LoadAsset(string assetPath) {
            if (string.IsNullOrWhiteSpace(assetPath)) {
                return null;
            }

            var asset = AssetDatabase.LoadAssetAtPath<LevelEditorWorkspaceAsset>(assetPath);
            if (asset == null) {
                return null;
            }

            if (asset.EnsureInitialized()) {
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }

            return asset;
        }

        private static void EnsureFolderExists(string folderPath) {
            string normalized = folderPath.Replace("\\", "/");
            string[] parts = normalized.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++) {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next)) {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static string SanitizeAssetName(string assetName) {
            string resolved = string.IsNullOrWhiteSpace(assetName) ? LevelEditorWorkspace.DefaultLevelName : assetName.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars()) {
                resolved = resolved.Replace(invalid, '_');
            }

            return string.IsNullOrWhiteSpace(resolved) ? LevelEditorWorkspace.DefaultLevelName : resolved;
        }
    }
}
