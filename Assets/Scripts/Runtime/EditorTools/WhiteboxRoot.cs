using UnityEngine;

namespace ArknightsLite.Editor.LevelEditor.Services {
    public sealed class WhiteboxRoot : MonoBehaviour {
        private const string DefaultLevelName = "NewLevel";

        [SerializeField] private string _levelName = string.Empty;
        [SerializeField] private int _mapWidth;
        [SerializeField] private int _mapDepth;
        [SerializeField] private float _cellSize = 1f;

        public string LevelName => _levelName;
        public int MapWidth => _mapWidth;
        public int MapDepth => _mapDepth;
        public float CellSize => _cellSize;

        public void ApplyLayout(string levelName, int mapWidth, int mapDepth, float cellSize) {
            _levelName = string.IsNullOrWhiteSpace(levelName) ? DefaultLevelName : levelName;
            _mapWidth = Mathf.Max(1, mapWidth);
            _mapDepth = Mathf.Max(1, mapDepth);
            _cellSize = Mathf.Max(0.1f, cellSize);
            gameObject.name = $"{_levelName}_Whitebox";
        }
    }
}
