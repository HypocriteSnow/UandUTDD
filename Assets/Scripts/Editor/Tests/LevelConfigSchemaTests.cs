using ArknightsLite.Config;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelConfigSchemaTests {
        [Test]
        public void LevelConfig_NewCollections_AreInitialized() {
            var config = ScriptableObject.CreateInstance<LevelConfig>();

            Assert.NotNull(config.portals);
            Assert.NotNull(config.waves);
            Assert.NotNull(config.enemies);
            Assert.NotNull(config.operators);
        }

        public static void RunFromCommandLine() {
            new LevelConfigSchemaTests().LevelConfig_NewCollections_AreInitialized();
            Debug.Log("[LevelEditorTests] LevelConfigSchemaTests passed.");
        }
    }
}
