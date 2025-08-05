using System;

namespace LccHotfix
{
    public interface INetworkService : IService
    {
        void SetPackageHelper(IPackageHelper packagePHelper);
        void SetMessageHelper(IMessageHelper messageHelper);
        bool IsConnected { get; }
        void Connect(string ip, int port, Action onConnectedCallback, Action onDisconnectedCallback, Action<NetworkMessage> onReciveMessageCallback);
        void Send(object message);
        void Disconnect();
        void ForceDisconnect();
    }
}