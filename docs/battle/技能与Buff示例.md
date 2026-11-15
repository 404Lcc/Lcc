# 技能与 Buff 示例

[战斗文档导航](../战斗系统与DSL.md) · [施法到清理案例](施法到清理案例.md)

本页通过动态前摇、连射、子物体穿透、灼烧 DOT 和合并规则五个例子，介绍如何组合节点、传递参数并处理运行时状态。示例参考另一份使用此框架的项目，部分 Gameplay 辅助方法需要自行实现，各节分别说明 Lcc 已有能力与需要补充的部分。

代码片段省略完整类型外壳及部分业务功能，尚未在 Unity 中编译运行。使用时需补齐所在配置类、上下文和服务。

## 1. 动态前摇：先计算参数，再执行节点

参考项目的 `NormalAttackTiming.Calculate` 会比较基础前摇和攻击间隔：如果攻击间隔为正且短于基础前摇，就把前摇缩到攻击间隔，同时提高动画速度；否则保留基础前摇与正常动画速度。

例如：

| 基础前摇 | 攻击间隔 | 实际前摇 | 动画速度 |
| --- | --- | --- | --- |
| 0.3 秒 | 0.6 秒 | 0.3 秒 | 1 |
| 0.3 秒 | 0.1 秒 | 0.1 秒 | 3 |
| 0.3 秒 | 未提供/非正 | 0.3 秒 | 1 |

`PrepareNormalAttackTiming` 在 BeginCall 中计算，并写入 `CV_AttackBeforeDelay` 与 `CV_AttackAnimationSpeed`，后面的动画和等待节点读取相应变量。实际配置结构可概括为：

```text
Seq
  → 计算本轮前摇和动画速度，写黑板
  → 播放攻击动画，读取速度变量
  → Delay，读取前摇变量
  → 产生攻击效果/发射
```

前摇与动画节点使用同一轮计算结果，可以保持播放速度与发射时机一致。

`PrepareNormalAttackTiming`、动画播放辅助方法属于参考项目玩法代码；Lcc 中可以找到 `BeginCall`、`Delay` 与黑板访问等基础能力，使用前者时需要在 Gameplay 层提供对应实现。输入参数应来自本轮攻击的运行时上下文，不应通过修改共享配置改变某个单位的攻速。

## 2. 连射：波次数与每波发射数分别计算

参考项目的连射实现先发首波，再根据能力决定是否发第二、第三波。每波都能包含主发射与分裂、平行等附加发射。

删去资源和具体玩法后，执行结构为：

```text
Seq
  → 计算攻击时序
  → 等待前摇
  → 计算本轮增强参数
  → 首波发射
  → 计算追加波次和间隔，写黑板
  → Delay(第二波间隔)
  → 检查条件，必要时发第二波
  → Delay(第三波间隔)
  → 检查条件，必要时发第三波
```

没有追加波次时，间隔可以写成零，但**发射行为仍要检查条件**。等待零秒只是不消耗时间，并不会自动取消紧接着的节点。

参考实现还在首波前计算本轮总发射量，用它计算一项增强系数并写黑板供后续使用。该增强系数在本轮固定；后续波次的条件仍读取运行时特性，因此攻击途中发生的特性变化可能影响追加发射。

| 改动 | 应修改的层 |
| --- | --- |
| 延后第二波 | 对应的黑板间隔值 |
| 增加第三波 | 波次条件及计数 |
| 每波增加平行弹 | 单波发射计算 |
| 统一伤害增强 | 首波前计算并按约定传入子物体 |

这个例子说明，组合节点负责时序，玩法委托负责计算和条件。无需为每种连射组合创建一个新的运行时根类型。

## 3. 子物体：先初始化，再设置穿透次数

实际配置先完成碰撞初始化，再用 BeginCall 读取特性并修改碰撞处理器的最大命中次数，最后由常驻命中节点处理命中。以下为去掉玩法细节的节点组织：

```text
根节点列表
  ├─ Subobject_InitColliderByPbCfg()
  ├─ Subobject_SetLocomotionStraightDir(...)
  ├─ BeginCall：读取能力，设置 handler.MaxHitCount
  └─ HandleSubobjectHit(CV_SbjHitFxRes)
```

节点声明顺序与实际发生时机需要分开理解：直线位移辅助方法配置的是初始化委托；碰撞初始化配置的是首次更新委托；后续 BeginCall 在同轮更新中使用碰撞初始化的结果。

如果把修改 `MaxHitCount` 放进更早的 InitializeCall，可能在碰撞处理器尚未创建时访问它。此处依赖的是明确的生命周期顺序，而非简单的代码排列顺序。

参考项目用“基础一次 + 额外穿透次数”的方式设置最大次数；还可能由追踪或其他能力提高最低次数。因此同一颗子物体能否命中后立即消失，不仅受表配置影响，也受运行时修改影响。

Lcc 已包含碰撞初始化、直线位移和命中节点，角色特性读取及穿透计算属于 Gameplay。详见 [贯穿案例的清理说明](施法到清理案例.md)。

## 4. 灼烧 DOT：初始化、持续更新与事件伤害

参考项目的灼烧配置使用 BuffCfg 设置持续时间，附加负面/DOT 标签，并设置叠层与刷新规则。根节点列表负责初始化参数、表现和持续更新：

```text
BuffCfg
  ├─ BeginCall：准备本次表现等参数
  ├─ 绑定效果节点
  └─ UpdateCall：累计时间并按条件派发伤害
```

持续 UpdateCall 放在根列表里，由 BuffLogic 每次更新推进；没有把它放进 Seq 当作一个等待结束的阶段。

下面的方法在每次更新中累计时间，达到间隔后派发一次伤害事件。将其放在 Gameplay 配置类中，配合 UpdateCall 使用：

```csharp
private static void UpdateDot(CustomNode node, float dt)
{
    var owner = node.GetOwnerEntity();
    if (owner == null || owner.IsDead())
    {
        return;
    }

    var interval = node.GetVar<float>("DotInterval", 1f);
    if (interval <= 0f)
    {
        interval = 1f;
    }

    var elapsed = node.GetVar<float>("DotElapsed", 0f) + dt;
    node.SetVar("DotElapsed", elapsed);
    if (elapsed < interval)
    {
        return;
    }

    node.SetVar("DotElapsed", 0f);
    var buff = node.GetVar<BuffLogic>(CvKey.CV_CurBuff);
    if (buff == null)
    {
        return;
    }

    var rate = node.GetVar<float>("DotRate", 0.1f);
    if (rate <= 0f)
    {
        return;
    }

    if (buff.Level > 0)
    {
        rate *= buff.Level;
    }

    var evt = EvtDamage.MakeSkillFactorDmgEvt(buff.BuffGenInfo.SumUnitSource, owner, rate, EDamageType.EdtAmmunition, buff.BuffGenInfo.SupplyId, TElementType.EetFire);
    node.GetLogicWorld().GetCreationInfo<BattleKernelCreationInfo>().DamageEventService.DispatchDamage(evt);
}
```

该片段要求世界和 `DamageEventService` 已正确注入。服务可缺失的路径需要由调用方补充检查。片段包含伤害来源与 SupplyId，完整的统计归因还需要按项目约定补充来源逻辑 ID 等字段。

### 如何理解伤害与时间

- 目标是 Buff 宿主，伤害来源取自 BuffGenInfo 保存的施加者来源，二者不是同一个角色概念。
- 伤害系数乘当前层数，但最终扣血仍经过伤害计算、免疫及策略，不是每跳直接扣固定 HP。
- 事件中区分 `EDamageType` 与 `TElementType`。该示例使用弹药类伤害与火元素；接入自己的玩法时，应分别选择伤害类别与元素类型。
- 计时使用收到的 dt，受到 Buff 所在更新路径的战斗时间与补偿策略影响。

参考实现达到间隔时将计时器清零，每次 Update 最多发一次，不保留余数，也不循环补跳。例如间隔 1 秒、某次累计到 1.4 秒，会结算一次并丢弃多出的 0.4 秒。需要补跳或保留余数的玩法，应在实现时另行规定计时规则。

Buff 的到期、刷新、移除由 BuffLogic 与 Buff 中心负责，不由这个 UpdateCall 自己销毁根逻辑。当前 UpdateBuff 先扣减剩余时长，到期就返回而不再调用 Update；因此“持续 1 秒、间隔 1 秒”不能直接推断一定结算一跳，首跳与到期边界需要结合实际 dt 验证。

## 5. 叠层、刷新时间与换来源是三件事

参考灼烧配置选择 `BuffMergeRule.SumLevelAndResetDuration`。当前合并实现会：

1. 将同逻辑 ID 的新层数合入旧 Buff，并受新请求的 MaxLevel 限制。
2. 更新旧实例的层数和层数元数据，调用 ResetTime 刷新持续时间。
3. 调用升级与状态变化通知，继续使用旧实例，而不是新建两份独立计时的 DOT。

这不等同于把所有状态恢复成刚创建。`DotElapsed` 等自定义计时器不会仅因 ResetTime 自动清零；已有黑板与伤害来源也不会仅因层数合并自动替换成新施加者的数据。是否在升级或状态变化节点中重置，是另一个玩法决定。

| 想要的行为 | 需要确认 |
| --- | --- |
| 叠层并刷新时长 | 合并规则、MaxLevel 和重置持续时间的条件 |
| 每次刷新都重新等待完整跳伤间隔 | 明确重置自定义 tick 计时器 |
| 多来源分别跳伤 | 能否合并、如何区分来源和独立实例 |
| 后施加者接管归因 | 合并时是否更新来源信息，不只更新 Level |

Lcc 保留的 [BuffMergeRule](../../Assets/Hotfix/GameLogic/CoreGame/BattleKernel/World/LogicWorld/Component/BuffCenterComponent/BuffLogic/BuffMergeRule.cs) 可用于对照这些细节。`Buff(...)` 构建辅助方法和绑定效果辅助方法位于参考项目玩法层，Lcc 中不能直接调用完整原配置。

## 6. 验证重点

| 例子 | 验证重点 |
| --- | --- |
| 动态前摇 | 攻击间隔低于前摇时，动画与发射一起调整 |
| 连射 | 无追加、一波追加、两波追加和每波数量 |
| 穿透 | 首次更新初始化顺序及命中后剩余次数 |
| 灼烧 | 首跳时机、低帧率不补跳、来源与元素 |
| 合并 | 层数上限、持续时间、自定义计时及来源保留 |

补齐 Gameplay 后，分别验证单次执行、重复施加、中途停止和对象池复用，覆盖配置创建之后的执行与回收过程。
