namespace ArknightsLite.Editor.LevelEditor.Core {
    using UnityEngine;

    public sealed class LevelEditorWorkspaceAsset : ScriptableObject {
        public LevelEditorWorkspace Workspace = LevelEditorWorkspace.CreateNew(LevelEditorWorkspace.DefaultLevelName);

        public bool EnsureInitialized() {
            if (Workspace == null) {
                Workspace = LevelEditorWorkspace.CreateNew(LevelEditorWorkspace.DefaultLevelName);
                return true;
            }

            return Workspace.EnsureDefaults();
        }
    }
}
