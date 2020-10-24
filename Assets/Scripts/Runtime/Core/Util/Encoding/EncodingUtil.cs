using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LccModel
{
    public static class EncodingUtil
    {
        public static byte[] LengthEncode(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                    bytes = stream.ToArray();
                }
            }
            return bytes;
        }
        public static byte[] LengthDecode(ref List<byte> cacheList)
        {
            byte[] bytes;
            using (MemoryStream stream = new MemoryStream(cacheList.ToArray()))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int length = reader.ReadInt32();
                    if (length > stream.Length - stream.Position)
                    {
                        return null;
                    }
                    bytes = reader.ReadBytes(length);
                    cacheList.Clear();
                    cacheList.AddRange(reader.ReadBytes((int)(stream.Length - stream.Position)));
                }
            }
            return bytes;
        }
        public static byte[] SocketModelEncode(SocketModel model)
        {
            byte[] bytes;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(model.type);
                    writer.Write(model.area);
                    writer.Write(model.command);
                    if (model.message != null)
                    {
                        writer.Write(SerializationEncode(model.message));
                    }
                    bytes = stream.ToArray();
                }
            }
            return bytes;

        }
        public static SocketModel SocketModelDncode(byte[] bytes)
        {
            SocketModel model = new SocketModel();
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    model.type = reader.ReadByte();
                    model.area = reader.ReadInt32();
                    model.command = reader.ReadInt32();
                    if (stream.Length > stream.Position)
                    {
                        model.message = DeserializationDecode(reader.ReadBytes((int)(stream.Length - stream.Position)));
                    }
                }
            }
            return model;
        }
        public static byte[] SerializationEncode(object obj)
        {
            byte[] bytes;
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                bytes = stream.ToArray();
            }
            return bytes;
        }
        public static object DeserializationDecode(byte[] bytes)
        {
            object obj;
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                obj = formatter.Deserialize(stream);
            }
            return obj;
        }
    }
}