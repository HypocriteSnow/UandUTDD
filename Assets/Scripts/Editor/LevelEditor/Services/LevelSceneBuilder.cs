namespace ArknightsLite.Editor.LevelEditor.Services {
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArknightsLite.Config;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class LevelSceneBuilder {
        public static bool TryBuild(LevelConfig config, GridVisualConfig visualConfig, out string scenePath, out string errorMessage) {
            scenePath = string.Empty;
            errorMessage = string.Empty;

            if (config == null || visualConfig == null) {
                errorMessage = "请先配置 LevelConfig 和 GridVisualConfig";
                return false;
            }

            if (!config.Validate()) {
                errorMessage = "LevelConfig 配置无效，请检查 Console";
                return false;
            }

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var gameManagerObj = new GameObject("GameManager");
            var gameMain = gameManagerObj.AddComponent<ArknightsLite.GameMain>();
            SetPrivateField(gameMain, "_levelConfig", config);

            var gridVisualizerObj = new GameObject("GridVisualizer");
            var gridRenderer = gridVisualizerObj.AddComponent<ArknightsLite.View.GridRenderer>();
            SetPrivateField(gridRenderer, "_visualConfig", visualConfig);
            SetPrivateField(gridRenderer, "_levelConfig", config);

            var cameraObj = new GameObject("Main Camera");
            var camera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            cameraObj.AddComponent<AudioListener>();

            var mapCenterX = config.mapWidth * config.cellSize * 0.5f;
            var mapCenterZ = config.mapDepth * config.cellSize * 0.5f;
            var mapMaxSize = Mathf.Max(config.mapWidth, config.mapDepth) * config.cellSize;

            cameraObj.transform.position = new Vector3(mapCenterX, mapMaxSize * 0.8f, mapCenterZ - mapMaxSize * 0.5f);
            cameraObj.transform.rotation = Quaternion.Euler(45, 0, 0);

            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            scenePath = $"Assets/Scenes/Levels/{config.name}.unity";
            var sceneDir = Path.GetDirectoryName(scenePath);
            if (!Directory.Exists(sceneDir)) {
                Directory.CreateDirectory(sceneDir);
            }

            if (!EditorSceneManager.SaveScene(newScene, scenePath)) {
                errorMessage = "场景保存失败";
                return false;
            }

            AddSceneToBuildSettings(scenePath);
            return true;
        }

        private static void SetPrivateField(object target, string fieldName, object value) {
            if (target == null) {
                return;
            }

            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) {
                field.SetValue(target, value);
            }
        }

        private static void AddSceneToBuildSettings(string scenePath) {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var exists = scenes.Exists(scene => scene.path == scenePath);
            if (!exists) {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
