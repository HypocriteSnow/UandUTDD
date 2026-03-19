namespace ArknightsLite.Editor.LevelEditor.Services {
    using ArknightsLite.Editor.LevelEditor.Core;
    using UnityEngine;

    public sealed class WhiteboxRoot : MonoBehaviour {
        [SerializeField] private string _levelName = string.Empty;
        [SerializeField] private int _mapWidth;
        [SerializeField] private int _mapDepth;
        [SerializeField] private float _cellSize = 1f;

        public string LevelName => _levelName;
        public int MapWidth => _mapWidth;
        public int MapDepth => _mapDepth;
        public float CellSize => _cellSize;

        public void ApplyWorkspace(LevelEditorWorkspace workspace) {
            if (workspace == null) {
                return;
            }

            _levelName = workspace.LevelName;
            _mapWidth = workspace.MapWidth;
            _mapDepth = workspace.MapDepth;
            _cellSize = workspace.CellSize;
            gameObject.name = $"{workspace.LevelName}_Whitebox";
        }
    }
}
