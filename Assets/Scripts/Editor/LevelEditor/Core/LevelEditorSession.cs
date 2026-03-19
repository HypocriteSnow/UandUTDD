namespace ArknightsLite.Editor.LevelEditor.Core {
    using ArknightsLite.Config;

    public sealed class LevelEditorSession {
        public LevelEditorMode Mode { get; private set; } = LevelEditorMode.Map;
        public LevelConfig CurrentLevel { get; private set; }
        public LevelEditorWorkspace CurrentWorkspace { get; private set; }
        public bool IsEditing { get; private set; }

        public void StartEditing(LevelConfig config = null) {
            CurrentLevel = config;
            Mode = LevelEditorMode.Map;
            IsEditing = true;
        }

        public void StopEditing() {
            IsEditing = false;
        }

        public void SetCurrentLevel(LevelConfig config) {
            CurrentLevel = config;
        }

        public void SetWorkspace(LevelEditorWorkspace workspace) {
            CurrentWorkspace = workspace;
        }

        public void SetMode(LevelEditorMode mode) {
            Mode = mode;
        }
    }
}
