# 开发规范 - View 层 (表现与交互)

## 1. 核心要求
1.  **MonoBehaviour**：View 组件必须挂载在 GameObject 上。
2.  **被动更新**：View 不应在 `Update` 中轮询 Model 数据，而应通过 `EventManager` 监听数据变更事件。
3.  **输入转发**：View 负责捕获用户输入（点击、拖拽），并调用 Model 层的 Public 方法执行逻辑。
4.  **无业务逻辑**：View 只负责“显示”和“交互”，不包含伤害计算、寻路等核心逻辑。

## 2. 代码结构模板

### 2.1 基础结构

```csharp
namespace ArknightsLite.View {
    using UnityEngine;
    using UnityEngine.UI;
    using ArknightsLite.Infrastructure; // 引用 EventManager
    using ArknightsLite.Model;          // 引用 Model (用于调用方法)

    public class ExampleView : MonoBehaviour {
        [SerializeField] private Text _costText;

        private void OnEnable() {
            // 1. 注册事件监听
            EventManager.Instance.AddListener<GameEvents.CostChangedEvent>(OnCostChanged);
            
            // 2. 初始化显示 (可选，从 Model 获取初始值)
            UpdateDisplay(ExampleModel.Instance.CurrentCost);
        }

        private void OnDisable() {
            // 3. 移除事件监听 (必须！)
            EventManager.Instance.RemoveListener<GameEvents.CostChangedEvent>(OnCostChanged);
        }

        // 事件回调
        private void OnCostChanged(GameEvents.CostChangedEvent evt) {
            UpdateDisplay(evt.NewCost);
        }

        // 更新 UI
        private void UpdateDisplay(int cost) {
            _costText.text = $"Cost: {cost}";
        }
    }
}
```

### 2.2 用户交互 (User Interaction)

当用户进行操作时，View 层直接调用 Model 层的方法。

```csharp
        // 绑定到 UI 按钮的 OnClick
        public void OnSkillButtonClicked() {
            // 调用 Model 方法
            BattleModel.Instance.ActivateSkill();
            
            // 注意：不要在这里直接更新 UI (如变灰)，
            // 应该等待 Model 发出 "SkillActivated" 事件后再更新。
            // 这样能确保逻辑成功执行后 UI 才变化。
        }
```

## 3. 实体视图 (Entity View)

对于场景中的单位（干员、敌人），View 层通常包含 `Animator`, `SpriteRenderer` 等组件。

```csharp
    public class EnemyView : MonoBehaviour {
        private Animator _animator;
        private string _entityId;

        public void Bind(string entityId) {
            _entityId = entityId;
            // 监听特定实体的事件 (需要在 Event 中包含 EntityID 进行过滤)
            EventManager.Instance.AddListener<GameEvents.EntityMoveEvent>(OnEntityMove);
        }

        private void OnEntityMove(GameEvents.EntityMoveEvent evt) {
            if (evt.EntityId != _entityId) return; // 过滤非本实体的事件

            transform.position = evt.TargetPos;
            _animator.SetTrigger("Move");
        }
    }
```

## 4. 开发注意事项
- **空检查**：在 `OnDestroy` 或 `OnDisable` 中移除监听时，确保 EventManager 实例仍然存在（防止游戏退出时报错）。
- **性能优化**：对于高频事件（如每帧血条更新），考虑使用脏标记（Dirty Flag）或限制刷新频率，但在 MVP 阶段可先直接刷新。
