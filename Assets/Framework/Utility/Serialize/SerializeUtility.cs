using LccModel;
using System;
using System.IO;

namespace LccModel
{
    public static class SerializeUtility
    {
        public static object Deserialize(Type type, byte[] bytes, int index, int count)
        {
            return ProtobufUtility.Deserialize(type, bytes, index, count);
        }

        public static byte[] Serialize(object message)
        {
            return ProtobufUtility.Serialize(message);
        }

        public static void Serialize(object message, Stream stream)
        {
            ProtobufUtility.Serialize(message, stream);
        }

        public static object Deserialize(Type type, Stream stream)
        {
            return ProtobufUtility.Deserialize(type, stream);
        }
    }
}