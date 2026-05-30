namespace LccHotfix
{
    public abstract class BattleKernelWorld : ECWorlds
    {
        protected override void CreateSystems()
        {
            _rootSystem = new ECSystems();

            AddKernelInitializeSystems(_rootSystem);
            AddExternalInitializeSystems(_rootSystem);
            AddKernelGameModeInitializeSystems(_rootSystem);
            AddKernelRuleSystems(_rootSystem);
            AddExternalDebugSystems(_rootSystem);
            AddKernelTimeAndInputSystems(_rootSystem);
            AddKernelRuntimeSystems(_rootSystem);
            AddExternalMidSystems(_rootSystem);
            AddKernelViewSystems(_rootSystem);
            AddKernelLifeSystems(_rootSystem);
            AddExternalLateSystems(_rootSystem);
        }

        protected virtual void AddKernelInitializeSystems(ECSystems systems)
        {
            systems.Add(new SysKernelInitialize(this));
        }

        protected virtual void AddExternalInitializeSystems(ECSystems systems)
        {
        }

        protected virtual void AddKernelGameModeInitializeSystems(ECSystems systems)
        {
            systems.Add(new SysKernelGameModeInitialize(this));
        }

        protected virtual void AddKernelRuleSystems(ECSystems systems)
        {
            systems.Add(new SysGameModeUpdate(this));
        }

        protected virtual void AddExternalDebugSystems(ECSystems systems)
        {
        }

        protected virtual void AddKernelTimeAndInputSystems(ECSystems systems)
        {
            systems.Add(new SysTimeScale(this));
            systems.Add(new SysCommandSend(this));
        }

        protected virtual void AddKernelRuntimeSystems(ECSystems systems)
        {
            systems.Add(new SysBounds(this));
            systems.Add(new SysAI(this));
            systems.Add(new SysMainFSM(this));
            systems.Add(new SysSkillProcess(this));
            systems.Add(new SysSkillSlot(this));
            systems.Add(new SysLocomotion(this));
            systems.Add(new SysCollision(this));
            systems.Add(new SysCameraBlender(this));
            systems.Add(new SysSubobject(this));
            systems.Add(new SysBuff(this));
            systems.Add(new SysHandleDamage(this));
        }

        protected virtual void AddExternalMidSystems(ECSystems systems)
        {
        }

        protected virtual void AddKernelViewSystems(ECSystems systems)
        {
            systems.Add(new SysViewLoader(this));
            systems.Add(new SysSyncViewTransform(this));
            systems.Add(new SysSyncViewAnimator(this));
        }

        protected virtual void AddKernelLifeSystems(ECSystems systems)
        {
            systems.Add(new SysLife(this));
            systems.Add(new SysSyncViewHp(this));
            systems.Add(new SysDeathProcess(this));
        }

        protected virtual void AddExternalLateSystems(ECSystems systems)
        {
        }
    }
}
