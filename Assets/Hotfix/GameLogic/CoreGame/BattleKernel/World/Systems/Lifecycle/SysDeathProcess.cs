using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysDeathProcess : IExecuteSystem, ITearDownSystem
    {
        private readonly LogicWorld _world;
        private readonly MetaWorld _metaWorld;
        private readonly IGroup<LogicEntity> _group;

        private readonly List<LogicEntity> _entityBuffer = new(256);

        public SysDeathProcess(ECWorlds worlds)
        {
            _world = worlds.LogicWorld;
            _metaWorld = worlds.MetaWorld;
            _group = worlds.LogicWorld.GetGroup(LogicMatcher.AllOf(LogicComponentsLookup.ComDeath));
        }

        void IExecuteSystem.Execute()
        {
            float dt = BattleTime.GetDeltaTime(_world);
            var buffer = _group.GetEntities(_entityBuffer);
            foreach (var entity in buffer)
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
                    var entityDt = dt * BattleBulletTimeUtility.GetCompensateRatio(entity, _metaWorld);
                    process.Update(entityDt);

                    if (process.CanStop())
                    {
                        DoDestroy(entity);
                    }
                }
            }
        }

        private void DoDestroy(LogicEntity entity)
        {
            // -------------------高级逻辑组件，手动优先按序Remove -------------------
            // 这里是纯业务功能安排（高级逻辑在底层组件尚存时提前析构，高级逻辑的暴力打断和结束，可以处理的更简单、更统一）
            // 类似现实社会: “先停职、再开除、再剥夺各项基本权利、最后枪毙”,  只要都是“先停职 再砍头”，就能确保手上工作有正确的处理
            // 但组件卸载不可以有严苛的顺序依赖！！！！ 不提前按序Remove也不能出现崩溃，致命错误 ！！！
            // 比如流程上意外出现：先车裂，然后才剥夺政治权利，原则上也是合法的行为，不可出现系统性崩溃 
            
            if (entity.hasComSkillProcess)
            {
                entity.RemoveComSkillProcess();
            }
            if (entity.hasComSubobject)
            {
                entity.comSubobject.DestroyLogic();
                entity.RemoveComSubobject();
            }
            // if (entity.hasComBuffCenter)
            // {
            //     entity.RemoveComBuffCenter();
            // }

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
