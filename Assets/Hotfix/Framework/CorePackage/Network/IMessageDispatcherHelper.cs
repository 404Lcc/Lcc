using System;

namespace LccHotfix
{
    public interface IMessageDispatcherHelper
    {
        public void RegisterMessage(int code, Action<MessageObject> handle);

        public void UnregisterMessage(int code, Action<MessageObject> handle);

        public void DispatcherMessage(NetworkMessage message);
    }
}