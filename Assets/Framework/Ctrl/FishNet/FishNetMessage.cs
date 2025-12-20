using FishNet.Broadcast;
using FishNet.CodeGenerating;
using FishNet.Serializing;
using GameKit.Dependencies.Utilities;

namespace LccModel
{
    [UseGlobalCustomSerializer]
    public struct FishNetMessage : IBroadcast
    {
        public int code;
        public byte[] bytes;
    }

    internal static class FishNetMessageSerializers
    {
        public static void Write(this Writer writer, FishNetMessage value)
        {
            writer.Write(value.code);
            writer.WriteArray(value.bytes);
        }

        public static FishNetMessage Read(this Reader reader)
        {
            int code = reader.ReadInt32();
            var bytes = CollectionCaches<byte>.RetrieveArray();
            reader.ReadArray(ref bytes);

            return new FishNetMessage()
            {
                code = code,
                bytes = bytes
            };
        }
    }
}