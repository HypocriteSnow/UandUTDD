namespace ArknightsLite.Config {
    using System;

    [Serializable]
    public class OperatorTemplateData {
        public string id = "operator_01";
        public string name = "Operator";
        public string className = string.Empty;
        public string combatType = "MELEE";
        public int type = 0;
        public int cost = 10;
        public int block = 1;
        public int hp = 1000;
        public int atk = 40;
        public int def = 150;
        public float attackSpeed = 1f;
        public int range = 1;
        public int targetCount = 1;
        public string color = "#eab308";
    }
}
