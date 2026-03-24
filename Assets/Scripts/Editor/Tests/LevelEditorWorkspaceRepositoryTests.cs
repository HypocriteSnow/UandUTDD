using ArknightsLite.Editor.LevelEditor.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelEditorWorkspaceRepositoryTests {
        [Test]
        public void SaveWorkspace_CreatesReusableWorkspaceAsset() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.ExportName = "Tutorial_01";

            string assetPath = null;

            try {
                assetPath = LevelEditorWorkspaceRepository.SaveAsNewAsset(workspace);
                var restored = LevelEditorWorkspaceRepository.LoadAsset(assetPath);

                Assert.AreEqual("Tutorial_01", restored.Workspace.LevelName);
                Assert.AreEqual("Tutorial_01", restored.Workspace.ExportName);
            } finally {
                if (!string.IsNullOrWhiteSpace(assetPath)) {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
        }

        public static void RunFromCommandLine() {
            new LevelEditorWorkspaceRepositoryTests().SaveWorkspace_CreatesReusableWorkspaceAsset();
            Debug.Log("[LevelEditorTests] LevelEditorWorkspaceRepositoryTests passed.");
        }
    }
}
