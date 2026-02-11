# 基础架构层 - 00. 架构总览 (Architecture Overview)

## 1. 核心架构模式：Simplified MV + Manager

本项目采用 **Simplified MV + Manager** 分层架构，旨在实现业务逻辑（Model）与表现层（View）的彻底解耦，并由 Manager 层提供底层服务支持。

### 1.1 架构分层图示

```mermaid
graph TD
    subgraph ViewLayer [View Layer (UI & Presentation)]
        UI[UI Components]
        EntityView[Entity Renderers]
    end

    subgraph ModelLayer [Model Layer (Data & Logic)]
        GameMain[GameMain (Driver)]
        BattleModel[BattleModel]
        EntityModel[EntityModel]
    end

    subgraph ManagerLayer [Manager Layer (Infrastructure)]
        EventManager[EventManager]
        TimeManager[TimeManager]
        ConfigManager[ConfigManager]
    end

    %% Relationships
    GameMain -->|Drives| BattleModel
    GameMain -->|Drives| EntityModel
    
    BattleModel -->|Notifies| EventManager
    EntityModel -->|Notifies| EventManager
    
    EventManager -->|Broadcasts| UI
    EventManager -->|Broadcasts| EntityView
    
    UI -->|Input| BattleModel
    
    ModelLayer -.->|Uses| ManagerLayer
    ViewLayer -.->|Uses| ManagerLayer
```

### 1.2 各层职责定义

1.  **Model Layer (核心逻辑层)**
    *   **职责**：负责所有游戏数据的存储、状态管理和核心业务逻辑计算（如伤害计算、寻路）。
    *   **特性**：
        *   **纯 C# 类**：不继承 MonoBehaviour。
        *   **单例模式**：核心 Model 采用纯 C# 单例。
        *   **数据驱动**：数据变更必须通过 `EventManager` 广播，严禁直接引用 View。
        *   **生命周期**：由 `GameMain` (MonoBehaviour) 统一驱动 (`Init`, `OnTick`, `Clear`)。

2.  **View Layer (表现层)**
    *   **职责**：负责接收用户输入、渲染游戏画面（UI、模型、特效）。
    *   **特性**：
        *   **MonoBehaviour**：挂载在 GameObject 上。
        *   **被动更新**：监听 `EventManager` 事件来刷新界面。
        *   **输入转发**：用户操作直接调用 Model 层提供的 Public 方法。

3.  **Manager Layer (服务层)**
    *   **职责**：提供通用的基础设施服务，不包含具体战斗业务逻辑。
    *   **特性**：
        *   **MonoSingleton**：通常继承 MonoBehaviour 以利用 Unity 生命周期（如 Coroutine, Update），但作为单例存在。
        *   **全局服务**：Time, Event, Config, Resource, Audio。

## 2. 目录结构规范

```text
Assets/
  ├── Scripts/
  │   ├── Core/
  │   │   ├── Infrastructure/    <-- Manager Layer (Time, Event, Config)
  │   │   ├── Model/             <-- Model Layer (BattleModel, EntityModel)
  │   │   └── View/              <-- View Layer (BattleView, EntityView)
  │   ├── Features/              <-- 具体业务功能模块
  │   └── GameMain.cs            <-- 游戏入口与驱动器
  └── Resources/
      └── Configs/
```

## 3. 命名空间规范

*   **Infrastructure**: `ArknightsLite.Infrastructure`
*   **Model**: `ArknightsLite.Model`
*   **View**: `ArknightsLite.View`
*   **Core**: `ArknightsLite.Core`

## 4. 核心原则

1.  **Model 独立性**：Model 层代码不得引用 `UnityEngine.UI` 或任何 View 层代码。
2.  **单向依赖**：View 依赖 Model，Model 不依赖 View。
3.  **统一驱动**：避免 Model 自身通过 `Update` 运行，所有 Model 的更新由 `GameMain` 统一调度，确保执行顺序可控。
