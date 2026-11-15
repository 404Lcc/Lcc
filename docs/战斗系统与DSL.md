# 战斗系统与 DSL

[返回首页](../README.md) · [架构说明](架构说明.md)

本组文档介绍战斗框架的分层、DSL 运行机制和玩法接入方式，帮助你配置技能、子物体与 Buff，并定位创建、执行和回收中的问题。

## 当前实现与接入前提

当前仓库包含战斗内核、通用节点、黑板、工厂及技能/Buff 接入代码。`BattleGameplay` 的配置注册器目前为空，具体玩法世界、玩法初始化系统与技能配置需要自行实现并注册，完成这些接入后才能运行完整战斗。

现有 [战斗框架 AI 必读](../Assets/Hotfix/GameLogic/CoreGame/战斗框架AI必读.md) 可作为分层约束参考，其中的旧命名空间与部分路径应以实际源码为准。

各部分职责如下：

| 部分 | 负责什么 | 典型入口 |
| --- | --- | --- |
| 战斗世界 | 创建上下文、装配系统、驱动系统执行 | `ECWorlds`、`BattleKernelWorld` |
| Entity / Component | 标识战斗对象，保存其状态及逻辑实例 | `LogicEntity`、`SkillProcessComponent` |
| System | 找出符合条件的实体，推进逻辑或处理状态 | `SysSkillProcess`、`SysBuff` |
| DSL 配置 | 描述一个逻辑由哪些节点、参数和组合关系构成 | `LogicConfigBase`、`CustomLogicCfg` |
| DSL 运行时 | 保存每次执行的状态，响应更新、命令与查询 | `CustomLogic`、`CustomNode` |
| 玩法层 | 提供具体配置、服务实现及世界接入 | `BattleGameplay`，当前尚待补齐 |

System 决定谁在什么时候被推进，DSL 决定被推进的那段逻辑如何执行。二者在组件持有的运行时逻辑实例处连接起来。

## 阅读导航

设计动机见 [为什么使用 DSL 与 Entity](battle/为什么使用DSL与Entity.md)：了解两者各自解决的问题、配合方式，以及适用场景与使用成本。

| 阅读顺序 | 文档 | 主要内容 |
| --- | --- | --- |
| 1 | [战斗世界与帧循环](battle/战斗世界与帧循环.md) | 分层、实体组件、服务注入与 Update/FixedUpdate |
| 2 | [DSL 配置与执行](battle/DSL配置与执行.md) | 配置容器、运行时节点、黑板、组合与回收 |
| 3 | [技能、Buff 与扩展](battle/技能Buff与扩展.md) | 战斗接入协议、伤害处理与扩展步骤 |
| 4 | [施法到清理案例](battle/施法到清理案例.md) | 最小战斗的准备清单、配置、运行步骤与验收，以及施法到清理调用链 |
| 5 | [玩法接入示例](battle/玩法接入示例.md) | 玩法世界子类、模式初始化、容器注册与服务注入 |
| 6 | [技能与 Buff 示例](battle/技能与Buff示例.md) | 动态前摇、连射、穿透、灼烧 DOT 与叠层规则 |

首次阅读可按表中顺序了解机制，再通过案例追踪完整流程；开始接入时，结合「玩法接入示例」准备世界、服务与配置容器。前四篇基于当前 Lcc，后两篇参考另一份使用此框架的项目，部分 Gameplay 类型与辅助方法需要自行实现。文档中的 C# 示例尚未在 Unity 中编译运行。

## 设计取舍与使用约束

- **配置与实例分离**：节点组合可以复用，每次执行的目标、计时和进度由实例保存。实例状态应保存在运行时，每个创建请求需要匹配容器名、配置 ID 和根类型。
- **System 与 DSL 分工**：System 负责实体筛选和更新驱动，DSL 节点负责时序、条件与行为组合。新增技能时优先组合现有节点，新增驱动规则时再扩展对应 System。
- **强类型上下文与黑板并用**：世界、所属单位等基础依赖由 GenInfo 表达，可变参数通过黑板传递。读写黑板时需要保持 Key 和泛型类型一致，并检查必要参数。
- **对象池与生命周期**：对象池复用减少重复创建；自定义节点仍需正确实现 Reset、Destroy，并清理引用和订阅。实际 GC 分配还取决于节点及服务实现，需要运行时测量。
- **组合与执行时序**：Seq 可以把剩余时间片交给后续行为。碰撞检测、命令派发和伤害处理可能跨不同更新阶段，具体执行时序取决于世界驱动和服务实现。

## 常见问题与定位顺序

| 现象 | 优先检查 |
| --- | --- |
| 创建返回 `null` | 容器是否注册、名称及配置 ID 是否匹配、根类型是否符合请求 |
| 创建成功但行为未执行 | 持有者是否调用 `Update`，节点是否激活，行为是否需要首次更新 |
| 多个步骤同时执行 | 是否把节点直接放在根列表，缺少 `Seq` 包装 |
| 等待结束得过早或永不结束 | 输入 dt、黑板时长、循环次数、停止条件与节点激活状态 |
| 读取到缺省值 | Key 拼写、泛型类型、初始化与首次更新的写入时机 |
| 技能有实例但无效果 | 是否挂载 `ComSkillProcess`，是否具备技能上下文和对应目标参数 |
| 命中无伤害 | 目标是否合法、DamageEventService 是否注入、伤害处理是否已初始化 |
| 模式或世界初始化报错 | GameModeGenInfo、ModeLogicService 及相关容器是否齐备 |
| 第二次执行异常 | Reset/Destroy 是否清理状态，是否共享或继续使用已回收的黑板 |
| 模板被拒绝展开 | 同根装配是否重复引用同一模板 ID，模板是否位于当前容器 |

阅读调用链时先找“谁创建和持有实例”，再找“谁更新、谁移除”，最后进入具体节点。这样能分清未执行是装配问题、驱动问题，还是节点本身的问题。

## 源码阅读索引

| 要理解的问题 | 推荐源码 |
| --- | --- |
| 世界的创建、更新与销毁 | [ECWorlds](../Assets/Hotfix/Framework/CoreGame/ECWorld/ECWorlds.cs) |
| 战斗阶段及系统装配 | [BattleKernelWorld](../Assets/Hotfix/GameLogic/CoreGame/BattleKernel/World/Core/BattleKernelWorld.cs) |
| 配置辅助方法 | [LogicConfigBase](../Assets/Hotfix/GameLogic/CoreGame/BattleFoundation/LogicDsl/LogicConfigBase.cs) |
| 配置定位与实例创建 | [CustomLogicFactory](../Assets/Hotfix/Framework/CoreGame/CustomLogic/CustomLogic.Base/CustomLogicFactory.cs) |
| 根节点驱动、停止与回收 | [CustomLogic](../Assets/Hotfix/Framework/CoreGame/CustomLogic/CustomLogic.Base/CustomLogic.cs) |
| 节点上下文与黑板访问 | [CustomNode](../Assets/Hotfix/Framework/CoreGame/CustomLogic/CustomLogic.Base/CustomNode/CustomNode.cs) |
| 首次更新与剩余时间 | [BehaviorNodeBase](../Assets/Hotfix/Framework/CoreGame/CustomNodes.Foundation/BehaviorNodes/Base/BehaviorNodeBase.cs) |
| 顺序推进与循环 | [SequenceBhv](../Assets/Hotfix/Framework/CoreGame/CustomNodes.Foundation/BehaviorNodes/Process/SequenceBhv.cs) |
| 有限时间行为 | [FiniteTimeBhv](../Assets/Hotfix/Framework/CoreGame/CustomNodes.Foundation/BehaviorNodes/Base/FiniteTimeBhv.cs) |
| 对象池复用 | [CustomObjectPool](../Assets/Hotfix/Framework/CoreGame/CustomLogic/CustomLogic.Base/Util/CustomObjectPool.cs) |
| 技能帧驱动与移除 | [SysSkillProcess](../Assets/Hotfix/GameLogic/CoreGame/BattleKernel/World/Systems/Behavior/SysSkillProcess.cs)、[SkillProcessComponent](../Assets/Hotfix/GameLogic/CoreGame/BattleKernel/World/LogicWorld/Component/SkillProcessComponent/SkillProcessComponent.cs) |
