namespace ArknightsLite.Editor.LevelEditor.Core {
    using System;

    [Serializable]
    public sealed class LevelRuntimeParameters {
        public int InitialDp = 20;
        public int BaseHealth = 3;
        public float DpRecoveryInterval = 1f;
        public int DpRecoveryAmount = 1;
    }
}
