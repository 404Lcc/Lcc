using System;

namespace LccHotfix
{
    public interface INetworkService : IService
    {
        void SetPackageHelper(IPackageHelper packagePHelper);
        void SetMessageHelper(IMessageHelper messageHelper);
        void SetMessageDispatcherHelper(IMessageDispatcherHelper messageDispatcherHelper);
        bool IsConnected { get; }
        void Connect(string ip, int port, Action onConnectedCallback, Action onDisconnectedCallback);
        void Send(int code, object message);
        void Disconnect();
    }
}