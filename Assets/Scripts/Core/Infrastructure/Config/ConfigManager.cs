namespace ArknightsLite.Infrastructure {
    using System.Collections.Generic;
    using UnityEngine;

    public class ConfigManager : MonoSingleton<ConfigManager> {
        private Dictionary<string, object> _configCache = new Dictionary<string, object>();

        public T LoadConfig<T>(string path) where T : Object {
            if (_configCache.ContainsKey(path)) {
                return _configCache[path] as T;
            }

            // Assume Resources load for MVP
            // Note: path should be relative to Resources folder
            T config = Resources.Load<T>(path);
            if (config == null) {
                Debug.LogWarning($"Config not found at path: {path}");
                return null;
            }

            _configCache[path] = config;
            return config;
        }

        public T GetConfig<T>(string path) where T : Object {
            if (_configCache.TryGetValue(path, out var config)) {
                return config as T;
            }
            return LoadConfig<T>(path);
        }
        
        public void PreloadConfigs(List<string> paths) {
            foreach (var path in paths) {
                LoadConfig<Object>(path); 
            }
        }
        
        public void ClearCache() {
            _configCache.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}
