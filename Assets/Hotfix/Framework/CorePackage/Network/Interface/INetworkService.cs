using System;

namespace LccHotfix
{
    public interface INetworkService : IService
    {
        bool IsConnected { get; }
        void SetPackageHelper(IPackageHelper packagePHelper);
        void SetMessageHelper(IMessageHelper messageHelper);
        void SetMessageDispatcherHelper(IMessageDispatcherHelper messageDispatcherHelper);
        void SetNetworkCallbackHelper(INetworkCallbackHelper networkCallbackHelper);
        void Connect(string ip, int port);
        void Send(int code, object message);
        void Disconnect();
    }
}