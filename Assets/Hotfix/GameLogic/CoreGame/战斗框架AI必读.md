# 战斗框架 AI 必读

写局内业务前先读本文。目标：知道**文件写在哪一层**，以及**新项目只有框架时如何搭玩法层**。

统一命名空间：`HotUpdate.CoreGame`（分层靠目录约定，当前无 asmdef 强制隔离）。

---

## 1. 硬规则（不可破）

依赖只能单向：

```
BattleGameplay  -->  BattleKernel  -->  BattleFoundation
```

| 规则 | 说明 |
|---|---|
| Kernel **不得**依赖 Gameplay | 禁止引用 Gameplay 类型、节点、常量、`BattleGameplayConfig`、玩法专用 Component |
| Kernel **只**依赖 Foundation | 以及引擎/通用框架层；跨层差异走注入，不走反向引用 |
| Gameplay 可依赖 Kernel + Foundation | 本项目玩法、配置、Service 实现默认都在这里 |
| 新项目必须新建 Gameplay 层 | **禁止**为了快跑把 Mode/技能/关卡塞进 Kernel |

跨层协作方式：Kernel 定义接口 / Policy / 委托槽位（见 `BattleKernelCreationInfo`），由 Gameplay 在 CreationInfo 构造或 `SysGameplayInitialize` 中注入实现。

---

## 2. 三层职责

| 层 | 路径 | 放什么 | 不放什么 |
|---|---|---|---|
| **Foundation** | `BattleFoundation/` | 定点数、AABB/四叉树、MultChange*、随机/SecureValue、`LogicConfigBase` 基类 | 任何战斗实体、技能、玩法逻辑 |
| **Kernel** | `BattleKernel/` | 世界管线、通用 Component/System、通用 Skill/Buff/Subobject/Query 节点、Services **接口** | 英雄/章节特化、项目常量、Supply 等玩法配置 |
| **Gameplay** | `BattleGameplay/`（新项目可改名，职责等同） | `LogicConfig*`、玩法节点、Service **实现**、扩展 System、Dependence | 可复用内核能力不应只放这里又被 Kernel 反向依赖 |

关键入口：

- 内核世界与帧序：`BattleKernel/World/Core/BattleKernelWorld.cs`
- 注入槽位：`BattleKernel/World/Core/BattleKernelCreationInfo.cs`
- 本项目世界：`BattleGameplay/World/Core/ECGameWorld.cs`
- 本项目创建信息：`BattleGameplay/World/Core/ECGameWorldCreationInfo.cs`
- 玩法初始化：`BattleGameplay/World/Systems/Initialize/SysGameplayInitialize.cs`

帧管线（Kernel 固定，Gameplay 只在对应阶段追加 System）：

`Initialize → Rule → Behavior → Move → Battle → SyncView → Late → Debug`，最后 `SysDeathProcess`。

---

## 3. 文件应该写在哪里

| 你要做的事 | 写到哪里 |
|---|---|
| 新技能 / 子弹(Subobject) / Buff **配置** | `BattleGameplay/LogicDsl/Authoring/Configs/{Skill,Subobject,Buff}/LogicConfig*.cs` |
| 仅本玩法用的节点（Bhv/State/Rule） | `BattleGameplay/LogicDsl/Nodes/...` |
| 多玩法可复用的通用节点 / 命中 / 索敌 | `BattleKernel/LogicDsl/Nodes/...`（节点内零 Gameplay 引用） |
| 纯数学 / 属性叠加 / 空间结构 | `BattleFoundation/BaseUtility/...` |
| GameMode / Level / AI / EntityFSM / Supply | `BattleGameplay/LogicDsl/Authoring/Configs/` + 对应 `Nodes/` |
| 项目战斗常量 | `BattleGameplay/LogicDsl/Authoring/BattleGameplayConfig.cs`（**Kernel 禁止引用**） |
| 帧 System 扩展 | 优先 `BattleGameplay/World/Systems/{Initialize,Rule,Move,Battle,Late,...}`，在 `ECGameWorld` 覆写 `Add*Systems` |
| Kernel 需要玩法差异 | Kernel 加 `IXxxService`/Policy/委托 → Gameplay 实现并注入 |
| 配置容器注册 | `BattleGameplay/LogicDsl/Authoring/LogicCfgContainerRegister_Gameplay.cs` |

Logic 容器 Key（Kernel 已定义，Gameplay 对齐使用）：

- `LogicConfigs_GameMode` / `EntityFSM` / `Skill` / `Subobject` / `Level` / `Buff` / `AI` / `PassiveSkill`
- 本项目扩展示例：`LogicConfigs_Supply`（只放在 Gameplay 的 `LogicContainerKey` partial）

---

## 4. 局内业务主链路（最短路径）

技能默认链路：

```
AI / EntityFSM
  → CreateSkillProcess(...)
  → SysSkillProcess（Kernel）
  → Subobject / Buff
  → EvtDamage / EvtHeal
  → SysHandleDamage（Kernel）
```

要点：

- 不要新建“Skill 类硬跑”；用 `LogicConfigSkill*.cs` 的 `AddConfig(LogicID, ...)` 拼节点图。
- 伤害不在技能里直接改血；走命中命令 → 事件 → `SysHandleDamage`。
- 世界由局外创建：`ECGameWorld.CreateWorld(creationInfo)`（新项目则是你的 `XxxGameWorld.CreateWorld`）。

---

## 5. 新项目：只有 Foundation + Kernel、没有 Gameplay

前提：已接入 `BattleFoundation` + `BattleKernel`。目标是搭出**可开战的最小玩法层**，再写业务。  
**不要**把 Mode/技能/关卡写进 Kernel。

### 5.1 目录骨架

```
BattleGameplay/   # 名称可改，职责必须独立
  LogicDsl/
    Authoring/
      BattleXxxConfig.cs              # 项目常量（禁止被 Kernel 引用）
      LogicCfgContainerRegister_Xxx.cs
      Configs/
        GameMode/                     # 至少一个可跑 Mode
        Level/
        EntityFSM/
        AI/
        Skill/
        Subobject/
        Buff/
    Nodes/                            # 玩法节点，按需增加
    Blackboard/Keys/                  # 仅扩展本项目 Key（如 Supply）
  World/
    Core/
      XxxGameWorld.cs                 # : BattleKernelWorld
      XxxGameWorldCreationInfo.cs     # : BattleKernelCreationInfo
    Systems/Initialize/
      SysGameplayInitialize.cs        # 写入 GameModeGenInfo / 注入委托
    Services/                         # 实现 Kernel 的 I* 接口
```

### 5.2 实施步骤（按顺序）

1. **建世界子类** `XxxGameWorld : BattleKernelWorld`  
   - 提供 `CreateWorld(IWorldCreationInfo)`  
   - 覆写 `AddInitializeSystems`：先 `base`，再追加 `SysGameplayInitialize`  
   - 必须在 Kernel Initialize 之后、Rule 之前写入 `GameModeGenInfo` / `ModeLogicService`（供 `SysKernelGameModeInitialize`）

2. **建 CreationInfo 子类** `XxxGameWorldCreationInfo : BattleKernelCreationInfo`  
   在构造函数填齐注入槽（可对照本仓库 `ECGameWorldCreationInfo`）。最小类别：

   | 类别 | 槽位 |
   |---|---|
   | 时间 | `BattleTimeService`、`TimerService` |
   | DSL | `CustomLogicService` |
   | 伤害 | `DamageEventService`、`DamagePolicyService`、`DamagePropertyModifier`、`BuffApplyPolicy` |
   | 单位/索敌 | `UnitOwnerInfoProvider`、`TargetQueryService` |
   | 技能策略 | `SkillFirePolicy`、`SkillLogicOverrideProvider`（可先透传/空实现） |
   | 子弹策略 | `SubobjectSpawnPolicy`、`SubobjectModelOverrideProvider`（可先空实现） |
   | 表现 | `ViewLoadService`、`BattleEffectService`、`BattleAudioService`、`DefaultMainGameObjectViewType` |
   | 日志/反馈 | `BattleLogService`、`BattleFeedbackSink` |
   | 生命周期 | `DeathProcessService`、`MonsterLifecycleSink`、`HealthTraceSink` |
   | 空间 | `CollisionSpaceConfig`、`BattlePlane` |

3. **实现 `SysGameplayInitialize`**  
   - 从 CreationInfo 装配玩家/关卡等 Meta 上下文  
   - 构造 `CustomLogicGenInfo`：`ConfigContainerName = LogicConfigs_GameMode`，`LogicConfigID = ModeLogicID`  
   - 赋值 `GameModeGenInfo`、`ModeLogicService`  
   - 按需挂 `HitBackApplier`、`VfxGradeScheduler`、`DeathProcessService` 等

4. **注册最小 LogicConfig 容器**  
   - 至少一个可跑的 `LogicConfigs_GameMode`  
   - 首局需要的 EntityFSM / Skill / Subobject / Buff / Level / AI  
   - 容器 Key 与 Kernel `LogicContainerKey` 对齐；项目扩展 Key 只放 Gameplay

5. **局外开战入口**  
   - Env/流程里：`XxxGameWorld.CreateWorld(creationInfo)`  
   - 不要直接使用未注入完整的 Kernel 世界

6. **再写业务**  
   - 技能/关卡/玩法 System 全部进 Gameplay  
   - 只有「多项目可复用且无项目类型」才下沉 Kernel

7. **裁剪原则**  
   - 不需要本仓库的 Supply/章节/小队时，可不拷贝对应 Configs/Nodes/Systems  
   - **不要删 Kernel 管线**；缺能力用空实现 Service 占位  
   - 禁止在 Kernel 写 `if (本项目特化)`

### 5.3 最小开战闭环

```
局外填充 XxxGameWorldCreationInfo
  → XxxGameWorld.CreateWorld
  → SysKernelInitialize
  → SysGameplayInitialize（注入 GameModeGenInfo / ModeLogicService）
  → SysKernelGameModeInitialize 等 Rule
  → Behavior / Move / Battle 帧循环
```

本仓库可对照的最小范本（点名参考，勿整目录盲拷）：

- `BattleGameplay/World/Core/ECGameWorld.cs`
- `BattleGameplay/World/Core/ECGameWorldCreationInfo.cs`
- `BattleGameplay/World/Systems/Initialize/SysGameplayInitialize.cs`
- `BattleGameplay/LogicDsl/Authoring/Configs/GameMode/`
- `BattleGameplay/LogicDsl/Authoring/LogicCfgContainerRegister_Gameplay.cs`
- `BattleGameplay/World/Services/`（按接口逐个实现，可先 stub）

---

## 6. 禁止事项

- Kernel 引用 `BattleGameplayConfig`、Gameplay 节点类、Gameplay 专用类型
- 新项目把 Mode / 技能 / 关卡直接写进 Kernel「先跑起来」
- 把项目常量写进 Kernel（应 Gameplay 常量或注入）
- 在 Kernel 里写章节 / 英雄 / 玩法特化分支
- 可复用逻辑只放在 Gameplay，却又让 Kernel 依赖它

---

## 7. 改代码前自检

1. 改动属于 Foundation / Kernel / Gameplay 哪一层？
2. 若工程还没有 Gameplay 层：是否先完成第 5 节骨架，而不是改 Kernel？
3. 若动 Kernel：去掉所有 Gameplay 引用后是否仍能成立？
4. 若需要玩法差异：是否走已有注入槽？没有则先加接口再实现？
5. 新配置是否进了正确的 `LogicConfigs_*` 容器并完成 Register？

---

## 8. 本仓库常见业务落点速查

| 领域 | 配置 | 节点 / 系统 |
|---|---|---|
| Skill | `Authoring/Configs/Skill/` | `Nodes/Skill/`；推进在 Kernel `SysSkillProcess` |
| Subobject | `Authoring/Configs/Subobject/` | `Nodes/Subobject/`；命中核心 Kernel `HandleSubobjectHitCmd` |
| Buff | `Authoring/Configs/Buff/` | `Nodes/Buff/`；Kernel `SysBuff` |
| EntityFSM / AI | `Configs/EntityFSM/`、`Configs/AI/` | `Nodes/EntityFSM/`、`Nodes/AI/`；Kernel `SysAI` / `SysMainFSM` |
| GameMode | `Configs/GameMode/` | `Nodes/GameMode/`；Kernel `SysGameModeUpdate` |
| Level | `Configs/Level/` | `Nodes/Level/` |
| Supply（本项目） | `Configs/Supply/` | `Nodes/Supply/` + `SysSupplyProcess`（Gameplay Late） |
