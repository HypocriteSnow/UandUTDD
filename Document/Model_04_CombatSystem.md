# 核心逻辑层 - 04. 战斗核心系统 (CombatSystem)

## 1. 系统概述
`BattleModel` 负责处理所有的战斗交互，包括索敌 (Targeting)、攻击 (Attacking) 和伤害结算 (Damage Calculation)。它是游戏中最复杂的数值交换中心。

## 2. 核心职责
1.  **索敌**：根据攻击范围和优先级筛选目标。
2.  **攻击执行**：处理攻击冷却 (Attack Speed) 和动作触发。
3.  **伤害流水线**：执行 `(ATK - DEF) * Multiplier` 等公式计算，并处理闪避、暴击等逻辑。

## 3. 核心逻辑

### 3.1 索敌系统 (Targeting)
采用 **过滤器链 (Filter Chain)** 模式，灵活组合索敌规则。

```csharp
public class TargetFinder {
    // 查找目标
    public Entity FindTarget(Entity attacker, List<Entity> candidates) {
        Entity bestTarget = null;
        float maxPriority = float.MinValue;

        foreach (var candidate in candidates) {
            // 1. 范围检测
            if (!IsInAttackRange(attacker, candidate)) continue;
            
            // 2. 有效性检测 (无敌、隐匿、阵营)
            if (!IsValidTarget(attacker, candidate)) continue;

            // 3. 优先级打分 (嘲讽等级 > 距离终点 > 防御力)
            float score = CalculatePriorityScore(attacker, candidate);
            if (score > maxPriority) {
                maxPriority = score;
                bestTarget = candidate;
            }
        }
        return bestTarget;
    }

    private float CalculatePriorityScore(Entity attacker, Entity target) {
        // 示例：优先攻击距离终点最近的敌人
        // 假设 PathProgress 越大越接近终点
        return target.GetComponent<StateComponent>().PathProgress;
    }
}
```

### 3.2 伤害流水线 (Damage Pipeline)
伤害计算不应是一个简单的函数，而是一个可被拦截和修饰的过程。

```csharp
public struct DamageInfo {
    public Entity Attacker;
    public Entity Target;
    public DamageType Type; // Physical, Magical, True
    public int RawDamage;
    public bool IsCritical;
}

public class DamageCalculator {
    public void ApplyDamage(DamageInfo info) {
        // 1. 获取攻防属性 (考虑 Buff)
        int atk = info.RawDamage;
        int def = info.Target.GetComponent<AttributeComponent>().Defense;
        int res = info.Target.GetComponent<AttributeComponent>().MagicResist;

        // 2. 计算最终伤害
        int finalDamage = 0;
        switch (info.Type) {
            case DamageType.Physical:
                finalDamage = Mathf.Max(atk - def, (int)(atk * 0.05f)); // 抛光保底 5%
                break;
            case DamageType.Magical:
                finalDamage = Mathf.Max((int)(atk * (1 - res / 100f)), 0);
                break;
            case DamageType.True:
                finalDamage = atk;
                break;
        }

        // 3. 扣除 HP
        var targetAttr = info.Target.GetComponent<AttributeComponent>();
        targetAttr.CurrentHP -= finalDamage;

        // 4. 广播事件 (UI飘字, 受击回复SP)
        EventManager.Instance.Broadcast(new GameEvents.DamageDealtEvent { 
            Info = info, 
            FinalDamage = finalDamage 
        });

        // 5. 死亡判定
        if (targetAttr.CurrentHP <= 0) {
            EntityModel.Instance.RemoveEntity(info.Target);
        }
    }
}
```

### 3.3 BattleModel (单例)
```csharp
namespace ArknightsLite.Model {
    using ArknightsLite.Infrastructure;

    public class BattleModel {
        private static BattleModel _instance;
        public static BattleModel Instance => _instance ??= new BattleModel();
        private BattleModel() {}

        private TargetFinder _targetFinder = new TargetFinder();
        private DamageCalculator _damageCalculator = new DamageCalculator();

        public void OnTick(int tick) {
            // 遍历所有干员执行攻击逻辑
            // 注意：实际项目中可能由 Entity 自身的 AttackComponent 驱动，
            // 这里为了演示，假设由 BattleModel 统一调度
        }

        public void PerformAttack(Entity attacker, Entity target) {
            // 构建伤害包
            var dmgInfo = new DamageInfo {
                Attacker = attacker,
                Target = target,
                Type = DamageType.Physical,
                RawDamage = attacker.GetComponent<AttributeComponent>().Attack
            };
            
            _damageCalculator.ApplyDamage(dmgInfo);
        }
    }
}
```

## 4. 扩展性设计
*   **Buff 系统**：在 `AttributeComponent` 中引入 `Buff` 列表，在获取 `Attack`/`Defense` 属性时动态计算加成。
*   **攻击特效**：`PerformAttack` 触发时，通过 Event 通知 View 播放弹道动画，弹道命中时再回调 `ApplyDamage`（投射物逻辑）。
