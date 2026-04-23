using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 静态配置: 状态转换节点
    /// </summary>
    public class StateTransitionNodeCfg : ICustomNodeCfg
    {
        public ICustomNodeCfg ConditionCfg { get; protected set; } //判断条件配置
        public string TrueStateID { get; protected set; } //条件达成 将跳转的stateID
        public string FalseStateID { get; protected set; } //条件不达成 将跳转的stateID
        public float CheckInterval { get; protected set; } = 0; //检查间隔

        public System.Type NodeType()
        {
            return typeof(StateTransitionNode);
        }


        public StateTransitionNodeCfg(ICustomNodeCfg cndCfg, string trueID = null, string falseID = null)
        {
            ConditionCfg = cndCfg;
            TrueStateID = trueID;
            FalseStateID = falseID;
        }
    }


    /// <summary>
    /// 状态转换节点类
    /// </summary>
    public class StateTransitionNode : CustomNode, INeedUpdate
    {
        //条件判断后会转向什么状态，不填就是不转换状态
        public string TrueStateID { get; set; }
        public string FalseStateID { get; set; }
        public float CheckInterval { get; set; }
        public float CheckCDRemian { get; set; }
        public ConditionNodeBase ConditionNode { get; set; }


        public StateTransitionNode()
        {
            TrueStateID = null;
            FalseStateID = null;
            CheckInterval = 0f;
            CheckCDRemian = 0f;
            ConditionNode = null;
        }


        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            base.InitializeNode(cfg, context);

            var theCfg = cfg as StateTransitionNodeCfg;
            ConditionNode = _context.Factory.CreateCustomNode(theCfg.ConditionCfg, context) as ConditionNodeBase;

            TrueStateID = theCfg.TrueStateID;
            FalseStateID = theCfg.FalseStateID;

            // 扩展配置：条件检查间隔，对于某些计算量比较大的条件检查，不能每帧都做
            CheckInterval = theCfg.CheckInterval;
            CheckCDRemian = 0f;
        }

        public override void Destroy()
        {
            _context.Factory.DestroyCustomNode(ConditionNode);
            ConditionNode = null;

            TrueStateID = null;
            FalseStateID = null;
            CheckInterval = 0f;
            CheckCDRemian = 0f;
            base.Destroy();
        }

        public override void Reset()
        {
            base.Reset();
            ConditionNode?.Reset();
        }

        public override void Activate()
        {
            base.Activate();
            ConditionNode?.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            ConditionNode?.Deactivate();
        }

        public override void CollectInterfaceInChildren<T>(ref List<T> interfaceList)
        {
            if (ConditionNode != null)
            {
                TraverseCollectInterface<T>(ref interfaceList, ConditionNode);
            }
        }


        public float Update(float dt)
        {
            if (ConditionNode is INeedUpdate tickCnd)
            {
                tickCnd.Update(dt);
            }

            if (CheckCDRemian > 0)
            {
                CheckCDRemian -= dt;
            }

            return dt;
        }

        public string CheckTransitions()
        {
            if (ConditionNode == null)
            {
                return null;
            }

            if (CheckInterval <= 0f)
            {
                return Inner_CheckTransition();
            }

            if (CheckCDRemian <= 0)
            {
                CheckCDRemian = CheckInterval;
                return Inner_CheckTransition();
            }

            return null;
        }


        private string Inner_CheckTransition()
        {
            string next_state = null;
            if (ConditionNode.IsConditionReached())
            {
                next_state = TrueStateID;
            }
            else
            {
                next_state = FalseStateID;
            }

            return next_state;
        }
    }
}