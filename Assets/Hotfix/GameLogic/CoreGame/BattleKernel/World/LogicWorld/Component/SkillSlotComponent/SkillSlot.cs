using PBConfig;

namespace LccHotfix
{
    /// <summary>
    /// 技能槽
    /// </summary>
    public class SkillSlot : IReference
    {
        public uint Tid => Cfg.Base.Id;
        public TSkillLogic Cfg { get; private set; }
        public float CdTimer { get;  set; } // Cd计时器，剩余多久可以释放

        public void Init(uint skillTid)
        {
            Cfg = PbCfg.GetData<TSkillLogic>(skillTid);
            if (Cfg is not null)
            {
                CdTimer = Cfg.Cd;
            }
        }

        /// <summary>
        /// 技能释放检查
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool Check(LogicEntity self, LogicEntity target)
        {
            if (CdTimer > 0)
            {
                return false;
            }
            if (self.GetTargetingDistance(target) > Cfg.Range)
            {
                return false;
            }
            if (target.hasComAttributes)
            {
                if (!target.GetAttributeBool(AttributeBool.CanBeTargeted))
                {
                    return false;
                }
            }
            return true;
        }

        public void OnRecycle()
        {
            Cfg = null;
            CdTimer = 0;
        }
    }
}