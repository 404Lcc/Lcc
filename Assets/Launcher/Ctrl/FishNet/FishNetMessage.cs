using FishNet.Broadcast;

namespace LccModel
{
    public struct FishNetMessage : IBroadcast
    {
        public int code;
        public byte[] bytes;
    }
}