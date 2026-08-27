using System.Collections.Generic;

namespace LccHotfix
{
    public class EntityCmdLogic : CustomLogic, IEntityCommandHandler
    {
        protected List<IEntityCommandHandler> _entityCmdHandlerList = new();

        public virtual bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd)
        {
            for (int i = 0; i < _entityCmdHandlerList.Count; ++i)
            {
                var theNode = _entityCmdHandlerList[i];
                var node = theNode as ICustomNode;
                if (node != null && node.IsActive)
                {
                    if (theNode.HandleEntityCommand(entity, cmd))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void ClearInterfaceCache()
        {
            _entityCmdHandlerList.Clear();
            base.ClearInterfaceCache();
        }

        protected override void CacheInterface(CustomNode node)
        {
            base.CacheInterface(node);
            TraverseCollectInterface(ref _entityCmdHandlerList, node);
        }
    }

    public class BattleFSM : EntityCmdLogic
    {
        private FSMNode _mainFsmNode; // 主状态机；约定为 Logic 根下直连子 FSMNode，入池前清空

        /// <summary>主状态机节点；约定配置为 Logic 根节点的直接子 FSMNode。</summary>
        public FSMNode MainFsmNode => _mainFsmNode;

        public override void InitializeNode(ICustomNodeCfg cfg, in CustomNodeContext context)
        {
            if (!(context.GenInfo is MainFsmGenInfo))
            {
                KLogger.Log($"纯提醒用, 注意修正：BattleFSM InitializeNode GenInfo is not MainFsmGenInfo, GenInfo={context.GenInfo.GetType()}");
            }
            
            base.InitializeNode(cfg, context);
            if (KLogger.IsDev)
                KLogger.Log($"MainFSM LogicConfigID={context.GenInfo.LogicConfigID}");
        }

        public override void Destroy()
        {
            _mainFsmNode = null;
            base.Destroy();
        }

        protected override void CacheInterface(CustomNode node)
        {
            base.CacheInterface(node);
            if (_mainFsmNode == null && node is FSMNode fsmNode)
            {
                _mainFsmNode = fsmNode;
            }
        }
    }
    

    public class MainFSMComponent : LogicComponent, IEntityCommandHandler
    {
        public BattleFSM Logic { get; private set; }

        public override void DisposeOnRemove()
        {
            if (Logic != null)
            {
                Owner?.OwnerWorld?.GetCreationInfo<BattleKernelCreationInfo>()?.CustomLogicService?.DestroyLogic(Logic);
                Logic = null;
            }

            base.DisposeOnRemove();
        }

        public void Init(BattleFSM fsm)
        {
            Logic = fsm;
        }

        public bool HandleEntityCommand(LogicEntity entity, EntityCommand cmd)
        {
            return Logic.HandleEntityCommand(entity, cmd);
        }
    }

    public partial class LogicEntity
    {
        public MainFSMComponent comFSM
        {
            get { return (MainFSMComponent)GetComponent(LogicComponentsLookup.ComMainFSM); }
        }

        public bool hasComFSM
        {
            get { return HasComponent(LogicComponentsLookup.ComMainFSM); }
        }

        public void AddComFSM(BattleFSM fsm)
        {
            if (fsm == null)
            {
                BattleLogger.LogError("AddComFSM fsm == null");
            }

            var index = LogicComponentsLookup.ComMainFSM;
            var component = (MainFSMComponent)CreateComponent(index, typeof(MainFSMComponent));
            component.Init(fsm);
            AddComponent(index, component);
        }

        public void RemoveComFSM()
        {
            if (hasComFSM)
            {
                RemoveComponent(LogicComponentsLookup.ComMainFSM);
            }
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComMainFSMIndex = new(typeof(MainFSMComponent));
        public static int ComMainFSM => _ComMainFSMIndex.Index;
    }
}
