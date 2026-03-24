using ArknightsLite.Editor.LevelEditor.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelEditorSessionTests {
        [Test]
        public void StartEditing_SetsCurrentModeToMap() {
            var session = new LevelEditorSession();

            session.StartEditing();

            Assert.AreEqual(LevelEditorMode.Map, session.Mode);
        }

        [Test]
        public void SetWorkspace_SetsCurrentWorkspace() {
            var session = new LevelEditorSession();
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            session.SetWorkspace(workspace);

            Assert.AreSame(workspace, session.CurrentWorkspace);
        }

        [Test]
        public void SetWorkspaceAsset_TracksAssetAndCurrentWorkspace() {
            var session = new LevelEditorSession();
            var asset = ScriptableObject.CreateInstance<LevelEditorWorkspaceAsset>();
            asset.Workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            try {
                session.SetWorkspaceAsset(asset);

                Assert.AreSame(asset, session.CurrentWorkspaceAsset);
                Assert.AreSame(asset.Workspace, session.CurrentWorkspace);
            } finally {
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void SetWorkspace_UpdatesTrackedWorkspaceAsset() {
            var session = new LevelEditorSession();
            var asset = ScriptableObject.CreateInstance<LevelEditorWorkspaceAsset>();
            asset.Workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            try {
                session.SetWorkspaceAsset(asset);

                var updatedWorkspace = LevelEditorWorkspace.CreateNew("Tutorial_02");
                session.SetWorkspace(updatedWorkspace);

                Assert.AreSame(updatedWorkspace, session.CurrentWorkspace);
                Assert.AreSame(updatedWorkspace, session.CurrentWorkspaceAsset.Workspace);
            } finally {
                Object.DestroyImmediate(asset);
            }
        }

        public static void RunFromCommandLine() {
            new LevelEditorSessionTests().StartEditing_SetsCurrentModeToMap();
            new LevelEditorSessionTests().SetWorkspace_SetsCurrentWorkspace();
            new LevelEditorSessionTests().SetWorkspaceAsset_TracksAssetAndCurrentWorkspace();
            new LevelEditorSessionTests().SetWorkspace_UpdatesTrackedWorkspaceAsset();
            Debug.Log("[LevelEditorTests] LevelEditorSessionTests passed.");
        }
    }
}
