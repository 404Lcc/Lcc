using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    /// <summary> 对即将承受 Buff 的目标做额外校验；返回 true 表示允许施加，false 表示拒绝（如免疫）。 </summary>
    public delegate bool TargetEntityCheckerFuc(LogicEntity targetEntity);

    public interface IEntityBuffCfg
    {
        /// <summary> 非 null 时在 <see cref="BuffCenterComponent.AddNewBuff"/> 中对宿主实体调用；返回 false 则不上 Buff。 </summary>
        TargetEntityCheckerFuc TargetChecker { get; }
    }

    public class BuffCfg : CustomLogicCfg, IEntityBuffCfg
    {
        public override System.Type NodeType()
        {
            return typeof(BuffLogic);
        }

        public TargetEntityCheckerFuc TargetChecker { get; protected set; }

        public BuffCfg WithTargetChecker(TargetEntityCheckerFuc checkFuc)
        {
            TargetChecker = checkFuc;
            return this;
        }

        public BuffCfg(int id, List<ICustomNodeCfg> nodeCfgList, System.Type logicType) : base(id, nodeCfgList, logicType)
        {
        }
    }

    public partial class BuffLogic : EntityCmdLogic, IBuff, IHasOwnerEntity, IHasSourceEntity, IForceEnd, IEntityCommandHandler
    {
        private BuffGenInfo m_buffGenInfo;
        private bool m_isForceEnd;
        private float mDuration;
        private bool mIsForever;


        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            PreInitializeNode(context);
            base.InitializeNode(cfg, context);
            m_isForceEnd = false;
            var durationCfg = GetVar<FloatCfg>(CvKey.CV_BuffDuration);
            mDuration = durationCfg.GetValue(this);
            if (mDuration > 0)
            {
                // 来自全局的debuff持续时间加成
                var durationModifier = GetVar<IBuffDurationModifier>(CvKey.CV_OwnerPlayerInfo);
                if (durationModifier != null)
                {
                    var playerDebuffDurationAdd = durationModifier.DebuffDurationAddRate;
                    if (playerDebuffDurationAdd > 0)
                    {
                        mDuration *= (1 + playerDebuffDurationAdd);
                    }
                }

                // buff生成时的持续时间加成
                if (m_buffGenInfo.DurationAddRate != 0)
                {
                    mDuration *= (1 + m_buffGenInfo.DurationAddRate);
                }
            }

            //CLHelper.LogInfo(this, $"当前的buff持续时间为:{mDuration}");
            mIsForever = mDuration <= 0;
        }

        private void PreInitializeNode(CustomNodeContext context)
        {
            m_buffGenInfo = (BuffGenInfo)context.GenInfo;
        }

        public override void Destroy()
        {
            m_buffGenInfo.Clear();
            m_isForceEnd = false;

            base.Destroy();
        }


        public Entity OwnerEntity
        {
            get { return m_buffGenInfo.Owner; }
        }

        public Entity SourceEntity
        {
            get { return m_buffGenInfo.Sourcer; }
        }


        public int Level { get; set; }
        public int MaxLevel { get; set; }

        public float Duration
        {
            get => mDuration;
        }

        public BuffGenInfo BuffGenInfo
        {
            get { return m_buffGenInfo; }
        }

        public bool IsFinished()
        {
            return m_isForceEnd || CanStop();
        }

        public void UpdateBuff(float dt)
        {
            if (!mIsForever)
            {
                mDuration -= Time.deltaTime;
                if (mDuration <= 0)
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

        public override bool CanStop()
        {
            if (mIsForever)
                return false;

            if (mDuration <= 0)
            {
                return true;
            }

            return false;
        }


        //系统外界信息的通知, 将通知传播给各个CustomNode和其子Node
        private List<IEntityAddBuffNotify> mBuffAddList = new List<IEntityAddBuffNotify>();

        private List<IBuffPreviousRemove> mBuffPreviousRemoveList = new List<IBuffPreviousRemove>();

        private List<IBuffUpgrade> mBuffUpgradeList = new List<IBuffUpgrade>();

        private List<IBuffHandleBeforeDmg> mBuffHandleBeforeDmgList = new List<IBuffHandleBeforeDmg>();


        protected override void ClearInterfaceCache()
        {
            mBuffAddList.Clear();
            mBuffPreviousRemoveList.Clear();
            mBuffUpgradeList.Clear();
            mBuffHandleBeforeDmgList.Clear();
            base.ClearInterfaceCache();
        }

        protected override void CacheInterface(CustomNode node)
        {
            base.CacheInterface(node);
            CustomNode.TraverseCollectInterface(ref mBuffAddList, node);
            CustomNode.TraverseCollectInterface(ref mBuffPreviousRemoveList, node);
            CustomNode.TraverseCollectInterface(ref mBuffUpgradeList, node);
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

            // buff驱散逻辑
            if (HasVar<List<int>>(CvKey.CV_DisperseBuffList))
            {
                var disperseBuffList = GetVar<List<int>>(CvKey.CV_DisperseBuffList);
                foreach (var buffLogicId in disperseBuffList)
                {
                    e.RemoveBuffByID(buffLogicId);
                }
            }
            // buff驱散buff的写法：在buffLogic后面写入初始黑板
            // env.WriteVar<List<int>>(CvKey.CV_DisperseBuffList, new() { buffLogicId });
        }

        public virtual void OnBuffUpgrade(LogicEntity e)
        {
            MaxLevel = m_buffGenInfo.BuffMaxLevel;
            if (m_buffGenInfo.BuffMaxLevel > Level)
            {
                Level++;
                foreach (var c in mBuffUpgradeList)
                {
                    var node = c as ICustomNode;
                    if (node != null && node.IsActive)
                    {
                        c.OnBuffUpgrade(e, this);
                    }
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