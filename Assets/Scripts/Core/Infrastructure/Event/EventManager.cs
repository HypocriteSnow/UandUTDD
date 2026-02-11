namespace ArknightsLite.Infrastructure {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class EventManager : MonoSingleton<EventManager> {
        private readonly Dictionary<Type, Delegate> _delegates = new Dictionary<Type, Delegate>();

        public void AddListener<T>(Action<T> listener) {
            if (listener == null) return;
            var type = typeof(T);
            if (!_delegates.ContainsKey(type)) {
                _delegates[type] = null;
            }
            _delegates[type] = Delegate.Combine(_delegates[type], listener);
        }

        public void RemoveListener<T>(Action<T> listener) {
            if (listener == null) return;
            var type = typeof(T);
            if (_delegates.ContainsKey(type)) {
                _delegates[type] = Delegate.Remove(_delegates[type], listener);
                if (_delegates[type] == null) {
                    _delegates.Remove(type);
                }
            }
        }

        public void Broadcast<T>(T eventData) {
            var type = typeof(T);
            if (_delegates.TryGetValue(type, out var d)) {
                var callback = d as Action<T>;
                if (callback != null) {
                    try {
                        callback(eventData);
                    } catch (Exception e) {
                        Debug.LogError($"Error broadcasting event {type.Name}: {e}");
                    }
                }
            }
        }
    }
}
