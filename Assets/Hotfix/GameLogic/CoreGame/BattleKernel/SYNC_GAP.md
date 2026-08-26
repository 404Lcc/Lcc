# BattleKernel 同步差距清单

> 对比：Lcc `BattleKernel` vs 僵尸 `BattleKernel`  
> 更新：2026-08-26

## 总览

| 项 | Lcc | 僵尸 | 状态 |
|----|-----|------|------|
| LogicDsl `.cs` | 50 | 50 | 组织已对齐 |
| World `.cs` | ~157 | ~175 | 仍有缺口 |
| BattleKernel 合计 | ~207 | ~225 | 约差 18 文件量级 |

---

## LogicDsl

### 已齐
- 目录：`Cfg` / `Configs/LogicConfigBase` / `CustomLogic` / `Keys` / `Nodes`
- `IsMainGameObjectViewReady`、`WaitEntityHasMainGameObjectView`
- `GetOwnerPlayerInfo`、`InGamePlayerInfo` 相关用法
- `SpawnSubobjectDanmaku`、`CreateSkillProcess` 主流程

### 仍差
| 类/文件 | 缺口 |
|---------|------|
| `LogicConfigBase`（Damage） | 扇形 `MakeDamageTo(float radius, float deg)` |
| `CustomNodeSkillExtensions` | `ApplySniperMagazineCritBoost` + 弹药/狙击特性整段 |
| `LogicVarKey` | Keys 略少于僵尸 |

---

## World / Systems（僵尸有、Lcc 无）

### Systems
- `SysBulletTime` / `BulletTimeController`（Lcc 仅占位 `BattleBulletTimeUtility`）
- `SysNavMeshAgent`
- `SysMeleeHitSlow`
- `SysSyncHeroWalkAnimSpeed`
- `SysSyncViewAnimatorSpeed`
- `SysSyncViewInteractiveItem`

### Locomotion 模式
- `LocomotionBounceOnGround`
- `LocomotionFall`
- `LocomotionFlockFlight`
- `LocomotionHelicopter`
- `LocomotionHermite`
- `LocomotionHop`
- `LocomotionParabolaFixedTime`
- `LocomotionSlowTracking`
- `LocomotionTrackingItem`
- `LocomotionTrackingSpirit`

### 组件 / View / 服务
- `TagComponent` / `TagEntityIndex` → **已移植**
- `ViewWrapperPool` / `LogicWorld.ViewWrapperPool` → **已移植**（含 `IViewWrapper.Bind` 接线）
- `ComMonster` / `ComHero` → **已移植**
- `SquadHeroBattleDeadComponent`
- `MapObjectViewLoader`
- `ElectricShockOverlay` / `ElectricShockViewController`
- `IVfxGradeScheduler`
- `LogicEntityFsmVarExtensions` → **已移植**

### HitMaker 命名
两边文件名不同（Lcc 已用 `*HitMaker` 方案），功能已覆盖，不算能力缺口。

---

## PlayerInfo / Gameplay

- `InGamePlayerInfo` 已迁入 `World/Dependence/PlayerInfo`
- 三选一：`PlayerBattleSupplyRecord` / `PlayerBattleSupplyRule` 仍为 stub
- 完整供给/英雄玩法在僵尸 `BattleGameplay`，不在 Kernel

---

## 建议优先级

1. LogicDsl 收尾（扇形伤害 + 狙击弹药）
2. ~~`TagComponent` / `ViewWrapperPool`~~（已完成）
3. 子弹时间 / NavMesh / 近战顿帧
4. 缺的 Locomotion
5. View 同步系统与其它组件
