# 核心逻辑层 - 02. 实体架构系统 (EntitySystem)

## 1. 系统概述
`EntityModel` 负责管理战场上所有活动对象（干员、敌人、子弹、陷阱）。采用**组合优于继承**的设计思想，通过组件 (`EntityComponent`) 来定义实体的行为特征。

## 2. 核心职责
1.  **生命周期管理**：实体的创建 (Spawn)、更新 (Tick)、销毁 (Despawn)。
2.  **对象池管理**：高频创建的实体（如 Projectile, Enemy）必须使用对象池。
3.  **组件管理**：管理实体的属性、状态、位置等数据组件。

## 3. 数据结构

### 3.1 Entity 类 (核心容器)
```csharp
public class Entity {
    public int InstanceID { get; private set; }
    public string ConfigID { get; private set; }
    public EntityType Type { get; private set; } // Operator, Enemy, Projectile
    
    // 组件容器
    private Dictionary<Type, IEntityComponent> _components = new Dictionary<Type, IEntityComponent>();

    public T GetComponent<T>() where T : IEntityComponent {
        return _components.TryGetValue(typeof(T), out var comp) ? (T)comp : default;
    }

    public void AddComponent<T>(T component) where T : IEntityComponent {
        _components[typeof(T)] = component;
    }
}
```

### 3.2 核心组件 (Components)
*   **GridPositionComponent**: 记录逻辑坐标 `(x, y)` 和朝向 `Direction`。
*   **AttributeComponent**: 记录 HP, MaxHP, ATK, DEF, RES, Cost。
*   **StateComponent**: 记录当前状态 (Idle, Move, Attack, Stunned, Dead)。
*   **BlockComponent**: 记录 `BlockCount` (干员阻挡数) 或 `BlockedBy` (敌人被谁阻挡)。

### 3.3 EntityModel (单例管理器)
```csharp
namespace ArknightsLite.Model {
    using System.Collections.Generic;
    using ArknightsLite.Infrastructure;

    public class EntityModel {
        private static EntityModel _instance;
        public static EntityModel Instance => _instance ??= new EntityModel();
        private EntityModel() {}

        private List<Entity> _entities = new List<Entity>();
        private int _nextId = 1;

        // 核心循环
        public void OnTick(int tick) {
            // 倒序遍历以安全移除
            for (int i = _entities.Count - 1; i >= 0; i--) {
                var entity = _entities[i];
                // 驱动组件逻辑 (如 Buff 倒计时)
                // 注意：移动和战斗逻辑通常由各自的 System 驱动，而不是 Entity 自身
            }
        }

        // 创建实体
        public Entity SpawnEntity(string configId, int x, int y, EntityType type) {
            var entity = new Entity();
            // TODO: 根据 ConfigID 初始化组件属性
            
            _entities.Add(entity);
            
            // 通知 View 层创建 GameObject
            EventManager.Instance.Broadcast(new GameEvents.EntitySpawnedEvent { 
                Entity = entity, 
                X = x, 
                Y = y 
            });
            
            return entity;
        }

        // 移除实体
        public void RemoveEntity(Entity entity) {
            if (_entities.Contains(entity)) {
                _entities.Remove(entity);
                EventManager.Instance.Broadcast(new GameEvents.EntityDespawnedEvent { Entity = entity });
            }
        }
    }
}
```

## 4. 扩展性设计
*   **组件热插拔**：新的机制（如“隐匿”）只需新增 `StealthComponent`，无需修改 `Entity` 基类。
*   **工厂模式**：`SpawnEntity` 内部应使用工厂模式，根据配置表自动组装组件。
