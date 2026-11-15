# 战斗系统与 DSL

[返回首页](../README.md) · [架构说明](架构说明.md)

战斗文档按主题拆分。建议先了解当前实现边界，再沿下面的顺序阅读；如果已经熟悉框架，可以直接看贯穿案例。

| 阅读顺序 | 文档 | 主要内容 |
| --- | --- | --- |
| 1 | [战斗世界与帧循环](battle/战斗世界与帧循环.md) | 分层、实体组件、服务注入与 Update/FixedUpdate |
| 2 | [DSL 配置与执行](battle/DSL配置与执行.md) | 配置容器、运行时节点、黑板、组合与回收 |
| 3 | [技能、Buff 与扩展](battle/技能Buff与扩展.md) | 战斗接入协议、伤害处理与扩展步骤 |
| 4 | [施法到清理案例](battle/施法到清理案例.md) | 前摇、发射、命中、伤害，以及三条清理路径 |
| 5 | [玩法接入示例](battle/玩法接入示例.md) | 真实世界子类、模式初始化、容器注册与服务注入 |
| 6 | [技能与 Buff 示例](battle/技能与Buff示例.md) | 动态前摇、连射、穿透、灼烧 DOT 与叠层规则 |

前四篇解释当前 Lcc 的机制与战斗案例；后两篇基于另一份使用此框架的项目源码提炼，展示 Gameplay 接入与技能配置的用法。两份工程的接口与业务实现存在差异，代码片段尚未在 Unity 中编译运行。

## 向面试官讲解的路线

建议先用 5～8 分钟讲清一条主线，再根据追问进入节点或系统源码。下面是讲解提纲，不代替具体项目中的个人职责说明。

| 顺序 | 要讲清楚的问题 | 用什么支撑 |
| --- | --- | --- |
| 1. 问题与目标 | 技能、子物体和 Buff 有大量时序、条件与复用需求，如何组织这些变化 | 从前摇、连射、DOT 三个具体需求切入 |
| 2. 系统边界 | 世界/System 负责驱动，DSL 负责逻辑组合，Gameplay 提供具体实现 | [世界与帧循环](battle/战斗世界与帧循环.md) 的分层与更新入口 |
| 3. 核心机制 | C# 配置如何装配为运行时节点，黑板如何传参，谁推进与回收 | [DSL 配置与执行](battle/DSL配置与执行.md) 中的工厂、上下文与生命周期 |
| 4. 战斗闭环 | 从一次施法追到发射、命中、伤害，解释三种对象为何分别清理 | [施法到清理案例](battle/施法到清理案例.md) |
| 5. 扩展与取舍 | 新增连射或 DOT 时改什么，时间、来源、合并和池复用有哪些约束 | [技能与 Buff 示例](battle/技能与Buff示例.md) |
| 6. 项目接入 | 模式上下文、服务注入和配置注册怎样让内核进入具体玩法 | [玩法接入示例](battle/玩法接入示例.md) |

### 值得展开的设计取舍

- **配置与实例分开**：节点组合可以复用，每次执行的目标、计时和进度属于实例；代价是需要管理配置 ID、类型与装配错误。
- **System 与 DSL 分工**：批量筛选和驱动留在 System，时序和行为组合放进节点；避免把所有战斗更新逻辑都塞进某个技能类。
- **强类型上下文与黑板并用**：世界、所属单位等基础依赖由 GenInfo 表达，可变参数在黑板传递；黑板 Key 和泛型类型仍需要约定与检查。
- **对象池与生命周期**：复用对象减少重复创建，但必须处理 Reset、Destroy、引用清理与所有权；不能把使用对象池直接等同于“零 GC”。
- **可组合与可预测性**：Seq 的剩余时间片能衔接阶段，但碰撞固定更新、命令队列和伤害服务会影响整体时序，不能宣称整条战斗链路同帧同步完成。

讲“做了什么”时，区分自己实现的框架机制、自己扩展的玩法，以及已有库提供的能力。效果优先用可展示的配置变化、执行链和验证记录支撑；性能、覆盖率或开发效率提升只使用已有测量，不从架构设计推导数字。

当前文档已对照源码，但没有本轮编译、实机效果或性能测量。面试时可以据此讲实现和取舍，运行效果与收益应结合项目已有证据说明。

## 先明确当前代码的边界

仓库中存在可阅读的战斗内核、通用节点、黑板、工厂与技能/Buff 接入代码，但当前 `BattleGameplay` 下仅有一个空的配置注册实现。旧说明中提到的 `ECGameWorld`、`SysGameplayInitialize` 和具体技能配置目录目前不存在。因此，这组文档解释现有机制和需要补齐的接入步骤，不承诺当前仓库可以直接进入完整战斗。

现有 [战斗框架 AI 必读](../Assets/Hotfix/GameLogic/CoreGame/战斗框架AI必读.md) 可作为分层约束参考，其中的旧命名空间与部分路径应以实际源码为准。

可以先记住下面的分工：

| 部分 | 负责什么 | 典型入口 |
| --- | --- | --- |
| 战斗世界 | 创建上下文、装配系统、驱动系统执行 | `ECWorlds`、`BattleKernelWorld` |
| Entity / Component | 标识战斗对象，保存其状态及逻辑实例 | `LogicEntity`、`SkillProcessComponent` |
| System | 找出符合条件的实体，推进逻辑或处理状态 | `SysSkillProcess`、`SysBuff` |
| DSL 配置 | 描述一个逻辑由哪些节点、参数和组合关系构成 | `LogicConfigBase`、`CustomLogicCfg` |
| DSL 运行时 | 保存每次执行的状态，响应更新、命令与查询 | `CustomLogic`、`CustomNode` |
| 玩法层 | 提供具体配置、服务实现及世界接入 | `BattleGameplay`，当前尚待补齐 |

System 决定谁在什么时候被推进，DSL 决定被推进的那段逻辑如何执行。二者在组件持有的运行时逻辑实例处连接起来。

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
