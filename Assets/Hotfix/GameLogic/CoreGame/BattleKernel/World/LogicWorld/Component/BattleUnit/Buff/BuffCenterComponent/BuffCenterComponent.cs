using System.Collections.Generic;

namespace LccHotfix
{
    public sealed class BuffCenterComponent : LogicComponent, IEntityCommandHandler
    {
        private List<IBuff> m_buffs = new List<IBuff>();
        private List<BuffGenInfo> m_creatingList = new List<BuffGenInfo>();

        private int m_updateDepth;

        public List<IBuff> BuffList
        {
            get { return m_buffs; }
            private set { m_buffs = value; }
        }

        public bool HasBuff(int logicConfigId)
        {
            for (int i = 0; i < m_buffs.Count; i++)
            {
                if (m_buffs[i].BuffGenInfo.LogicConfigID == logicConfigId)
                    return true;
            }
            return false;
        }

        public bool HasBuffAtMaxLevel(int logicConfigId)
        {
            var buff = GetBuff(logicConfigId);
            return buff != null && buff.MaxLevel > 0 && buff.Level >= buff.MaxLevel;
        }

        public bool AddNewBuff(BuffGenInfo genInfo)
        {
            var svc = Owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.CustomLogicService;
            var container = svc?.GetConfigContainer(genInfo.ConfigContainerName);
            var cfg = container?.GetCustomLogicCfg(genInfo.LogicConfigID) as IEntityBuffCfg;
            if (cfg == null)
            {
                BattleLogger.LogWarning("cfg不是 IEntityBuffCfg");
                return false;
            }

            // buff条件预检测
            if (cfg.TargetChecker != null)
            {
                if (!cfg.TargetChecker(Owner))
                {
                    BattleLogger.LogWarning($"buff{genInfo.LogicConfigID} AddNewBuff 时 TargetChecker 检查不通过");
                    return false;
                }
            }

            // buff免疫（配表万分比）
            if (Owner.hasComImmune)
            {
                var buffImmune = Owner.GetBuffTagImmune(cfg.BuffTag) + Owner.GetBuffImmune(genInfo.LogicConfigID);
                if (buffImmune != 0)
                {
                    if (buffImmune >= 10000) // 完全免疫，buff无法上身
                    {
                        if (BattleLogger.IsDebugEnabled)
                            BattleLogger.LogWarning($"buff{genInfo.LogicConfigID} AddNewBuff 时 buff完全免疫 免疫值{buffImmune}");
                        return false;
                    }
                    genInfo.DurationAddRate -= buffImmune / 10000f;
                }
            }

            // 同类buff合并规则
            var mergeRule = cfg.MergeRule ?? BuffMergeRule.Replace; // 默认为Replace规则
            if (!mergeRule.Merge(this, genInfo))
            {
                if (BattleLogger.IsDebugEnabled)
                {
                    BattleLogger.LogDebug($"buff{genInfo.LogicConfigID} AddNewBuff 合并规则 {mergeRule.GetType().Name}");
                }
                return false;
            }

            genInfo.Owner = Owner;

            if (m_updateDepth > 0)
            {
                m_creatingList.Add(genInfo);
                return true;
            }

            CreateAndAttachBuff(genInfo);
            return true;
        }

        /// <summary>
        /// 将创建队列中的 Buff 立刻落到 m_buffs
        /// </summary>
        public void FlushPendingBuffs()
        {
            const int maxPass = 8;
            for (int pass = 0; pass < maxPass && m_creatingList.Count > 0; pass++)
            {
                var pending = m_creatingList;
                m_creatingList = new List<BuffGenInfo>();
                for (int i = 0; i < pending.Count; i++)
                {
                    CreateAndAttachBuff(pending[i]);
                }
            }

            if (m_creatingList.Count > 0)
            {
                BattleLogger.LogWarning($"FlushPendingBuffs 超过安全次数仍有待创建 Buff，count={m_creatingList.Count}");
            }
        }

        private void CreateAndAttachBuff(BuffGenInfo genInfo)
        {
            var svc = Owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.CustomLogicService;
            var buffLogic = svc?.CreateLogic<BuffLogic>(genInfo);
            if (buffLogic == null)
                return;

            m_buffs.Add(buffLogic);
            buffLogic.OnEntityAddBuff(Owner);
        }

        public BuffLogic GetBuff(int logicConfigId)
        {
            BuffLogic last = null;
            for (int i = 0; i < m_buffs.Count; i++)
            {
                var buff = m_buffs[i] as BuffLogic;
                if (buff != null && buff.GenInfo.LogicConfigID == logicConfigId)
                    last = buff;
            }
            return last;
        }

        public int GetBuffSumLevel(int logicConfigId)
        {
            int sum = 0;
            for (int i = 0; i < m_buffs.Count; i++)
            {
                var b = m_buffs[i];
                if (b.BuffGenInfo.LogicConfigID == logicConfigId)
                    sum += b.Level;
            }
            return sum;
        }

        /// <summary>
        /// 移除所有指定LoginID的buff
        /// </summary>
        /// <param name="logicConfigId"></param>
        /// <returns></returns>
        public bool RemoveBuffByID(params int[] logicConfigId)
        {
            if (logicConfigId == null || logicConfigId.Length == 0) return false;
            bool removed = false;
            for (int i = m_buffs.Count - 1; i >= 0; --i)
            {
                var buff = m_buffs[i] as BuffLogic;
                if (buff == null) continue;
                int id = buff.BuffGenInfo.LogicConfigID;
                for (int k = 0; k < logicConfigId.Length; k++)
                {
                    if (logicConfigId[k] == id)
                    {
                        DestroyBuff(buff);
                        m_buffs.RemoveAt(i);
                        removed = true;
                        break;
                    }
                }
            }

            return removed;
        }

        /// <summary>
        /// 移除所有完全包含指定BuffTag的buff
        /// </summary>
        /// <param name="buffTag"></param>
        /// <returns></returns>
        public bool RemoveBuffByBuffTag(params int[] buffTag)
        {
            if (buffTag == null || buffTag.Length == 0) return false;
            bool removed = false;
            for (int i = m_buffs.Count - 1; i >= 0; --i)
            {
                var buff = m_buffs[i] as BuffLogic;
                if (buff == null) continue;
                for (int k = 0; k < buffTag.Length; k++)
                {
                    if (buff.HasBuffTag(buffTag[k]))
                    {
                        DestroyBuff(buff);
                        m_buffs.RemoveAt(i);
                        removed = true;
                        break;
                    }
                }
            }

            return removed;
        }

        /// <summary>
        /// 按index移除buff，内部使用
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool RemoveBuffByIndex(int index)
        {
            if (index < 0 || index >= m_buffs.Count)
                return false;
            var buff = m_buffs[index] as BuffLogic;
            DestroyBuff(buff);
            m_buffs.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 移除身上全部 Buff，含待创建队列。
        /// </summary>
        public void RemoveAllBuffs()
        {
            for (int i = m_buffs.Count - 1; i >= 0; --i)
            {
                var buff = m_buffs[i] as BuffLogic;
                DestroyBuff(buff);
                m_buffs.RemoveAt(i);
            }

            m_creatingList.Clear();
        }
        
        private void DestroyBuff(BuffLogic buff)
        {
            buff.OnBuffPreviousRemove();
            Owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.CustomLogicService?.DestroyLogic(buff);
        }

        public void HandleBeforeDmg(DamageContext context, ref DamageResult result)
        {
            for (int i = 0; i < m_buffs.Count; i++)
            {
                var buff = m_buffs[i] as BuffLogic;
                if (buff == null)
                    continue;
                buff.HandleBeforeDmg(context, ref result);
            }
        }

        public void Update(float dt)
        {
            m_updateDepth++;
            try
            {
                FlushPendingBuffs();

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

                FlushPendingBuffs();
            }
            finally
            {
                m_updateDepth--;
            }
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
                Owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.CustomLogicService?.DestroyLogic(buff);
            }

            m_creatingList.Clear();
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

        public bool HasBuff(int buffLogicId)
        {
            if (!hasComBuffCenter)
            {
                return false;
            }
            return comBuffCenter.HasBuff(buffLogicId);
        }
    }

    public static class LogicEntityExtend_BuffCenter
    {
        static public void SetBuffCenter(this LogicEntity e)
        {
            if (e.hasComBuffCenter)
            {
                BattleLogger.LogError("SetBuffCenter Error, has buffCenter already");
                return;
            }

            var index = LogicComponentsLookup.ComBuffCenter;
            var component = (BuffCenterComponent)e.CreateComponent(index, typeof(BuffCenterComponent));
            e.AddComponent(index, component);
        }

        static public void AddBuff(this LogicEntity e, BuffGenInfo genInfo)
        {
            if (!e.hasComBuffCenter)
            {
                if (BattleLogger.IsDebugEnabled)
                    BattleLogger.LogDebug("AddBuff unusual, cant find buffCenter");
                e.SetBuffCenter();
            }

            e.comBuffCenter.AddNewBuff(genInfo);
        }

        static public void RemoveBuffByID(this LogicEntity e, int logicConfigId)
        {
            if (!e.hasComBuffCenter)
                return;
            e.comBuffCenter.RemoveBuffByID(logicConfigId);
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComBuffCenterIndex = new(typeof(BuffCenterComponent));
        public static int ComBuffCenter => _ComBuffCenterIndex.Index;
    }
}
