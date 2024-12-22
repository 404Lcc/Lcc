using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class SysSkillCD : IExecuteSystem
    {
        private IGroup<LogicEntity> _group;

        public SysSkillCD(ECSWorld world)
        {
            _group = world.LogicContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComSkills, LogicMatcher.ComProperty, LogicMatcher.ComTransform));
        }

        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var comProp = entity.comProperty;
                if (!comProp.isSkillable)
                {
                    continue;
                }

                var skills = entity.comSkills;
                foreach (var skill in skills.skillDict.Values)
                {
                    var skillCd = skill.skillCD;
                    if (skillCd == null)
                        continue;
                    if (!skillCd.isCoolable)
                        continue;

                    if (skillCd.isCooling)
                    {
                        skillCd.cdTimer += Time.deltaTime;
                        if (skillCd.cdTimer >= skillCd.ColdTime)
                        {
                            skillCd.isCooling = false;
                            skillCd.cdTimer = 0;
                        }
                    }
                }
            }
        }
    }
}