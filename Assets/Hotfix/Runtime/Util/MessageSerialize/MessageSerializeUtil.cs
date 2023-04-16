using LccModel;
using System.IO;

namespace LccHotfix
{
    public static class MessageSerializeUtil
    {
        private static MemoryStream GetStream(int count = 0)
        {
            MemoryStream stream;
            if (count > 0)
            {
                stream = new MemoryStream(count);
            }
            else
            {
                stream = new MemoryStream();
            }

            return stream;
        }

        public static (int, MemoryStream) MessageToStream(object message)
        {
            MemoryStream stream = GetStream(Packet.OpcodeLength);

            int opcode = NetServices.Instance.GetOpcode(message.GetType());

            stream.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
            stream.SetLength(Packet.OpcodeLength);

            stream.GetBuffer().WriteTo(0, opcode);

            SerializeUtil.Serialize(message, stream);

            stream.Seek(0, SeekOrigin.Begin);
            return (opcode, stream);
        }
    }
}