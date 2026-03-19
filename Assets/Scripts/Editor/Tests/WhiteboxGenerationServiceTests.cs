using ArknightsLite.Editor.LevelEditor.Core;
using ArknightsLite.Editor.LevelEditor.Services;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class WhiteboxGenerationServiceTests {
        [Test]
        public void EnsureWhitebox_CreatesExpectedTileCountFromWorkspaceSize() {
            var workspace = LevelEditorWorkspace.CreateNew("Tutorial_01");
            workspace.MapWidth = 4;
            workspace.MapDepth = 3;

            var result = WhiteboxGenerationService.BuildPreview(workspace);

            Assert.AreEqual(12, result.TileCount);
        }

        public static void RunFromCommandLine() {
            new WhiteboxGenerationServiceTests().EnsureWhitebox_CreatesExpectedTileCountFromWorkspaceSize();
            Debug.Log("[LevelEditorTests] WhiteboxGenerationServiceTests passed.");
        }
    }
}
