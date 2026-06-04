using System.Collections.Generic;

namespace LccHotfix
{
    public sealed class CommandSenderComponent : LogicComponent
    {
        private List<EntityCommand> m_sendQueue = new(4);

        public List<EntityCommand> SendQueue
        {
            get { return m_sendQueue; }
            set { m_sendQueue = value; }
        }

        private IEntityCommandPreHandler m_preHandler;

        public void Initialize(IEntityCommandPreHandler preHandler = null)
        {
            m_preHandler = preHandler;
        }

        // 本地命令派发入口。当前战斗命令不走 CommandReceiver 队列，由 preHandler 直接分发给本实体组件。
        public void PreHandleCommand()
        {
            if (m_preHandler == null)
                return;

            for (int i = 0; i < m_sendQueue.Count; i++)
            {
                var cmd = m_sendQueue[i];
                m_preHandler.PreHandleCommand(_owner, cmd);
            }
        }

        public override void DisposeOnRemove()
        {
            m_preHandler = null;
            m_sendQueue.Clear();
            base.DisposeOnRemove();
        }
    }


    public partial class LogicEntity
    {
        public CommandSenderComponent comCommandSender
        {
            get { return (CommandSenderComponent)GetComponent(LogicComponentsLookup.ComCommandSender); }
        }

        public bool hasComCommandSender
        {
            get { return HasComponent(LogicComponentsLookup.ComCommandSender); }
        }

        public void AddComCommandSender(IEntityCommandPreHandler preHandler = null)
        {
            var index = LogicComponentsLookup.ComCommandSender;
            var component = (CommandSenderComponent)CreateComponent(index, typeof(CommandSenderComponent));
            if (preHandler == null)
            {
                preHandler = new StandaloneEntityCmdPreHandler();
            }
            component.Initialize(preHandler);
            AddComponent(index, component);
        }

        public void SendCmd(EntityCommand cmd)
        {
            if (!hasComCommandSender)
                return;
            var index = LogicComponentsLookup.ComCommandSender;
            comCommandSender.SendQueue.Add(cmd);
            ReplaceComponent(index, comCommandSender);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComCommandSenderIndex = new(typeof(CommandSenderComponent));
        public static int ComCommandSender => _ComCommandSenderIndex.Index;
    }
}
