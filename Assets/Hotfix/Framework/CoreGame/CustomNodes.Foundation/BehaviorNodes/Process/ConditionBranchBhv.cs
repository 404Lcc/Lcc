using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 静态配置
    /// </summary>
    public class ConditionBranchBhvCfg : ICustomNodeCfg
    {
        //判断条件配置
        public ICustomNodeCfg CndCfg { get; protected set; }

        //条件达成行为配置
        public ICustomNodeCfg TrueBhvCfg { get; protected set; }

        //条件不达成行为配置
        public ICustomNodeCfg FalseBhvCfg { get; protected set; }
        public bool CheckOnTick { get; protected set; } = true;

        public System.Type NodeType()
        {
            return typeof(ConditionBranchBhv);
        }

        public ConditionBranchBhvCfg()
        {
        }

        public ConditionBranchBhvCfg(ICustomNodeCfg cndCfg, ICustomNodeCfg trueCfg = null, ICustomNodeCfg falseCfg = null)
        {
            CndCfg = cndCfg;
            TrueBhvCfg = trueCfg;
            FalseBhvCfg = falseCfg;
        }
    }

    /// <summary>
    /// 按条件触发的行为节点：条件 + 行为
    /// </summary>
    public class ConditionBranchBhv : BehaviorNodeBase, INeedStopCheck
    {
        protected ConditionNodeBase _condition = null; //激活条件
        protected BehaviorNodeBase _trueBhv = null; //附带行为
        protected BehaviorNodeBase _falseBhv = null; //附带行为

        protected bool? _isConditionReached = null;
        protected bool _checkOnTick = false;


        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            ConditionBranchBhvCfg theCfg = cfg as ConditionBranchBhvCfg;

            _condition = _context.Factory.CreateCustomNode(theCfg.CndCfg, context) as ConditionNodeBase;

            //行为一开始处于非激活状态
            if (theCfg.TrueBhvCfg != null)
            {
                _trueBhv = _context.Factory.CreateCustomNode(theCfg.TrueBhvCfg, context) as BehaviorNodeBase;
                _trueBhv.Deactivate();
            }

            if (theCfg.FalseBhvCfg != null)
            {
                _falseBhv = _context.Factory.CreateCustomNode(theCfg.FalseBhvCfg, context) as BehaviorNodeBase;
                _falseBhv.Deactivate();
            }

            CLHelper.Assert(_condition != null);
            CLHelper.Assert(_trueBhv != null || _falseBhv != null);

            _isConditionReached = null;
            _checkOnTick = theCfg.CheckOnTick;
            if (_condition is INeedUpdate)
            {
                _checkOnTick = true;
            }
        }

        public override void Destroy()
        {
            _context.Factory.DestroyCustomNode(_condition);
            _condition = null;

            _context.Factory.DestroyCustomNode(_trueBhv);
            _trueBhv = null;

            _context.Factory.DestroyCustomNode(_falseBhv);
            _falseBhv = null;

            _isConditionReached = null;
            _checkOnTick = false;
            base.Destroy();
        }

        public override void Reset()
        {
            base.Reset();
            _isConditionReached = null;
            _condition?.Reset();
            _trueBhv?.Reset();
            _falseBhv?.Reset();
        }

        public override void CollectInterfaceInChildren<T>(ref List<T> interfaceList)
        {
            TraverseCollectInterface<T>(ref interfaceList, _condition);
            if (_trueBhv != null)
            {
                TraverseCollectInterface<T>(ref interfaceList, _trueBhv);
            }

            if (_falseBhv != null)
            {
                TraverseCollectInterface<T>(ref interfaceList, _falseBhv);
            }
        }

        public override void Activate()
        {
            base.Activate();
            _condition?.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            _condition?.Deactivate();
            _trueBhv?.Deactivate();
            _falseBhv?.Deactivate();
        }

        public bool CanStop()
        {
            // 1. 条件检查是否能被停止
            INeedStopCheck cndNF = _condition as INeedStopCheck;
            if (cndNF != null && !cndNF.CanStop())
            {
                return false;
            }

            // 2. 条件达成后，行为是否能被停止
            var bhv = _falseBhv;
            if (_isConditionReached == true)
            {
                bhv = _trueBhv;
            }

            if (bhv != null && bhv is INeedStopCheck bhvNSC)
            {
                return bhvNSC.CanStop();
            }

            return true;
        }

        protected override void OnBegin()
        {
            Inner_CheckConditionReached();
        }

        protected override float OnUpdate(float dt)
        {
            if (_condition is INeedUpdate updateCnd)
            {
                updateCnd.Update(dt);
            }

            if (_checkOnTick)
            {
                Inner_CheckConditionReached();
            }

            if (_isConditionReached == true)
            {
                if (_trueBhv != null)
                {
                    dt = _trueBhv.Update(dt);
                }
            }
            else
            {
                if (_falseBhv != null)
                {
                    dt = _falseBhv.Update(dt);
                }
            }

            return dt;
        }

        protected void Inner_CheckConditionReached()
        {
            var isReached = _condition.IsConditionReached();
            bool hasChange = _isConditionReached != isReached;
            _isConditionReached = isReached;
            if (!hasChange)
            {
                return;
            }

            if (_isConditionReached == true)
            {
                _trueBhv?.Activate();
                _falseBhv?.Deactivate();
            }
            else
            {
                _trueBhv?.Deactivate();
                _falseBhv?.Activate();
            }
        }
    }
}