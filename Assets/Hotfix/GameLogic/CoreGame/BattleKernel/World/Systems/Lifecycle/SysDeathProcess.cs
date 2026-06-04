using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysDeathProcess : IExecuteSystem, ITearDownSystem
    {
        private readonly LogicWorld _world;
        private readonly IGroup<LogicEntity> _group;

        public SysDeathProcess(ECWorlds worlds)
        {
            _world = worlds.LogicWorld;
            _group = worlds.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComDeath));
        }

        void IExecuteSystem.Execute()
        {
            float dt = BattleTime.GetDeltaTime(_world);
            foreach (var entity in _group.GetEntities())
            {
                var comDeath = entity.comDeath;
                if (comDeath == null)
                    continue;
                var process = comDeath.DeathProcess;
                if (process == null)
                {
                    DoDestroy(entity);
                }
                else
                {
                    process.Update(dt);

                    if (process.CanStop())
                    {
                        DoDestroy(entity);
                    }
                }
            }
        }

        private void DoDestroy(LogicEntity entity)
        {
            // 高级逻辑组件优先析构，外部扩展组件清理由注入服务补充。
            if (entity.hasComSkillProcess)
            {
                entity.RemoveComSkillProcess();
            }

            _world.GetCreationInfo<BattleKernelCreationInfo>().DeathProcessService?.RemoveExternalComponentsBeforeDestroy(entity);

            if (entity.isEnabled)
            {
                entity.Destroy();
            }
            else
            {
                UnityEngine.Debug.LogWarning("SysDeathProcess DoDestroy !entity.isEnabled");
            }
        }

        public void TearDown()
        {
            foreach (var entity in _world.GetEntities())
            {
                DoDestroy(entity);
            }
        }
    }
}
