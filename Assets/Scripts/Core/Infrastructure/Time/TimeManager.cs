namespace ArknightsLite.Infrastructure {
    using System.Collections.Generic;
    using UnityEngine;

    public interface ITickable {
        void OnTick(int tickCount);
    }

    public class TimeManager : MonoSingleton<TimeManager> {
        private readonly List<ITickable> _tickables = new List<ITickable>();
        private readonly List<ITickable> _tickablesToAdd = new List<ITickable>();
        private readonly List<ITickable> _tickablesToRemove = new List<ITickable>();
        
        private int _currentTick = 0;
        private float _tickTimer = 0f;
        private float _tickInterval = 0.033f; // 30 FPS logic
        private bool _isPaused = false;
        
        public int CurrentTick => _currentTick;
        public float DeltaTime => _tickInterval;

        public void RegisterTick(ITickable tickable) {
            if (!_tickables.Contains(tickable) && !_tickablesToAdd.Contains(tickable)) {
                _tickablesToAdd.Add(tickable);
            }
        }

        public void UnregisterTick(ITickable tickable) {
            if (_tickables.Contains(tickable) && !_tickablesToRemove.Contains(tickable)) {
                _tickablesToRemove.Add(tickable);
            }
        }

        public void Pause() {
            _isPaused = true;
            Time.timeScale = 0; 
        }

        public void Resume() {
            _isPaused = false;
            Time.timeScale = 1;
        }

        public void SetTimeScale(float scale) {
            Time.timeScale = scale;
        }

        private void Update() {
            if (_isPaused) return;

            _tickTimer += Time.deltaTime;
            while (_tickTimer >= _tickInterval) {
                _tickTimer -= _tickInterval;
                _currentTick++;
                ProcessTicks();
            }
        }

        private void ProcessTicks() {
            if (_tickablesToAdd.Count > 0) {
                _tickables.AddRange(_tickablesToAdd);
                _tickablesToAdd.Clear();
            }
            
            if (_tickablesToRemove.Count > 0) {
                foreach (var t in _tickablesToRemove) {
                    _tickables.Remove(t);
                }
                _tickablesToRemove.Clear();
            }

            foreach (var tickable in _tickables) {
                try {
                    tickable.OnTick(_currentTick);
                } catch (System.Exception e) {
                    Debug.LogError($"Error in OnTick: {e}");
                }
            }
        }
    }
}
