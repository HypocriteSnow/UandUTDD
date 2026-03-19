namespace ArknightsLite.Editor.LevelEditor.Services {
    using System.Collections.Generic;
    using ArknightsLite.Config;

    public static class LevelValidationService {
        public static LevelValidationResult Validate(LevelConfig config) {
            var result = new LevelValidationResult();
            if (config == null) {
                result.Errors.Add("LevelConfig is null.");
                return result;
            }

            var enemyIds = new HashSet<string>();
            foreach (var enemy in config.enemies) {
                if (enemy == null || string.IsNullOrEmpty(enemy.id)) {
                    continue;
                }

                enemyIds.Add(enemy.id);
            }

            foreach (var wave in config.waves) {
                if (wave == null || string.IsNullOrEmpty(wave.enemyId)) {
                    continue;
                }

                if (!enemyIds.Contains(wave.enemyId)) {
                    var waveId = string.IsNullOrEmpty(wave.waveId) ? "<unnamed>" : wave.waveId;
                    result.Errors.Add($"Wave {waveId} references missing enemy '{wave.enemyId}'.");
                }
            }

            return result;
        }
    }
}
