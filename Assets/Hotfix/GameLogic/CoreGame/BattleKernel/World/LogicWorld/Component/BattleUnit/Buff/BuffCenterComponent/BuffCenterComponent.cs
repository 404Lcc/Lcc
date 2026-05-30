using System.Collections.Generic;
using System.Linq;

namespace LccHotfix
{
    public sealed class BuffCenterComponent : LogicComponent, IEntityCommandHandler
    {
        private List<IBuff> m_buffs = new List<IBuff>();
        private List<BuffGenInfo> m_creatingList = new List<BuffGenInfo>();
        private List<int> m_removingList = new List<int>();

        public List<IBuff> BuffList
        {
            get { return m_buffs; }
            private set { m_buffs = value; }
        }

        public bool HasBuff(int id)
        {
            return m_buffs.Exists(b => b.BuffGenInfo.BuffLogicID == id);
        }

        public bool AddNewBuff(BuffGenInfo genInfo)
        {
            //TODO: 基础buff叠加预检测

            var svc = Owner?.OwnerWorld?.CustomLogicService;
            var container = svc?.GetConfigContainer(genInfo.ConfigContainerName);
            var cfg = container?.GetCustomLogicCfg(genInfo.LogicConfigID);
            if (cfg is IEntityBuffCfg entityBuffCfg && entityBuffCfg.TargetChecker != null)
            {
                if (!entityBuffCfg.TargetChecker(Owner))
                {
                    BattleLog.Warning("AddNewBuff 时才发现 TargetChecker 检查不通过");
                    return false;
                }
            }

            // buff免疫（配表万分比）
            var buffImmune = Owner.GetBuffImmune(genInfo.LogicConfigID);
            if (buffImmune != 0)
            {
                if (buffImmune >= 10000) // 完全免疫，buff无法上身
                {
                    return false;
                }

                genInfo.DurationAddRate -= buffImmune / 10000f;
            }

            if (HasBuff(genInfo.BuffLogicID)) //临时Demo，先放个无相同buff规则
            {
                var buff = GetBuff(genInfo.BuffLogicID);
                if (buff != null)
                {
                    UpgradeBuff(buff);
                }

                return false;
            }

            genInfo.Owner = Owner;
            m_creatingList.Add(genInfo);
            return true;
        }

        public BuffLogic GetBuff(int buffId)
        {
            for (int i = m_buffs.Count - 1; i >= 0; --i)
            {
                var buff = m_buffs[i] as BuffLogic;
                if (buff == null)
                    continue;
                if (buff.GenInfo.LogicConfigID == buffId)
                {
                    return buff;
                }
            }

            return null;
        }

        public int GetBuffSumLevel(int buffId)
        {
            return m_buffs.Where(buff => buff.BuffGenInfo.BuffLogicID == buffId).Sum(buff => buff.Level);
        }

        public bool RemoveBuffByID(int buffID)
        {
            for (int i = m_buffs.Count - 1; i >= 0; --i)
            {
                var buff = m_buffs[i] as BuffLogic;
                if (buff.GenInfo.LogicConfigID == buffID)
                {
                    DestroyBuff(buff);
                    m_buffs.RemoveAt(i);
                }
            }

            return false;
        }

        private void DestroyBuff(BuffLogic buff)
        {
            buff.OnBuffPreviousRemove();
            Owner?.OwnerWorld?.CustomLogicService?.DestroyLogic(buff);
        }

        private void UpgradeBuff(BuffLogic buff)
        {
            buff.Upgrade();
            buff.OnBuffUpgrade(Owner);
        }

        public void HandleBeforeDmg()
        {
            for (int i = 0; i < m_buffs.Count; i++)
            {
                var buff = m_buffs[i] as BuffLogic;
            }
        }

        public void Update(float dt)
        {
            if (m_creatingList.Count > 0)
            {
                var svc = Owner?.OwnerWorld?.CustomLogicService;
                foreach (var genInfo in m_creatingList)
                {
                    var buffLogic = svc?.CreateLogic<BuffLogic>(genInfo);
                    if (buffLogic == null)
                    {
                        continue;
                    }

                    m_buffs.Add(buffLogic);
                    buffLogic.OnEntityAddBuff(Owner);
                }

                m_creatingList.Clear();
            }

            if (m_buffs == null)
                return;

            for (int i = m_buffs.Count - 1; i >= 0; --i)
            {
                var buff = m_buffs[i] as BuffLogic;

                buff.UpdateBuff(dt);

                if (buff.IsFinished())
                {
                    DestroyBuff(buff);
                    m_buffs.RemoveAt(i);
                }
            }
        }

        public void PreInitialize(IBuffMergeRule mergeRule)
        {
            //TODO： 定义基础的Buff叠加规则
        }

        public bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd)
        {
            foreach (var buff in m_buffs)
            {
                if (buff is IEntityCommandHandler commandHandler)
                {
                    if (commandHandler.HandleEntityCommand(entity, cmd))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void DisposeOnRemove()
        {
            for (int i = 0; i < m_buffs.Count; ++i)
            {
                var buff = m_buffs[i] as BuffLogic;
                buff.OnBuffPreviousRemove();
                Owner?.OwnerWorld?.CustomLogicService?.DestroyLogic(buff);
            }

            m_creatingList.Clear();
            m_removingList.Clear();
            m_buffs.Clear();
            base.DisposeOnRemove();
        }
    }


    public partial class LogicEntity
    {
        public BuffCenterComponent comBuffCenter
        {
            get { return (BuffCenterComponent)GetComponent(LogicComponentsLookup.ComBuffCenter); }
        }

        public bool hasComBuffCenter
        {
            get { return HasComponent(LogicComponentsLookup.ComBuffCenter); }
        }

        public void RemoveComBuffCenter()
        {
            RemoveComponent(LogicComponentsLookup.ComBuffCenter);
        }
    }

    public static class LogicEntityExtend_BuffCenter
    {
        static public void SetBuffCenter(this LogicEntity e, IBuffMergeRule mergeRule)
        {
            if (e.hasComBuffCenter)
            {
                BattleLog.Error("SetBuffCenter Error, has buffCenter already");
                return;
            }

            var index = LogicComponentsLookup.ComBuffCenter;
            var component = (BuffCenterComponent)e.CreateComponent(index, typeof(BuffCenterComponent));
            component.PreInitialize(mergeRule);
            e.AddComponent(index, component);
        }

        static public void AddBuff(this LogicEntity e, BuffGenInfo genInfo)
        {
            if (!e.hasComBuffCenter)
            {
                if (BattleLog.IsDebugEnabled)
                    BattleLog.Debug("AddBuff unusual, cant find buffCenter");
                e.SetBuffCenter(null);
            }

            e.comBuffCenter.AddNewBuff(genInfo);
        }

        static public void RemoveBuffByID(this LogicEntity e, int buffID)
        {
            e.comBuffCenter.RemoveBuffByID(buffID);
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComBuffCenterIndex = new(typeof(BuffCenterComponent));
        public static int ComBuffCenter => _ComBuffCenterIndex.Index;
    }
}