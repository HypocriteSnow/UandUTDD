# 基础架构层 - 01. 时间管理系统 (TimeManager)

## 1. 系统概述
塔防游戏要求逻辑的**确定性**。`TimeManager` 负责接管 Unity 的原生时间，提供基于 `Tick`（逻辑滴答）的时间驱动机制，实现逻辑帧与渲染帧的分离。

## 2. 核心职责
1. **逻辑帧驱动**：以固定频率（如 30Hz 或 60Hz）分发 `OnTick` 事件。
2. **时间控制**：实现暂停、倍速功能，不影响渲染帧率。
3. **计时器服务**：提供基于 Tick 的倒计时与定时任务。

## 3. 接口规范

### 3.1 ITickable 接口
所有需要随时间更新的逻辑对象（如敌人移动、技能CD）必须实现此接口，**禁止在 `Update()` 中写逻辑**。

```csharp
public interface ITickable {
    void OnTick(int tickCount);
}
```

### 3.2 TimeManager API
```csharp
public class TimeManager : MonoSingleton<TimeManager> {
    // 注册/注销 Tick 事件
    public void RegisterTick(ITickable tickable);
    public void UnregisterTick(ITickable tickable);

    // 时间控制
    public void Pause();
    public void Resume();
    public void SetTimeScale(float scale); // 1x, 2x

    // 获取当前逻辑时间
    public int CurrentTick { get; }
    public float DeltaTime { get; } // 逻辑帧间隔 (e.g., 0.033s)
}
```

## 4. 开发注意事项
- **严禁**在战斗逻辑中使用 `Time.deltaTime`，必须使用 `TimeManager.Instance.DeltaTime` 或固定数值。
- **插值平滑**：渲染层（View）应在 `Update` 中根据 `(当前时间 - 上次Tick时间) / Tick间隔` 计算插值系数，平滑移动表现，避免低逻辑帧率导致的卡顿。
