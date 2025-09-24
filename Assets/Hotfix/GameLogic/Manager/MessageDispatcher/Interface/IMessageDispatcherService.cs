using System;

namespace LccHotfix
{
    public interface IMessageDispatcherService : IService
    {
        void RegisterMessage(int code, Action<ProtobufObject> messageHandle);
        void UnregisterMessage(int code, Action<ProtobufObject> messageHandle);
        void DispatcherMessage(NetworkMessage message);
    }
}