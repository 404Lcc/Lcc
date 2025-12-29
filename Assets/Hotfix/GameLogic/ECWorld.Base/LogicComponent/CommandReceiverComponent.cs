using System.Collections.Generic;

namespace LccHotfix
{
    public sealed class CommandReceiverComponent : LogicComponent
    {
        private List<EntityCommand> _receiveQueue = new(4);
        private IEntityCommandDispatcher _dispatcher;

        public List<EntityCommand> ReceiveQueue
        {
            get { return _receiveQueue; }
            set { _receiveQueue = value; }
        }

        public void Initialize(IEntityCommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Dispatch()
        {
            foreach (var cmd in _receiveQueue)
            {
                _dispatcher.HandleEntityCommand(_owner, cmd);
            }

            _receiveQueue.Clear();
        }

        public override void PostInitialize(LogicEntity owner)
        {
            base.PostInitialize(owner);
            _dispatcher.BindOwner(owner);
        }

        public override void DisposeOnRemove()
        {
            _dispatcher.UnBindOwner();
            _dispatcher = null;
            _receiveQueue.Clear();
            base.DisposeOnRemove();
        }
    }


    public partial class LogicEntity
    {
        public CommandReceiverComponent comCommandReceiver
        {
            get { return (CommandReceiverComponent)GetComponent(LogicComponentsLookup.ComCommandReceiver); }
        }

        public bool hasComCommandReceiver
        {
            get { return HasComponent(LogicComponentsLookup.ComCommandReceiver); }
        }

        public void AddComCommandReceiver(IEntityCommandDispatcher dispatcher = null)
        {
            var index = LogicComponentsLookup.ComCommandReceiver;
            var component = (CommandReceiverComponent)CreateComponent(index, typeof(CommandReceiverComponent));
            if (dispatcher == null)
                dispatcher = new EntityCommandSimpleDispatcher();
            component.Initialize(dispatcher);
            AddComponent(index, component);
        }

        public void ReplaceComReceiveCommand(EntityCommand cmd)
        {
            if (!hasComCommandReceiver)
                return;
            var index = LogicComponentsLookup.ComCommandReceiver;
            comCommandReceiver.ReceiveQueue.Add(cmd);
            ReplaceComponent(index, comCommandReceiver);
        }

        public void ReplaceComReceiveCommand(List<EntityCommand> cmds)
        {
            if (!hasComCommandReceiver)
                return;
            var index = LogicComponentsLookup.ComCommandReceiver;
            var cmdList = comCommandReceiver.ReceiveQueue;
            cmdList.InsertRange(cmdList.Count, cmds);
            ReplaceComponent(index, comCommandReceiver);
        }
    }


    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComCommandReceiverIndex = new(typeof(CommandReceiverComponent));
        public static int ComCommandReceiver => _ComCommandReceiverIndex.Index;
    }
}