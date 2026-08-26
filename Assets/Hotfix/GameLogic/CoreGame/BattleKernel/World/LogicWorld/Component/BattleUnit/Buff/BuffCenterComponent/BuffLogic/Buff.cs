using Entitas;
using PBConfig;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    /// <summary> 对即将承受 Buff 的目标做额外校验；返回 true 表示允许施加，false 表示拒绝（如免疫）。 </summary>
    public delegate bool TargetEntityCheckerFunc(LogicEntity targetEntity);

    public interface IEntityBuffCfg
    {
        /// <summary> 非 null 时在 <see cref="BuffCenterComponent.AddNewBuff"/> 中对宿主实体调用；返回 false 则不上 Buff。 </summary>
        int BuffTag { get; }
        TargetEntityCheckerFunc TargetChecker { get; }
        IBuffMergeRule MergeRule { get; }
    }

    public class BuffCfg : CustomLogicCfg, IEntityBuffCfg
    {
        public override System.Type NodeType()
        {
            return typeof(BuffLogic);
        }

        public TargetEntityCheckerFunc TargetChecker { get; protected set; } // buff预加载检查器，用于在buff添加时对目标实体做额外的检查（比如免疫），返回true表示允许添加，false表示拒绝添加
        public int BuffTag { get; protected set; } // buff标签，mask格式，用于对buff进行批量管理
        public IBuffMergeRule MergeRule { get; protected set; } // 同类buff合并规则

        public BuffCfg WithTargetChecker(TargetEntityCheckerFunc checkFuc)
        {
            TargetChecker = checkFuc;
            return this;
        }

        public BuffCfg WithBuffTag(int buffTag)
        {
            BuffTag = buffTag;
            return this;
        }

        public BuffCfg WithMergeRule(IBuffMergeRule mergeRule)
        {
            MergeRule = mergeRule;
            return this;
        }

        public BuffCfg(int id, List<ICustomNodeCfg> nodeCfgList, System.Type logicType) : base(id, nodeCfgList, logicType)
        {
        }
    }

    public partial class BuffLogic : EntityCmdLogic, IBuff, IHasOwnerEntity, IHasSourceEntity, IForceEnd, IEntityCommandHandler
    {
        private BuffCfg m_buffCfg;
        private BuffGenInfo m_buffGenInfo;

        private float m_timeLeft;
        private float m_timeTotal;
        private bool m_isForever;
        private bool m_isForceEnd;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            PreInitializeNode(cfg, context);
            base.InitializeNode(cfg, context);
            m_isForceEnd = false;
            m_timeTotal = CalcDuration();
            m_timeLeft = m_timeTotal;
            m_isForever = m_timeLeft <= 0;
            SetVar(CvKey.CV_CurBuff, this);
        }

        private void PreInitializeNode(ICustomNodeCfg cfg, CustomNodeContext context)
        {
            m_buffCfg = (BuffCfg)cfg;
            m_buffGenInfo = (BuffGenInfo)context.GenInfo;
        }

        private float CalcDuration()
        {
            var durationCfg = GetVar<FloatCfg>(CvKey.CV_BuffDuration);
            var duration = durationCfg.GetValue(this);
            if (duration > 0)
            {
                if (m_buffGenInfo.DurationAddSeconds != 0)
                {
                    duration += m_buffGenInfo.DurationAddSeconds;
                }
                if (m_buffGenInfo.SourceDurationAddRate != 0)
                {
                    duration *= (1 + m_buffGenInfo.SourceDurationAddRate);
                }

                // 只有负面 Buff 应用全局减益持续时间加成，避免影响正面 Buff。
                var durationModifier = GetVar<IBuffDurationModifier>(CvKey.CV_OwnerPlayerInfo);
                if (HasBuffTag(BuffTag.Negative) && durationModifier != null)
                {
                    var playerDebuffDurationAdd = durationModifier.DebuffDurationAddRate;
                    if (playerDebuffDurationAdd > 0)
                    {
                        duration *= (1 + playerDebuffDurationAdd);
                    }
                }
                // buff生成时的持续时间加成
                if (m_buffGenInfo.DurationAddRate != 0)
                {
                    duration *= (1 + m_buffGenInfo.DurationAddRate);
                }
            }
            return duration;
        }

        public override void Destroy()
        {
            m_buffCfg = null;
            m_buffGenInfo.Clear();
            m_isForceEnd = false;

            base.Destroy();
        }


        public LogicEntity OwnerEntity
        {
            get { return m_buffGenInfo.Owner; }
        }

        public LogicEntity SourceEntity
        {
            get { return m_buffGenInfo.Sourcer; }
        }


        public int Level { get; set; }
        public int MaxLevel { get; set; }

        public float TimeLeft
        {
            get => m_timeLeft;
        }

        public float TimeTotal
        {
            get => m_timeTotal;
        }

        public BuffCfg BuffCfg
        {
            get { return m_buffCfg; }
        }

        public BuffGenInfo BuffGenInfo
        {
            get { return m_buffGenInfo; }
        }

        public bool HasBuffTag(int buffTag)
        {
            return (BuffCfg.BuffTag & buffTag) == buffTag;
        }

        public bool IsFinished()
        {
            return m_isForceEnd || CanStop();
        }

        public void UpdateBuff(float dt)
        {
            if (!m_isForever)
            {
                m_timeLeft -= dt;
                if (m_timeLeft <= 0)
                    return;
            }

            Update(dt);
        }

        public void Upgrade()
        {
        }


        public void ForceEnd()
        {
            m_isForceEnd = true;
        }

        public void SetTime(float duration)
        {
            m_timeTotal = duration;
            m_timeLeft = duration;
        }

        public void ResetTime()
        {
            m_timeTotal = CalcDuration();
            m_timeLeft = m_timeTotal;
        }

        public override bool CanStop()
        {
            if (m_isForever)
                return false;

            if (m_timeLeft <= 0)
            {
                return true;
            }

            return false;
        }


        //系统外界信息的通知, 将通知传播给各个CustomNode和其子Node
        private List<IEntityAddBuffNotify> mBuffAddList = new List<IEntityAddBuffNotify>();

        private List<IBuffPreviousRemove> mBuffPreviousRemoveList = new List<IBuffPreviousRemove>();

        private List<IBuffStateChanged> mBuffStateChangedList = new List<IBuffStateChanged>();

        private List<IBuffHandleBeforeDmg> mBuffHandleBeforeDmgList = new List<IBuffHandleBeforeDmg>();


        protected override void ClearInterfaceCache()
        {
            mBuffAddList.Clear();
            mBuffPreviousRemoveList.Clear();
            mBuffStateChangedList.Clear();
            mBuffHandleBeforeDmgList.Clear();
            base.ClearInterfaceCache();
        }

        protected override void CacheInterface(CustomNode node)
        {
            base.CacheInterface(node);
            CustomNode.TraverseCollectInterface(ref mBuffAddList, node);
            CustomNode.TraverseCollectInterface(ref mBuffPreviousRemoveList, node);
            CustomNode.TraverseCollectInterface(ref mBuffStateChangedList, node);
            CustomNode.TraverseCollectInterface(ref mBuffHandleBeforeDmgList, node);
        }

        public virtual void OnEntityAddBuff(LogicEntity e)
        {
            Level = m_buffGenInfo.BuffLevel;
            MaxLevel = m_buffGenInfo.BuffMaxLevel;
            foreach (var c in mBuffAddList)
            {
                var node = c as ICustomNode;
                if (node != null && node.IsActive)
                {
                    c.OnEntityAddBuff(e);
                }
            }
        }

        public virtual void OnBuffStateChanged(LogicEntity e)
        {
            foreach (var c in mBuffStateChangedList)
            {
                var node = c as ICustomNode;
                if (node != null && node.IsActive)
                {
                    c.OnBuffStateChanged(e, this);
                }
            }
        }

        public virtual void HandleBeforeDmg(DamageContext context, ref DamageResult result)
        {
            for (int i = 0; i < mBuffHandleBeforeDmgList.Count; i++)
            {
                var c = mBuffHandleBeforeDmgList[i];
                var node = c as ICustomNode;
                if (node != null && node.IsActive)
                {
                    c.HandleBeforeDmg(context, ref result);
                }
            }
        }

        public virtual void OnBuffPreviousRemove()
        {
            Level = 0;
            foreach (var c in mBuffPreviousRemoveList)
            {
                var node = c as ICustomNode;
                if (node != null && node.IsActive)
                {
                    c.OnBuffPreviousRemove();
                }
            }
        }

        public bool IsMaxLevel()
        {
            return Level >= MaxLevel;
        }
    }
}
