namespace ArknightsLite.Editor.LevelEditor.Core {
    using UnityEngine;

    public sealed class LevelEditorWorkspaceAsset : ScriptableObject {
        public LevelEditorWorkspace Workspace = LevelEditorWorkspace.CreateNew("NewLevel");

        public bool EnsureInitialized() {
            if (Workspace == null) {
                Workspace = LevelEditorWorkspace.CreateNew("NewLevel");
                return true;
            }

            return Workspace.EnsureDefaults();
        }
    }
}
