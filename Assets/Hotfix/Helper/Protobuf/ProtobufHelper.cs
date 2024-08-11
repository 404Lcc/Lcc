using ProtoBuf;
using System;
using System.ComponentModel;
using System.IO;

namespace LccModel
{
    public static class ProtobufHelper
    {
        public static object Deserialize(Type type, byte[] bytes, int index, int count)
        {
            using MemoryStream stream = new MemoryStream(bytes, index, count);
            object o = Serializer.Deserialize(type, stream);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }

        public static byte[] Serialize(object message)
        {
            using MemoryStream stream = new MemoryStream();
            Serializer.Serialize(stream, message);
            return stream.ToArray();
        }

        public static void Serialize(object message, Stream stream)
        {
            Serializer.Serialize(stream, message);
        }

        public static object Deserialize(Type type, Stream stream)
        {
            object o = Serializer.Deserialize(type, stream);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }
    }
}