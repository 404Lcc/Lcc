namespace LccHotfix
{
    /// <summary>
    /// 战斗内核世界。系统注册总序：
    /// Initialize → Rule → Behavior → Move → Battle → SyncView → Late → Debug。
    /// </summary>
    public abstract class BattleKernelWorld : ECWorlds
    {
        protected override void CreateSystems()
        {
            _rootSystem = new ECSystems();

            // 帧管线：先完成创建信息与模式装配，再决策/技能，再位移与结算，最后刷表现并收尾生命周期。
            AddInitializeSystems(_rootSystem);
            AddRuleSystems(_rootSystem);
            AddBehaviorSystems(_rootSystem);
            AddMoveSystems(_rootSystem);
            AddBattleSystems(_rootSystem);
            AddSyncViewSystems(_rootSystem);
            AddLateSystems(_rootSystem);
            AddDebugSystems(_rootSystem);
            
            // 死亡收尾最后做，清理实体与扩展组件。
            _rootSystem.Add(new SysDeathProcess(this));
        }

        /// <summary>
        /// 世界创建期初始化（IInitializeSystem 为主）。
        /// Kernel 先建索引/服务；子类须在此后、进入 Rule 前写入 GameModeGenInfo / ModeLogicService
        /// </summary>
        protected virtual void AddInitializeSystems(ECSystems systems)
        {
            // 一次性内核初始化：日志服务、EntityIndex 等，需全局最先。
            systems.Add(new SysKernelInitialize(this));
        }

        /// <summary>
        /// 规则与帧前置：装配 GameMode、刷新时间、派发指令、更新 Bounds。
        /// 依赖 Initialize 已填好 GameModeGenInfo；必须在 Behavior/Move 前跑完，
        /// 保证本帧模式 dt、子弹时间补偿和包围盒对决策可用。
        /// </summary>
        protected virtual void AddRuleSystems(ECSystems systems)
        {
            // 读取 CreationInfo.GameModeGenInfo，创建 ComUniGameMode；依赖 Initialize 阶段已赋值。
            systems.Add(new SysKernelGameModeInitialize(this));
            // TimeScale / BulletTime 须在 GameModeUpdate 之前：模式 dt 用当帧子弹时间补偿比。
            systems.Add(new SysTimeScale(this));
            systems.Add(new SysBulletTime(this));
            systems.Add(new SysGameModeUpdate(this));
            // 指令派发在模式 tick 后、行为前，让本帧输入进入后续 AI/FSM。
            systems.Add(new SysCommandSend(this));
            // 用当前逻辑坐标刷 Bounds，供索敌/技能距离/碰撞；放在行为前。
            systems.Add(new SysBounds(this));
        }

        /// <summary>
        /// 行为决策：AI → 主状态机 → 技能推进 → 装 View。
        /// 须在 Move 前：技能可能改禁移/Locomotion。
        /// ViewLoader 放技能后：同帧 Spawn 的 ComViewLoader 能立刻装上，Move 也能用到 View.Init 补的 NavMesh。
        /// </summary>
        protected virtual void AddBehaviorSystems(ECSystems systems)
        {
            // AI 产出意图，FSM 切状态/开战技，二者先于技能时间轴。
            systems.Add(new SysAI(this));
            systems.Add(new SysMainFSM(this));
            // 技能逻辑与 CD；需在 FSM 开战技之后、位移之前同帧推进。
            systems.Add(new SysSkillProcess(this));
            systems.Add(new SysSkillSlot(this));
            // 技能末尾加载 View，避免新单位拖到下一帧才有表现/NavMesh。
            systems.Add(new SysViewLoader(this));
        }

        /// <summary>
        /// 位移：行为定稿后、战斗结算前写入逻辑坐标。
        /// 子类可追加 NavMesh 等，仍须保持在 Battle 之前。
        /// </summary>
        protected virtual void AddMoveSystems(ECSystems systems)
        {
            systems.Add(new SysLocomotion(this));
        }

        /// <summary>
        /// 战斗结算：碰撞 → 子物体 → Buff → 伤害 → 相机。
        /// 依赖本帧最终逻辑位置；伤害前先跑碰撞/子弹/Buff，保证命中与属性修正齐全。
        /// 子类可在末尾追加近战减速计时、弹药回复等玩法 tick。
        /// </summary>
        protected virtual void AddBattleSystems(ECSystems systems)
        {
            // 命中检测（含 Physics.SyncTransforms），需在位移之后。
            systems.Add(new SysCollision(this));
            // 子弹/子物体推进，可能继续制造命中，放在伤害前。
            systems.Add(new SysSubobject(this));
            // Buff tick 可能改属性/免疫，须在伤害结算前。
            systems.Add(new SysBuff(this));
            systems.Add(new SysHandleDamage(this));
            // 相机跟随不参与伤害公式，放在结算末尾即可。
            systems.Add(new SysCameraBlender(this));
        }

        /// <summary>
        /// 表现同步：逻辑结算后再刷 Transform/Animator。
        /// 与 Battle 分离，避免表现写回影响当帧逻辑；血条外 UI/插值等由子类在此后追加。
        /// </summary>
        protected virtual void AddSyncViewSystems(ECSystems systems)
        {
            // 先同步位姿，再刷动画状态/速度。
            systems.Add(new SysSyncViewTransform(this));
            systems.Add(new SysSyncViewAnimator(this));
            systems.Add(new SysSyncViewAnimatorSpeed(this));
        }

        /// <summary>
        /// 生命周期收尾：寿命等；血条同步/供给/飘字由子类追加。
        /// 死亡销毁在 CreateSystems 末尾统一挂 SysDeathProcess。
        /// </summary>
        protected virtual void AddLateSystems(ECSystems systems)
        {
            systems.Add(new SysLife(this));
        }

        /// <summary>
        /// 调试系统：不影响正式逻辑，固定挂在管线最末。
        /// </summary>
        protected virtual void AddDebugSystems(ECSystems systems)
        {
        }
    }
}
