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
                    theNode.HandleEntityCommand(entity, cmd);
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
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComMainFSMIndex = new(typeof(MainFSMComponent));
        public static int ComMainFSM => _ComMainFSMIndex.Index;
    }
}