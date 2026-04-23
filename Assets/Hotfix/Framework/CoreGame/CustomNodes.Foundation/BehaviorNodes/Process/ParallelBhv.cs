using System.Collections.Generic;


namespace LccHotfix
{
    /// 逻辑节点: ParallelBhv
    /// 节点描述: 并行执行所包含的节点


    public class ParallelBhvCfg : ICustomNodeCfg
    {
        public List<ICustomNodeCfg> SubCfgList { get; set; } = null;

        public System.Type NodeType()
        {
            return typeof(ParallelBhv);
        }

        public ParallelBhvCfg(List<ICustomNodeCfg> nodeCfgList)
        {
            SubCfgList = nodeCfgList;
        }
    }

    /// <summary>
    /// 并行执行 行为组包装
    /// </summary>
    public class ParallelBhv : BehaviorNodeBase, INeedStopCheck
    {
        private List<BehaviorNodeBase> _nodeList = new();

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            _nodeList.Clear();

            var theCfg = cfg as ParallelBhvCfg;
            for (int i = 0; i < theCfg.SubCfgList.Count; ++i)
            {
                ICustomNodeCfg bhvCfg = theCfg.SubCfgList[i];
                var subbhv = _context.Factory.CreateCustomNode(bhvCfg, context) as BehaviorNodeBase;
                AddBhv(subbhv);
            }

            Reset();
        }

        public void AddBhv(BehaviorNodeBase node)
        {
            if (node == null)
            {
                this.LogError("ParallelBhv Add bhv == null");
                return;
            }

            _nodeList.Add(node);
        }

        public override void Activate()
        {
            base.Activate();
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                _nodeList[i].Activate();
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                _nodeList[i].Deactivate();
            }
        }

        public override void Destroy()
        {
            if (_nodeList != null)
            {
                for (int i = 0; i < _nodeList.Count; ++i)
                {
                    _context.Factory.DestroyCustomNode(_nodeList[i]);
                }

                _nodeList.Clear();
            }

            base.Destroy();
        }

        public override void Reset()
        {
            base.Reset();
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                _nodeList[i].Reset();
            }
        }

        protected override float OnUpdate(float dt)
        {
            CLHelper.Assert(_nodeList != null);
            if (_nodeList == null)
                return dt;

            var dt_remain = dt;
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                var bhv = _nodeList[i];
                if (!bhv.IsActive)
                    continue;

                var sub_dt_remain = bhv.Update(dt);
                if (sub_dt_remain < dt_remain)
                {
                    dt_remain = sub_dt_remain;
                }

                var canStopBhv = bhv as INeedStopCheck;
                if (canStopBhv != null && canStopBhv.CanStop())
                {
                    bhv.Deactivate();
                }
            }

            return dt_remain;
        }


        public override void CollectInterfaceInChildren<T>(ref List<T> interfaceList)
        {
            base.CollectInterfaceInChildren<T>(ref interfaceList);
            if (_nodeList == null)
                return;
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                CustomNode.TraverseCollectInterface(ref interfaceList, _nodeList[i]);
            }
        }

        public bool CanStop()
        {
            for (int i = 0; i < _nodeList.Count; ++i)
            {
                INeedStopCheck bhvSC = _nodeList[i] as INeedStopCheck;
                if (bhvSC != null && !bhvSC.CanStop())
                {
                    return false;
                }
            }

            return true;
        }
    }
}