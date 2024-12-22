using cfg;

namespace LccHotfix
{
    public class SkillData
    {
        public Skill config;
        public int SkillId => config.SkillId;
        public string Name => config.Name;
        public float ColdTime => config.ColdTime;
        public string LogicScript => config.LogicScript;
        public float Range => config.Range;
        //技能权重
        public int Weight => config.Weight == 0 ? 1 : config.Weight;
        public float DamageRate => config.DamageRate;
        //true可以被打断
        public bool Interruptible => config.Interruptible;
        public SkillSpellType SpellType => (SkillSpellType)config.SpellType;
        public void Init(Skill config)
        {
            this.config = config;
        }
    }
}