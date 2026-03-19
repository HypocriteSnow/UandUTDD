using ArknightsLite.Editor.LevelEditor.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelEditorWorkspaceTests {
        [Test]
        public void CreateNewWorkspace_UsesLevelNameAndInitializesRuntimeParameters() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            Assert.AreEqual("Tutorial_01", workspace.LevelName);
            Assert.AreEqual(10, workspace.MapWidth);
            Assert.AreEqual(10, workspace.MapDepth);
            Assert.AreEqual(20, workspace.Runtime.InitialDp);
            Assert.AreEqual(3, workspace.Runtime.BaseHealth);
        }

        public static void RunFromCommandLine() {
            new LevelEditorWorkspaceTests().CreateNewWorkspace_UsesLevelNameAndInitializesRuntimeParameters();
            Debug.Log("[LevelEditorTests] LevelEditorWorkspaceTests passed.");
        }
    }
}
