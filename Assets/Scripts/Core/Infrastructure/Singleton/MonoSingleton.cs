namespace ArknightsLite.Infrastructure {
    using UnityEngine;

    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> {
        private static T _instance;
        public static T Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null) {
                        GameObject obj = new GameObject(typeof(T).Name);
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake() {
            if (_instance == null) {
                _instance = (T)this;
                DontDestroyOnLoad(gameObject);
            } else if (_instance != this) {
                Destroy(gameObject);
            }
        }
    }
}
