namespace ArknightsLite.Config {
    using System;

    [Serializable]
    public class EnemyTemplateData {
        public string id = "enemy_01";
        public string name = "Enemy";
        public string movementType = "GROUND";
        public int blockCost = 1;
        public string specialMechanic = string.Empty;
        public int hp = 300;
        public int atk = 25;
        public int def = 0;
        public float attackSpeed = 1f;
        public float speed = 1f;
        public string color = "#ef4444";
    }
}
