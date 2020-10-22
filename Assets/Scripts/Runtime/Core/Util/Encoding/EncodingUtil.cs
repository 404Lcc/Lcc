using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LccModel
{
    public static class EncodingUtil
    {
        public static byte[] LengthEncode(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(bytes.Length);
            writer.Write(bytes);
            bytes = stream.ToArray();
            stream.Close();
            writer.Close();
            return bytes;
        }
        public static byte[] LengthDecode(ref List<byte> cacheList)
        {
            MemoryStream stream = new MemoryStream(cacheList.ToArray());
            BinaryReader reader = new BinaryReader(stream);
            int length = reader.ReadInt32();
            if (length > stream.Length - stream.Position)
            {
                return null;
            }
            byte[] bytes = reader.ReadBytes(length);
            cacheList.Clear();
            cacheList.AddRange(reader.ReadBytes((int)(stream.Length - stream.Position)));
            stream.Close();
            reader.Close();
            return bytes;
        }
        public static byte[] SocketModelEncode(SocketModel model)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(model.type);
            writer.Write(model.area);
            writer.Write(model.command);
            if (model.message != null)
            {
                writer.Write(SerializationEncode(model.message));
            }
            byte[] bytes = stream.ToArray();
            stream.Close();
            writer.Close();
            return bytes;

        }
        public static SocketModel SocketModelDncode(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BinaryReader reader = new BinaryReader(stream);
            SocketModel model = new SocketModel();
            model.type = reader.ReadByte();
            model.area = reader.ReadInt32();
            model.command = reader.ReadInt32();
            if (stream.Length > stream.Position)
            {
                model.message = DeserializationDecode(reader.ReadBytes((int)(stream.Length - stream.Position)));
            }
            stream.Close();
            reader.Close();
            return model;
        }
        public static byte[] SerializationEncode(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            byte[] bytes = stream.ToArray();
            stream.Close();
            return bytes;
        }
        public static object DeserializationDecode(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BinaryFormatter formatter = new BinaryFormatter();
            object obj = formatter.Deserialize(stream);
            stream.Close();
            return obj;
        }
    }
}