using HotUpdate.Framework;
using HotUpdate.Framework.PbCfg;
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

        public void OnRecycle()
        {
            Cfg = null;
            CdTimer = 0;
        }
    }
}