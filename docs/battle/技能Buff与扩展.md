# 技能、Buff 与扩展

[战斗文档导航](../战斗系统与DSL.md) · [返回首页](../../README.md)

## DSL 如何接入技能、子物体与 Buff

### 一次技能的创建与完成

[CustomNodeSkillExtensions.cs](../../Assets/Hotfix/GameLogic/CoreGame/BattleKernel/LogicDsl/NodeExtensions/CustomNodeSkillExtensions.cs) 的现有路径为：

```text
调用方提供施法实体、技能表 ID 和输入黑板
  → SkillFirePolicy 判断是否允许创建
  → 查询 TSkillLogic，取得 LogicID
  → SkillLogicOverrideProvider 按需替换 LogicID
  → 补充技能黑板，构造 SkillLogicGenInfo
  → 在 LogicConfigs_Skill 容器中创建 SkillLogic
  → 发送技能命令，将逻辑挂到 ComSkillProcess
  → SysSkillProcess.Update 推进实例
  → CanStop 为 true 时重置相关 CD 并移除组件
  → 组件 DisposeOnRemove 回收逻辑
```

`SysSkillProcess` 使用 `BattleTime.GetDeltaTime`，并结合实体的子弹时间补偿系数。因而 DSL 的时间行为应使用收到的 dt，避免在节点里直接混用 `Time.deltaTime`。

`CreateSkillProcessLogic` 本身只创建实例；`CreateSkillProcess` 才包含挂载组件等流程。若直接使用前者，就需要自己负责实例的驱动和释放。

当前创建路径依赖 `MainFsmGenInfo`、技能表、技能根类型和配置容器。普通 `CustomLogic` 配置不能直接当成可运行技能；根类型、输入上下文和驱动协议都需要对应。

### 子物体与 Buff

- [SysSubobject.cs](../../Assets/Hotfix/GameLogic/CoreGame/BattleKernel/World/Systems/Battle/SysSubobject.cs) 获取 `ComSubobject` 中的逻辑并推进。它的 `Execute` 本身没有统一的 `CanStop → 销毁` 分支，不能照搬技能的结束规则。
- [SysBuff.cs](../../Assets/Hotfix/GameLogic/CoreGame/BattleKernel/World/Systems/Battle/SysBuff.cs) 推进实体的 Buff 中心；添加、合并、到期和移除由 [BuffCenterComponent.cs](../../Assets/Hotfix/GameLogic/CoreGame/BattleKernel/World/LogicWorld/Component/BuffCenterComponent/BuffCenterComponent.cs) 等类型负责。
- Buff 还有添加、移除、状态变化和伤害前等事件节点。新增效果前应确认它属于持续更新还是事件响应，避免放在错误的生命周期入口。

同样是 DSL 实例，不同持有者拥有不同的外部协议和结束条件。创建通用逻辑是底层能力，战斗组件和系统才赋予它技能、Buff 或子物体的业务语义。

### 命中与伤害处理

[HandleSubobjectHitCmd.cs](../../Assets/Hotfix/GameLogic/CoreGame/BattleKernel/LogicDsl/Nodes/Subobject/HandleSubobjectHitCmd.cs) 提供子物体命中处理节点；[MakeDamageTo](../../Assets/Hotfix/GameLogic/CoreGame/BattleKernel/LogicDsl/Authoring/LogicConfigBase/LogicConfigBase_Damage.cs) 提供针对黑板目标派发 `EvtDamage` 的配置方法。后者需要战斗世界、目标与伤害服务上下文。

```text
命中处理 / 伤害行为
  → 构造 EvtDamage / DamageContext
  → DamageEventService 派发
  → SysHandleDamage 已注册的处理函数
  → 计算伤害 → 应用结果 → 记录伤害 → 派发 EvtAfterDamage
```

[SysHandleDamage.cs](../../Assets/Hotfix/GameLogic/CoreGame/BattleKernel/World/Systems/Battle/SysHandleDamage/SysHandleDamage.cs) 在 `Initialize()` 注册伤害与治疗处理函数，在 `TearDown()` 移除。它没有逐帧 `Execute()`；事件同步还是排队派发取决于注入的服务实现，不能根据它位于 Battle 注册阶段就断言伤害一律在帧末统一结算。

新增技能时优先复用现有伤害入口，让计算、应用和后续通知保持完整，不在某个节点里直接修改生命值绕过这条链路。死亡转换和最终实体清理由相应处理及 `SysDeathProcess` 承接。

## 如何扩展

### 新增行为节点

1. 先确认已有节点或 `DelegateBhvCfg` 能否表达需求，避免为一次简单调用增加类型。
2. 需要复用或保存独立运行状态时，新增 `ICustomNodeCfg` 实现，通过 `NodeType()` 返回节点类型；节点继承 `BehaviorNode<TCfg>`，在 `OnBegin` 或 `OnUpdate` 中处理行为。
3. 持续行为明确实现停止约束，并正确返回剩余时间片。即时行为通常只需 `OnBegin`，不必引入自己的计时循环。
4. 在初始化与 `Reset` 中维护可重复执行的状态，在 `Destroy` 中清理引用、订阅及资源并调用基类。
5. 项目专用节点放 Gameplay，通用战斗节点放 Kernel，通用组合能力放共享节点层。节点通过注入接口取得玩法差异。

至少验证一次执行、重复执行和中途回收。使用对象池的节点尤其需要确认上一轮的状态不会进入下一轮。

### 新增技能或 Buff 配置

先补齐当前缺失的玩法容器，在注册器中完成注册，再为技能或 Buff 建立正确的根类型、配置 ID 和节点图。逐项核对输入黑板的 Key/类型、强类型 GenInfo、表数据映射与持有组件。

随后确认对应 System 的驱动和移除路径。技能看 `SysSkillProcess` 与 `SkillProcessComponent`；Buff 看 Buff 中心及其事件回调。先用最小逻辑验证创建、推进、停止和回收，再加入索敌、伤害及表现。

### 补齐玩法世界

当前 `BattleKernelWorld` 是抽象类。实际接入还需要玩法世界子类及 CreationInfo，准备模式配置、服务实现和模式 GenInfo。玩法初始化必须在内核基础初始化之后、模式初始化之前完成；扩展 `AddInitializeSystems` 时先保留基类注册。

这部分属于玩法实现工作，本页未生成对应代码，也没有将旧文档中的世界类当作现成可复制入口。

内容依据当前源码整理，代码示例未在 Unity 中编译运行。完整战斗案例见 [施法到清理案例](施法到清理案例.md)。

动态前摇、连射、子物体穿透与灼烧 DOT 的实际配置经验，见 [技能与 Buff 示例](技能与Buff示例.md)。
