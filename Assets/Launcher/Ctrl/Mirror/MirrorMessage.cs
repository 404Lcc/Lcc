using Mirror;

namespace LccModel
{
    public struct MirrorMessage : Mirror.NetworkMessage
    {
        public int code;
        public byte[] bytes;

        public static void Register()
        {
            Writer<MirrorMessage>.write = Write;
            Reader<MirrorMessage>.read = Read;
        }

        public static void Write(NetworkWriter writer, MirrorMessage value)
        {
            writer.WriteArray(value.bytes);
        }

        public static MirrorMessage Read(NetworkReader reader)
        {
            MirrorMessage value = new MirrorMessage();
            value.bytes = reader.ReadArray<byte>();
            return value;
        }
    }
}