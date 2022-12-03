using ProtoBuf;
using System;
using System.IO;

namespace LccModel
{
    public static class ProtobufUtil
    {
        public static byte[] Serialize(object message)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize(stream, message);
                return stream.ToArray();
            }
        }
        public static object Deserialize(Type type, byte[] bytes, int index, int count)
        {
            using (MemoryStream stream = new MemoryStream(bytes, index, count))
            {
                return Serializer.Deserialize(type, stream);
            }
        }
        public static T Deserialize<T>(byte[] bytes, int index, int count) where T : ProtobufObject
        {
            using (MemoryStream stream = new MemoryStream(bytes, index, count))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }
    }
}