# 开发规范 - Manager 层 (基础设施服务)

## 1. 核心要求
1.  **MonoSingleton**：Manager 通常需要利用 Unity 生命周期（如 `Update`, `Coroutine`, `OnApplicationQuit`），因此推荐继承 `MonoSingleton<T>`。
2.  **全局服务**：提供跨越 Model 和 View 的通用服务（时间、事件、配置、资源）。
3.  **无业务逻辑**：Manager 只负责“如何做”（机制），不负责“做什么”（策略）。例如 `TimeManager` 负责计时，但不决定何时刷怪。

## 2. 核心 Manager 列表与职责

### 2.1 EventManager (事件总线)
*   **职责**：提供强类型的发布/订阅机制。
*   **规范**：详见 `Document/Infra_02_EventManager.md`。
*   **关键点**：必须线程安全（如果涉及多线程），但在 Unity 主线程模型下主要关注空引用保护。

### 2.2 TimeManager (时间管理)
*   **职责**：提供 Tick 驱动与时间缩放。
*   **规范**：详见 `Document/Infra_01_TimeManager.md`。
*   **关键点**：`GameMain` 应当监听 `TimeManager` 的 Tick 事件来驱动 Model 层。

### 2.3 ConfigManager (配置管理)
*   **职责**：加载与缓存静态数据。
*   **规范**：详见 `Document/Infra_03_ConfigManager.md`。
*   **关键点**：提供同步/异步加载接口，返回只读数据结构。

### 2.4 ResourceManager (资源管理 - 预留)
*   **职责**：加载 Prefab, Sprite, AudioClips。
*   **API 示例**：
    ```csharp
    public T Load<T>(string path) where T : Object;
    public void Release(Object obj);
    ```

## 3. Manager 与其他层的交互

### 3.1 被 Model 层调用
Model 层调用 Manager 获取数据或发送通知。
*   `EventManager.Instance.Broadcast(...)`
*   `ConfigManager.Instance.GetConfig(...)`

### 3.2 被 View 层调用
View 层调用 Manager 注册监听或获取资源。
*   `EventManager.Instance.AddListener(...)`
*   `ResourceManager.Instance.Load<Sprite>(...)`

### 3.3 被 GameMain 驱动
虽然 Manager 是单例，但某些 Manager 可能需要初始化参数或重置状态，由 `GameMain` 在启动时统一配置。

```csharp
// GameMain.cs
void Awake() {
    // 确保所有 Manager 初始化完毕
    TimeManager.Instance.Init();
    ConfigManager.Instance.LoadAll();
}
```

## 4. 扩展性原则
*   **接口抽象**：如果预见到未来会更换实现（如从 Resources 加载改为 Addressables），应提取接口 `IResourceManager`，让上层依赖接口而非具体类。
*   **服务定位器**：虽然我们使用单例模式，但在单元测试中，单例可能难以 Mock。如果项目规模扩大，可考虑引入简单的 Service Locator 或依赖注入 (Zenject/VContainer)，但在 MVP 阶段保持单例即可。
