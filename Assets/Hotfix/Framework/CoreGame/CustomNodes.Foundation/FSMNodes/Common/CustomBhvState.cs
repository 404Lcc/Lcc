using System.Collections.Generic;

namespace LccHotfix
{
    //静态配置
    public class CustomBhvStateCfg : StateNodeCfg
    {
        public ICustomNodeCfg Bhv { get; set; }
        public ICustomNodeCfg ExitBhv { get; set; }

        public override System.Type NodeType()
        {
            if (StateClass.IsSubclassOf(typeof(CustomBhvState)))
            {
                return StateClass;
            }

            return typeof(CustomBhvState);
        }
    }


    /// <summary>
    /// 运行时节点: 基类状态
    /// </summary>
    public class CustomBhvState : StateNode
    {
        protected BehaviorNodeBase _bhv = null;
        protected BehaviorNodeBase _exitBhv = null;
        protected bool _isStateBhvEnd = false;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);
            var theCfg = cfg as CustomBhvStateCfg;
            if (theCfg == null)
            {
                return;
            }

            if (theCfg.Bhv != null)
            {
                _bhv = _context.Factory.CreateCustomNode(theCfg.Bhv, context) as BehaviorNodeBase;
                _bhv.Deactivate();
            }

            if (theCfg.ExitBhv != null)
            {
                _exitBhv = _context.Factory.CreateCustomNode(theCfg.ExitBhv, context) as BehaviorNodeBase;
                _exitBhv.Deactivate();
            }
        }

        public override void Destroy()
        {
            _context.Factory.DestroyCustomNode(_bhv);
            _bhv = null;

            _context.Factory.DestroyCustomNode(_exitBhv);
            _exitBhv = null;

            _isStateBhvEnd = false;
            base.Destroy();
        }

        public override void Activate()
        {
            base.Activate();
            _bhv?.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            _bhv?.Deactivate();
        }

        public override void CollectInterfaceInChildren<T>(ref List<T> interfaceList)
        {
            base.CollectInterfaceInChildren(ref interfaceList);
            if (_bhv != null)
            {
                TraverseCollectInterface<T>(ref interfaceList, _bhv);
            }
            // if (mExitBhv != null)    //结束行为，激活状态只在瞬间。无需处理外部输入
            // {
            //     TraverseCollectInterface<T>(ref interfaceList, mExitBhv);    
            // }
        }

        public override float Update(float dt)
        {
            base.Update(dt);
            if (_bhv != null)
            {
                _bhv.Update(dt);
                if (!_isStateBhvEnd && _bhv.IsNodeCanStop())
                {
                    _isStateBhvEnd = true;
                    OnNodeLogicEnd();
                }
            }
            else
            {
                _isStateBhvEnd = true;
            }

            return dt;
        }


        public override void Enter()
        {
            base.Enter();

            if (_bhv != null)
            {
                _bhv.Reset();
                _bhv.Activate();
            }

            _exitBhv?.Reset();
            _isStateBhvEnd = false;
        }

        public override void Exit()
        {
            if (_bhv != null)
            {
                _bhv.Deactivate();
            }

            if (_exitBhv != null)
            {
                //ExitNode中只能执行能够立即完成的节点，原则上不该有持续性节点
                _exitBhv.Activate();
                _exitBhv.Update(1000f);
                _exitBhv.Deactivate();
            }

            base.Exit();
        }

        public override string CheckTransitions()
        {
            if (!_isStateBhvEnd)
            {
                return null;
            }

            return base.CheckTransitions();
        }

        protected virtual void OnNodeLogicEnd()
        {
        }
    }
}