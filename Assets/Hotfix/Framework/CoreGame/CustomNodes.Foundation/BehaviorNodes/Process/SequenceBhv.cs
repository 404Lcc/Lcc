using System.Collections.Generic;

namespace LccHotfix
{

    public class SequenceBhvCfg : ICustomNodeCfg
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


    }

    /// <summary>
    /// 顺序执行 行为队列包装 
    /// </summary>
    public class SequenceBhv : BehaviorNodeBase, INeedStopCheck
    {
        private List<BehaviorNodeBase> _behaviorSeq = new();
        private int _cfgLoopCnt = 1;
        private float _cfgLoopInterval = 0f;

        private int _curBhvIndex = 0;
        private int _remainLoopCnt = 0;
        private float _remainTimeToNextLoop = -1f;
        private bool AlwaysLoop => _cfgLoopCnt <= -1;
        private SequenceBhvCfg _cfg;



        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            SequenceBhvCfg theCfg = cfg as SequenceBhvCfg;
            _cfg = theCfg;

            _cfgLoopCnt = theCfg.LoopCnt.GetValue(this);
            _cfgLoopInterval = theCfg.LoopInterval.GetValue(this);

            _remainLoopCnt = _cfgLoopCnt;

            _curBhvIndex = 0;
            _behaviorSeq.Clear();

            if (theCfg.SubCfgList == null)
            {
                this.LogError("SequenceBhv:InitializeNode theCfg.SubCfgList == null");
                return;
            }

            for (int i = 0; i < theCfg.SubCfgList.Count; ++i)
            {
                ICustomNodeCfg bhvCfg = theCfg.SubCfgList[i];
                var subbhv = _context.Factory.CreateCustomNode(bhvCfg, context) as BehaviorNodeBase;
                if (!CLHelper.Assert(subbhv != null))
                    continue;
                _behaviorSeq.Add(subbhv);
            }
        }

        protected override void OnBegin()
        {
            base.OnBegin();
            _cfgLoopCnt = _cfg.LoopCnt.GetValue(this);
            _cfgLoopInterval = _cfg.LoopInterval.GetValue(this);

            _remainLoopCnt = _cfgLoopCnt;
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
            _cfgLoopCnt = 1;
            _cfgLoopInterval = 0f;

            _curBhvIndex = 0;
            _remainLoopCnt = 1;
            _remainTimeToNextLoop = -1f;

            for (int i = 0; i < _behaviorSeq.Count; ++i)
            {
                _context.Factory.DestroyCustomNode(_behaviorSeq[i]);
            }

            _behaviorSeq.Clear();
            _cfg = null;

            base.Destroy();
        }


        public override void Reset()
        {
            base.Reset();
            _curBhvIndex = 0;
            _remainLoopCnt = _cfgLoopCnt;
            _remainTimeToNextLoop = -1f;

            for (int i = 0; i < _behaviorSeq.Count; ++i)
            {
                _behaviorSeq[i].Reset();
            }
        }

        protected override float OnUpdate(float dt)
        {
            if (_behaviorSeq == null)
                return dt;
            var nodesSize = _behaviorSeq.Count;
            if (nodesSize == 0)
                return dt;

            var remainLoopCnt = _remainLoopCnt;
            if (AlwaysLoop)
                remainLoopCnt = 1;
            var totalIndexCnt = nodesSize * remainLoopCnt;
            //尽量保证时间精确，过剩的时间片传入后续的更新
            float dt_remain = dt;
            for (int i = 0; i < totalIndexCnt; ++i)
            {
                var curIndex = _curBhvIndex;
                //---------------------- 处理 Loop Interval Beg ----------------------
                var RemainTimeToNextLoop = _remainTimeToNextLoop;
                if (RemainTimeToNextLoop >= 0)
                {
                    if (dt_remain >= RemainTimeToNextLoop)
                    {
                        dt_remain = dt_remain - RemainTimeToNextLoop;
                        _remainTimeToNextLoop = -1;
                        //开启新的循环
                        curIndex = 0;
                        //所有节点Reset
                        foreach (var bhv in _behaviorSeq)
                        {
                            bhv.Reset();
                        }

                        _curBhvIndex = curIndex;
                        ActivateCurBhv();
                    }
                    else
                    {
                        _remainTimeToNextLoop = RemainTimeToNextLoop - dt_remain;
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

                var curBhv = _behaviorSeq[_curBhvIndex];
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
                        _remainLoopCnt = remainLoopCnt;
                        if (remainLoopCnt > 0 || AlwaysLoop)
                        {
                            //设置interval
                            _remainTimeToNextLoop = _cfgLoopInterval;
                        }
                    }
                    else
                    {
                        _curBhvIndex = curIndex;
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
            if (_behaviorSeq == null)
                return;
            for (int i = 0; i < _behaviorSeq.Count; ++i)
            {
                CustomNode.TraverseCollectInterface(ref interfaceList, _behaviorSeq[i]);
            }
        }


        public bool CanStop()
        {
            if (AlwaysLoop)
            {
                return false;
            }

            if (_behaviorSeq.Count == 0)
                return true;
            return _remainLoopCnt <= 0;
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
            if (_curBhvIndex < 0 || _curBhvIndex >= _behaviorSeq.Count)
            {
                return null;
            }

            return _behaviorSeq[_curBhvIndex];
        }

        private void ActivateCurBhv()
        {
            if (_curBhvIndex >= 0 && _curBhvIndex < _behaviorSeq.Count)
            {
                _behaviorSeq[_curBhvIndex].Activate();
            }
        }

        private void DeactivateCurBhv()
        {
            if (_curBhvIndex >= 0 && _curBhvIndex < _behaviorSeq.Count)
            {
                _behaviorSeq[_curBhvIndex].Deactivate();
            }
        }
    }
}