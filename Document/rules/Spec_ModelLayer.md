# 开发规范 - Model 层 (数据与逻辑)

## 1. 核心要求
1.  **纯 C# 类**：不得继承 MonoBehaviour。
2.  **单例模式**：必须实现线程安全的纯 C# 单例。
3.  **数据驱动**：数据变化必须通过 `EventManager` 发布事件，严禁直接调用 View。
4.  **统一驱动**：生命周期由 `GameMain` 驱动，不自行使用 Timer 或 Thread。
5.  **类名后缀**：必须以 `Model` 结尾（如 `BattleModel`, `PlayerModel`）。

## 2. 代码结构模板

### 2.1 单例实现 (Singleton Pattern)

```csharp
namespace ArknightsLite.Model {
    public class ExampleModel {
        // 1. 私有静态实例
        private static ExampleModel _instance;
        
        // 2. 公共静态访问点
        public static ExampleModel Instance {
            get {
                if (_instance == null) {
                    _instance = new ExampleModel();
                }
                return _instance;
            }
        }

        // 3. 私有构造函数 (防止外部 new)
        private ExampleModel() {
            // 初始化数据结构 (List, Dictionary 等)
        }
    }
}
```

### 2.2 数据字段与属性 (Fields & Properties)

```csharp
        // 私有字段
        private int _currentCost;
        
        // 公共属性 (只读或受保护的 set)
        public int CurrentCost => _currentCost;

        // 数据变更通知方法
        private void NotifyCostChanged() {
            // 使用 EventManager 广播事件
            EventManager.Instance.Broadcast(new GameEvents.CostChangedEvent { 
                NewCost = _currentCost 
            });
        }
```

### 2.3 生命周期接口 (Lifecycle)

Model 必须提供以下标准接口供 `GameMain` 调用：

```csharp
        // 初始化 (加载配置、重置数据)
        public void Init() {
            _currentCost = 0;
        }

        // 逻辑更新 (每帧或每 Tick 调用)
        public void OnTick(int tick) {
            // 业务逻辑，如每秒回复费用
            if (tick % 30 == 0) { // 假设 30 ticks = 1 sec
                _currentCost++;
                NotifyCostChanged();
            }
        }

        // 清理 (释放资源)
        public void Clear() {
            _currentCost = 0;
        }
```

## 3. GameMain 集成规范

`GameMain` 是 Model 的驱动者，负责按顺序初始化和更新各个 Model。

```csharp
public class GameMain : MonoBehaviour {
    void Start() {
        // 1. 初始化各个 Model
        BattleModel.Instance.Init();
        EnemyModel.Instance.Init();
    }

    void Update() {
        // 2. 驱动 TimeManager (如果 TimeManager 不是自驱动)
        // 或者监听 TimeManager 的 Tick 事件来驱动 Model
    }
    
    // 推荐方式：监听 TimeManager 的 Tick 事件
    void OnEnable() {
        TimeManager.Instance.RegisterTick(OnTick);
    }

    void OnTick(int tick) {
        // 3. 统一驱动 Model 逻辑
        BattleModel.Instance.OnTick(tick);
        EnemyModel.Instance.OnTick(tick);
    }
}
```

## 4. 代码编写顺序
1.  **单例实现**：写好 `Instance` 和 `private constructor`。
2.  **私有字段**：定义数据。
3.  **公共属性**：暴露数据（Getter）。
4.  **公共方法**：业务逻辑入口（`Init`, `OnTick`, `DoSkill`）。
5.  **事件通知**：在数据变更处插入 `Notify` 调用。
