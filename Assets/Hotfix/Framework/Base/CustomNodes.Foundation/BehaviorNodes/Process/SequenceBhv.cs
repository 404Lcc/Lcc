using System.Collections.Generic;
using System.Xml;

namespace LccHotfix
{
    public static partial class NodeConfigTypeRegistry
    {
        static bool SequenceBhvCfg = Register(typeof(SequenceBhvCfg), NodeCategory.Bhv);
    }

    /// <summary>
    /// 静态配置
    /// </summary>
    public class SequenceBhvCfg : ICustomNodeCfg, IParseFromXml
    {
        public List<ICustomNodeCfg> SubCfgList { get; protected set; } = null;
        public IntCfg LoopCnt { get; protected set; }
        public FloatCfg LoopInterval { get; protected set; }

        public System.Type NodeType()
        {
            return typeof(SequenceBhv);
        }

        public SequenceBhvCfg(List<ICustomNodeCfg> nodeCfgList)
        {
            SubCfgList = nodeCfgList;
            LoopCnt = new IntCfg(1);
            LoopInterval = new FloatCfg(0);
        }

        public SequenceBhvCfg(List<ICustomNodeCfg> nodeCfgList, int loopCnt = 1, float loopInterval = 0f)
        {
            SubCfgList = nodeCfgList;
            LoopCnt = new IntCfg(loopCnt);
            LoopInterval = new FloatCfg(loopInterval);
        }

        public SequenceBhvCfg(List<ICustomNodeCfg> nodeCfgList, IntCfg loopCnt, FloatCfg loopInterval)
        {
            SubCfgList = nodeCfgList;
            LoopCnt = loopCnt;
            LoopInterval = loopInterval;
        }

        public SequenceBhvCfg WithLoopCnt(int loopCnt)
        {
            if (loopCnt == 0)
            {
                LogWrapper.LogError($"SequenceBhvCfg WithLoopCnt loopCnt == 0");
                return this;
            }

            LoopCnt = new IntCfg(loopCnt);
            return this;
        }

        public SequenceBhvCfg WithLoopCnt(string loopCntVar, int defaultCnt = 1)
        {
            LoopCnt = new IntCfg(loopCntVar, defaultCnt);
            return this;
        }

        public SequenceBhvCfg WithLoopInterval(float loopInterval)
        {
            LoopInterval = new FloatCfg(loopInterval);
            return this;
        }

        public SequenceBhvCfg WithLoopInterval(string intervalVar, float defaultInterval = 0f)
        {
            LoopInterval = new FloatCfg(intervalVar, defaultInterval);
            return this;
        }

        public bool ParseFromXml(XmlNode xmlNode)
        {
            var loopCntVar = XmlHelper.GetInt(xmlNode, "LoopCnt", 1);
            LoopCnt = new IntCfg(loopCntVar);
            var loopInterval = XmlHelper.GetFloat(xmlNode, "LoopInterval", 0);
            LoopInterval = new FloatCfg(loopInterval);

            NodeCfgList<ICustomNodeCfg> cfglist = new();
            SubCfgList = cfglist;
            return cfglist.ParseFromXml(xmlNode);
        }
    }

    /// <summary>
    /// 顺序执行 行为队列包装 
    /// </summary>
    public class SequenceBhv : BehaviorNodeBase, INeedStopCheck
    {
        private List<BehaviorNodeBase> mBehaviorSeq = new();
        private int mCfgLoopCnt = 1;
        private float mCfgLoopInterval = 0f;

        private int mCurBhvIndex = 0;
        private int mRemainLoopCnt = 0;
        private float mRemainTimeToNextLoop = -1f;
        private bool AlwaysLoop => mCfgLoopCnt <= -1;
        private SequenceBhvCfg mCfg;



        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            SequenceBhvCfg theCfg = cfg as SequenceBhvCfg;
            mCfg = theCfg;

            mCfgLoopCnt = theCfg.LoopCnt.GetValue(this);
            mCfgLoopInterval = theCfg.LoopInterval.GetValue(this);

            mRemainLoopCnt = mCfgLoopCnt;

            mCurBhvIndex = 0;
            mBehaviorSeq.Clear();

            if (theCfg.SubCfgList == null)
            {
                this.LogError("SequenceBhv:InitializeNode theCfg.SubCfgList == null");
                return;
            }

            for (int i = 0; i < theCfg.SubCfgList.Count; ++i)
            {
                ICustomNodeCfg bhvCfg = theCfg.SubCfgList[i];
                var subbhv = mContext.Factory.CreateCustomNode(bhvCfg, context) as BehaviorNodeBase;
                if (!CLHelper.Assert(subbhv != null))
                    continue;
                mBehaviorSeq.Add(subbhv);
            }
        }

        protected override void OnBegin()
        {
            base.OnBegin();
            mCfgLoopCnt = mCfg.LoopCnt.GetValue(this);
            mCfgLoopInterval = mCfg.LoopInterval.GetValue(this);

            mRemainLoopCnt = mCfgLoopCnt;
        }

        public override void Activate()
        {
            base.Activate();
            ActivateCurBhv();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            DeactivateCurBhv();
        }

        public override void Destroy()
        {
            mCfgLoopCnt = 1;
            mCfgLoopInterval = 0f;

            mCurBhvIndex = 0;
            mRemainLoopCnt = 1;
            mRemainTimeToNextLoop = -1f;

            for (int i = 0; i < mBehaviorSeq.Count; ++i)
            {
                mContext.Factory.DestroyCustomNode(mBehaviorSeq[i]);
            }

            mBehaviorSeq.Clear();
            mCfg = null;

            base.Destroy();
        }


        public override void Reset()
        {
            base.Reset();
            mCurBhvIndex = 0;
            mRemainLoopCnt = mCfgLoopCnt;
            mRemainTimeToNextLoop = -1f;

            for (int i = 0; i < mBehaviorSeq.Count; ++i)
            {
                mBehaviorSeq[i].Reset();
            }
        }

        protected override float OnUpdate(float dt)
        {
            if (mBehaviorSeq == null)
                return dt;
            var nodesSize = mBehaviorSeq.Count;
            if (nodesSize == 0)
                return dt;

            var remainLoopCnt = mRemainLoopCnt;
            if (AlwaysLoop)
                remainLoopCnt = 1;
            var totalIndexCnt = nodesSize * remainLoopCnt;
            //尽量保证时间精确，过剩的时间片传入后续的更新
            float dt_remain = dt;
            for (int i = 0; i < totalIndexCnt; ++i)
            {
                var curIndex = mCurBhvIndex;
                //---------------------- 处理 Loop Interval Beg ----------------------
                var RemainTimeToNextLoop = mRemainTimeToNextLoop;
                if (RemainTimeToNextLoop >= 0)
                {
                    if (dt_remain >= RemainTimeToNextLoop)
                    {
                        dt_remain = dt_remain - RemainTimeToNextLoop;
                        mRemainTimeToNextLoop = -1;
                        //开启新的循环
                        curIndex = 0;
                        //所有节点Reset
                        foreach (var bhv in mBehaviorSeq)
                        {
                            bhv.Reset();
                        }

                        mCurBhvIndex = curIndex;
                        ActivateCurBhv();
                    }
                    else
                    {
                        mRemainTimeToNextLoop = RemainTimeToNextLoop - dt_remain;
                        dt_remain = 0;
                    }
                }

                //---------------------- 处理 Loop Interval End ----------------------
                if (dt_remain <= 0)
                {
                    break;
                }

                if (curIndex >= nodesSize)
                {
                    this.LogError("SequenceBhv:BN_OnUpdate curIndex >= nodesSize");
                    break;
                }

                var curBhv = mBehaviorSeq[mCurBhvIndex];
                //过剩的时间片传入后续的更新
                dt_remain = curBhv.Update(dt_remain);

                //内部的节点如果有不推荐的暴力行为, Update后可能会从内部销毁整个逻辑, 作为通用节点，需要对此防御一手
                if (!IsActive)
                {
                    break;
                }

                if (IsCurBhvEnd(curBhv))
                {
                    //进行下一个行为
                    DeactivateCurBhv();
                    curIndex++;

                    //处理多次循环
                    if (curIndex >= nodesSize)
                    {
                        remainLoopCnt--;
                        mRemainLoopCnt = remainLoopCnt;
                        if (remainLoopCnt > 0 || AlwaysLoop)
                        {
                            //设置interval
                            mRemainTimeToNextLoop = mCfgLoopInterval;
                        }
                    }
                    else
                    {
                        mCurBhvIndex = curIndex;
                        ActivateCurBhv();
                    }
                }
                else
                {
                    return 0f;
                }
            }

            return dt_remain;
        }


        public override void CollectInterfaceInChildren<T>(ref List<T> interfaceList)
        {
            base.CollectInterfaceInChildren<T>(ref interfaceList);
            if (mBehaviorSeq == null)
                return;
            for (int i = 0; i < mBehaviorSeq.Count; ++i)
            {
                CustomNode.TraverseCollectInterface(ref interfaceList, mBehaviorSeq[i]);
            }
        }


        public bool CanStop()
        {
            if (AlwaysLoop)
            {
                return false;
            }

            if (mBehaviorSeq.Count == 0)
                return true;
            return mRemainLoopCnt <= 0;
        }


        private bool IsCurBhvEnd(BehaviorNodeBase curBhv)
        {
            if (curBhv is INeedStopCheck theBhv)
            {
                return theBhv.CanStop();
            }

            return true;
        }

        private BehaviorNodeBase GetCurBhv()
        {
            if (mCurBhvIndex < 0 || mCurBhvIndex >= mBehaviorSeq.Count)
            {
                return null;
            }

            return mBehaviorSeq[mCurBhvIndex];
        }

        private void ActivateCurBhv()
        {
            if (mCurBhvIndex >= 0 && mCurBhvIndex < mBehaviorSeq.Count)
            {
                mBehaviorSeq[mCurBhvIndex].Activate();
            }
        }

        private void DeactivateCurBhv()
        {
            if (mCurBhvIndex >= 0 && mCurBhvIndex < mBehaviorSeq.Count)
            {
                mBehaviorSeq[mCurBhvIndex].Deactivate();
            }
        }
    }
}