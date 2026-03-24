using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Panels;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelRuntimePanelDataTests {
        [Test]
        public void ApplyEdits_WritesRuntimeParametersIntoWorkspace() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");

            LevelRuntimePanel.ApplyEdits(workspace, 35, 5, 0.5f, 2);

            Assert.AreEqual(35, workspace.Runtime.InitialDp);
            Assert.AreEqual(5, workspace.Runtime.BaseHealth);
            Assert.AreEqual(0.5f, workspace.Runtime.DpRecoveryInterval);
            Assert.AreEqual(2, workspace.Runtime.DpRecoveryAmount);
        }

        public static void RunFromCommandLine() {
            new LevelRuntimePanelDataTests().ApplyEdits_WritesRuntimeParametersIntoWorkspace();
            Debug.Log("[LevelEditorTests] LevelRuntimePanelDataTests passed.");
        }
    }
}
