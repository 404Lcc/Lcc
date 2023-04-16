using LccModel;
using System;
using System.IO;

namespace LccModel
{
    public static class SerializeUtil
    {
        public static object Deserialize(Type type, byte[] bytes, int index, int count)
        {
            return ProtobufUtil.Deserialize(type, bytes, index, count);
        }

        public static byte[] Serialize(object message)
        {
            return ProtobufUtil.Serialize(message);
        }

        public static void Serialize(object message, Stream stream)
        {
            ProtobufUtil.Serialize(message, stream);
        }

        public static object Deserialize(Type type, Stream stream)
        {
            return ProtobufUtil.Deserialize(type, stream);
        }
    }
}