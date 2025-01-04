using cfg;

namespace LccHotfix
{
    public class SkillProcess : ISkillProcess
    {
        public LogicEntity Owner { get; set; }
        public SkillAbility SkillAbility { get; set; }
        public BTAgent Agent { get; set; }
        public bool IsFinish { get; set; }

        public void Start()
        {
            //关闭移动
            Owner.comProperty.SetSubBool(BoolPropertyType.Moveable, SkillAbility.skillId, false);


            //如果不能被打断 开启不能眩晕
            if (!SkillAbility.skillData.Interruptible)
            {
                Owner.comProperty.SetSubBool(BoolPropertyType.Stunable, SkillAbility.skillId, false);
            }


            Agent.Trigger(BTAction.OnReleaseSkill, this);
        }

        public void Update()
        {
            Agent.Update();
        }
        public void LateUpdate()
        {
            Agent.LateUpdate();
        }

        public bool IsFinished()
        {
            if (SkillAbility.skillData.Interruptible)
            {
                if (Owner.hasComProperty)
                {
                    //打断技能
                    if (Owner.comProperty.isStuning)
                    {
                        return true;
                    }
                }
            }
            return IsFinish;
        }

        public void Dispose()
        {
            //统一进入cd
            int skillId = Agent.LogicId;
            var skills = Owner.comSkills.skillDict;
            if (skills.ContainsKey(skillId))
            {
                var skill = skills[skillId];
                var cd = skill.skillCD;
                cd.EnterCD();
            }


            //清理特效
            var entities = Owner.OwnerContext.GetGroup(LogicMatcher.AllOf(LogicMatcher.ComOwnerEntity, LogicMatcher.ComTag)).GetEntities();
            foreach (var entity in entities)
            {
                if (entity.comOwnerEntity.ownerEntityID == Owner.comID.id)
                {
                    entity.ReplaceComLife(0);
                }
            }
            //开启移动
            Owner.comProperty.SetSubBool(BoolPropertyType.Moveable, SkillAbility.skillId, true);

            //如果不能被打断 结束不能眩晕
            if (!SkillAbility.skillData.Interruptible)
            {
                Owner.comProperty.SetSubBool(BoolPropertyType.Stunable, SkillAbility.skillId, true);
            }


            IsFinish = true;
            Agent.Dispose();
        }
    }
}