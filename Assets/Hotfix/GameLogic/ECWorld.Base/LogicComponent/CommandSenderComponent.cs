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

        //预处理命令
        //常见的处理有：服务器确认前 先做预测性表现、RTS低级指令转高级指令、连续指令输入型出招表
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