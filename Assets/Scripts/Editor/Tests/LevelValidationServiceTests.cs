using ArknightsLite.Config;
using ArknightsLite.Editor.LevelEditor.Services;
using NUnit.Framework;
using UnityEngine;

namespace ArknightsLite.Editor.Tests.LevelEditor {
    public class LevelValidationServiceTests {
        [Test]
        public void Validate_Fails_WhenWaveReferencesUnknownEnemy() {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.waves.Add(new WaveDefinition {
                waveId = "w1",
                enemyId = "missing_enemy"
            });

            var result = LevelValidationService.Validate(config);

            Assert.IsFalse(result.IsValid);
            Assert.That(result.Errors, Has.Some.Contains("missing_enemy"));
        }

        public static void RunFromCommandLine() {
            new LevelValidationServiceTests().Validate_Fails_WhenWaveReferencesUnknownEnemy();
            Debug.Log("[LevelEditorTests] LevelValidationServiceTests passed.");
        }
    }
}
