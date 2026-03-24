namespace ArknightsLite.Editor.LevelEditor.Services {
    using System.Collections.Generic;

    public sealed class LevelValidationResult {
        public List<string> Errors { get; } = new List<string>();

        public bool IsValid => Errors.Count == 0;
    }
}
