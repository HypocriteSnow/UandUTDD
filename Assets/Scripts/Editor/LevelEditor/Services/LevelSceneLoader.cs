namespace ArknightsLite.Editor.LevelEditor.Services {
    using System.Reflection;
    using ArknightsLite.Config;
    using UnityEngine;

    public static class LevelSceneLoader {
        public static bool TryLoadFromOpenScene(out LevelConfig config, out GridVisualConfig visualConfig, out string errorMessage) {
            config = null;
            visualConfig = null;
            errorMessage = string.Empty;

            var gameMain = Object.FindObjectOfType<ArknightsLite.GameMain>();
            if (gameMain == null) {
                errorMessage = "当前场景中未找到 GameMain 组件";
                return false;
            }

            config = GetPrivateField<LevelConfig>(gameMain, "_levelConfig");
            if (config == null) {
                errorMessage = "GameMain 中的 LevelConfig 为空";
                return false;
            }

            var gridRenderer = Object.FindObjectOfType<ArknightsLite.View.GridRenderer>();
            if (gridRenderer != null) {
                visualConfig = GetPrivateField<GridVisualConfig>(gridRenderer, "_visualConfig");
            }

            return true;
        }

        private static T GetPrivateField<T>(object target, string fieldName) where T : class {
            if (target == null) {
                return null;
            }

            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) {
                return null;
            }

            return field.GetValue(target) as T;
        }
    }
}
