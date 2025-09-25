using System;
using Mirror;

namespace LccHotfix
{
    public interface IMirrorServerMessageDispatcherHelper
    {
        public void RegisterMessage(int code, Action<NetworkConnectionToClient, MessageObject> handle);

        public void UnregisterMessage(int code, Action<NetworkConnectionToClient, MessageObject> handle);

        public void DispatcherMessage(NetworkConnectionToClient client, NetworkMessage message);
    }
}