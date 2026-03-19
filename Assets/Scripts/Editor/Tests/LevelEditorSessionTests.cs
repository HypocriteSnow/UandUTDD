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

        public static void RunFromCommandLine() {
            new LevelEditorSessionTests().StartEditing_SetsCurrentModeToMap();
            new LevelEditorSessionTests().SetWorkspace_SetsCurrentWorkspace();
            Debug.Log("[LevelEditorTests] LevelEditorSessionTests passed.");
        }
    }
}
