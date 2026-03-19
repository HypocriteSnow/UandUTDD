namespace ArknightsLite.Config {
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class WaveDefinition {
        public string waveId = "wave_01";
        public float time = 0f;
        public string enemyId = string.Empty;
        public int count = 1;
        public float interval = 1f;
        public string spawnId = string.Empty;
        public string targetId = string.Empty;
        public List<PathNodeDefinition> path = new List<PathNodeDefinition>();
    }
}
