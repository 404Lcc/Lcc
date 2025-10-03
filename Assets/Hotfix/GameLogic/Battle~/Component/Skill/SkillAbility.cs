using UnityEngine;

namespace LccHotfix
{
    public class SkillAbility
    {
        public int skillId;
        public LogicEntity owner;
        public SkillData skillData;
        public SkillCD skillCD;

        public float SkillRange => skillData.Range;
        public bool Interruptible => skillData.Interruptible;

        public SkillAbility(LogicEntity owner, int skillId)
        {
            this.owner = owner;
            this.skillId = skillId;

            skillData = LoadSkillData(skillId);

            skillCD = new SkillCD();
            skillCD.Init(skillData);
        }


        public bool CanSpellSkill()
        {
            //校验cd
            if (skillCD.isCooling)
            {
                return false;
            }

            //TODO校验消耗

            //校验属性
            if (owner.hasComProperty)
            {
                if (!owner.comProperty.isSkillable)
                {
                    return false;
                }
            }

            //如果是不可以中断的，校验是否正在放技能
            if (!Interruptible)
            {
                if (owner.hasComSkillProcess)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanSpellSkill(Vector3 target)
        {
            if (!CanSpellSkill())
            {
                return false;
            }

            //校验距离
            if (!owner.hasComTransform)
                return false;
            var dir = target - owner.comTransform.position;
            var sqrDis = dir.sqrMagnitude;

            var sqrSkillRange = SkillRange * SkillRange;
            if (sqrDis > sqrSkillRange)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检测技能范围
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool CanSkillRange(Vector3 target)
        {

            //校验距离
            if (!owner.hasComTransform)
                return false;
            var dir = target - owner.comTransform.position;
            var sqrDis = dir.sqrMagnitude;

            var sqrSkillRange = SkillRange * SkillRange;
            if (sqrDis > sqrSkillRange)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 无视距离，只检测cd和消耗，释放
        /// </summary>
        public bool SpellSkill(KVContext context = null)
        {
            if (CanSpellSkill())
            {
                CreateSkillProcess(context);
                return true;
            }
            else
            {
                if (context != null)
                {
                    context.Clear();
                }
            }
            return false;
        }

        public void CreateSkillProcess(KVContext context = null)
        {
            //TODO处理消耗

            var entityId = owner.comID.id;
            SkillProcess process = new SkillProcess();
            if (context == null)
            {
                context = new KVContext();
            }

            context.SetObject(KVType.SourceSkillAbility, this);
            context.SetObject(KVType.SkillOwnerEntity, owner);


            BTAgent agent = new BTAgent();
            agent.Init(skillData.BTScript, entityId, skillId, context);

            process.Owner = owner;
            process.SkillAbility = this;
            process.Agent = agent;
            process.Start();


            //打断技能过程
            if (owner.hasComSkillProcess)
            {
                owner.RemoveComSkillProcess();
            }
            owner.AddComSkillProcess(process);
        }

        public SkillData LoadSkillData(int skillId)
        {
            var config = Main.ConfigService.Tables.TBSkill.Get(skillId);
            if (config == null)
            {
                return null;
            }
            SkillData data = new SkillData();
            data.Init(config);
            return data;
        }
    }
}