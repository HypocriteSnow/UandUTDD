namespace ArknightsLite {
    using UnityEngine;
    using ArknightsLite.Infrastructure;
    using ArknightsLite.Model;
    using ArknightsLite.Config;

    /// <summary>
    /// 游戏主入口 - 驱动所有 Model 层的生命周期
    /// 职责：初始化系统、驱动 Model 逻辑更新、响应用户输入
    /// </summary>
    public class GameMain : MonoBehaviour, ITickable {
        
        [Header("关卡配置")]
        [SerializeField] private LevelConfig _levelConfig;
        
        [Header("调试选项")]
        [SerializeField] private bool _autoInit = true;
        [SerializeField] private KeyCode _reloadKey = KeyCode.R;
        
        
        private void Awake() {
            Debug.Log("=== GameMain Awake ===");
            
            // 确保 Manager 单例初始化
            InitializeManagers();
        }
        
        private void Start() {
            Debug.Log("=== GameMain Start ===");
            
            if (_autoInit) {
                InitializeGame();
            }
        }
        
        private void OnEnable() {
            // 注册到 TimeManager 的 Tick 事件
            TimeManager.Instance.RegisterTick(this);
        }
        
        private void OnDisable() {
            // 注销 Tick 事件
            if (TimeManager.Instance != null) {
                TimeManager.Instance.UnregisterTick(this);
            }
        }
        
        
        // ==================== 初始化流程 ====================
        
        /// <summary>
        /// 初始化所有 Manager
        /// </summary>
        private void InitializeManagers() {
            // 触发单例实例化（确保生命周期正确）
            var timeManager = TimeManager.Instance;
            var eventManager = EventManager.Instance;
            var configManager = ConfigManager.Instance;
            
            Debug.Log("[GameMain] Managers initialized");
        }
        
        /// <summary>
        /// 初始化游戏
        /// </summary>
        private void InitializeGame() {
            Debug.Log("=== Initializing Game ===");
            
            // 1. 加载关卡配置
            if (_levelConfig == null) {
                Debug.LogError("[GameMain] LevelConfig is not assigned!");
                return;
            }
            
            // 2. 初始化 GridModel
            GridModel.Instance.LoadFromConfig(_levelConfig);
            
            // 3. 通知 View 层生成场景（通过事件或直接调用）
            // 注意：这里通过 FindObjectOfType 获取 GridRenderer
            // 更好的做法是通过事件系统，但为了简化先直接调用
            var gridRenderer = FindObjectOfType<ArknightsLite.View.GridRenderer>();
            if (gridRenderer != null) {
                gridRenderer.GenerateGrid();
            }
            
            // 4. 未来在这里初始化其他 Model
            // EntityModel.Instance.Init();
            // NavModel.Instance.Init();
            // BattleModel.Instance.Init();
            
            Debug.Log("=== Game Initialized ===");
        }
        
        
        // ==================== 游戏循环 ====================
        
        /// <summary>
        /// 逻辑帧更新 - 由 TimeManager 驱动
        /// </summary>
        public void OnTick(int tickCount) {
            // 驱动所有 Model 的 Tick
            GridModel.Instance.OnTick(tickCount);
            
            // 未来在这里驱动其他 Model
            // EntityModel.Instance.OnTick(tickCount);
            // NavModel.Instance.OnTick(tickCount);
            // BattleModel.Instance.OnTick(tickCount);
        }
        
        
        // ==================== 调试功能 ====================
        
        private void Update() {
            // 热重载关卡
            if (Input.GetKeyDown(_reloadKey)) {
                Debug.Log("[GameMain] Reloading level...");
                GridModel.Instance.Clear();
                InitializeGame();
            }
        }
        
        
        // ==================== 公共接口 ====================
        
        /// <summary>
        /// 手动初始化（用于编辑器按钮）
        /// </summary>
        [ContextMenu("Initialize Game")]
        public void ManualInitialize() {
            InitializeGame();
        }
        
        /// <summary>
        /// 清理游戏数据
        /// </summary>
        [ContextMenu("Clear Game")]
        public void ClearGame() {
            GridModel.Instance.Clear();
            Debug.Log("[GameMain] Game cleared");
        }
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame() {
            TimeManager.Instance.Pause();
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame() {
            TimeManager.Instance.Resume();
        }
        
        /// <summary>
        /// 设置时间缩放
        /// </summary>
        public void SetTimeScale(float scale) {
            TimeManager.Instance.SetTimeScale(scale);
        }
    }
}
