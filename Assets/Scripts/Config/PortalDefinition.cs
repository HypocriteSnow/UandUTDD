namespace ArknightsLite.Config {
    using System;
    using UnityEngine;

    [Serializable]
    public class PortalDefinition {
        public string id = "portal_01";
        public Vector2Int inPos;
        public Vector2Int outPos;
        public float delay = 0f;
        public string color = "#ffffff";
    }
}
