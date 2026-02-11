# 基础架构层 - 02. 事件总线系统 (EventManager)

## 1. 系统概述
为了降低各系统间的耦合度（如：干员造成伤害 -> UI飘字 -> 音效播放），我们需要一个中心化的事件分发系统。本系统采用**发布/订阅 (Pub/Sub)** 模式。

## 2. 核心职责
1. **解耦**：发送者无需知道接收者的存在。
2. **类型安全**：使用泛型或结构体定义事件，避免使用字符串作为事件Key（防止拼写错误）。

## 3. 接口规范

### 3.1 事件定义范式
建议使用 `struct` 或 `class` 定义事件参数，作为事件的唯一标识。

```csharp
// 示例：干员攻击事件
public struct OperatorAttackEvent {
    public Entity Attacker;
    public Entity Target;
    public int DamageValue;
}
```

### 3.2 EventManager API
```csharp
public class EventManager : MonoSingleton<EventManager> {
    // 订阅事件
    public void AddListener<T>(Action<T> listener);
    
    // 取消订阅
    public void RemoveListener<T>(Action<T> listener);
    
    // 广播事件
    public void Broadcast<T>(T eventData);
}
```

## 4. 开发注意事项
- **生命周期管理**：订阅者在销毁（OnDestroy）时**必须**调用 `RemoveListener`，否则会导致内存泄漏或空引用异常。
- **异常处理**：`Broadcast` 内部应包裹 `try-catch`，防止某个监听器的报错中断整个事件链。
- **禁止滥用**：不要用事件总线替代核心逻辑流（如：伤害计算流程应直接调用 Pipeline，而不是发事件让 Pipeline 处理，事件主要用于 UI、音效等副作用）。
