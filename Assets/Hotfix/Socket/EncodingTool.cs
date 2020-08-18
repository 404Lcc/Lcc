using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class EncodingTool
{
    private static EncodingTool _instance;
    public static EncodingTool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EncodingTool();
            }
            return _instance;
        }
    }
    public byte[] LengthEncode(byte[] data)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(data.Length);
        writer.Write(data);
        data = stream.ToArray();
        stream.Close();
        writer.Close();
        return data;
    }
    public byte[] LengthDecode(ref List<byte> cacheList)
    {
        MemoryStream stream = new MemoryStream(cacheList.ToArray());
        BinaryReader reader = new BinaryReader(stream);
        int length = reader.ReadInt32();
        if (length > stream.Length - stream.Position)
        {
            return null;
        }
        byte[] data = reader.ReadBytes(length);
        cacheList.Clear();
        cacheList.AddRange(reader.ReadBytes((int)(stream.Length - stream.Position)));
        stream.Close();
        reader.Close();
        return data;
    }
    public byte[] SocketModelEncode(SocketModel model)
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
        byte[] data = stream.ToArray();
        stream.Close();
        writer.Close();
        return data;

    }
    public SocketModel SocketModelDncode(byte[] data)
    {
        MemoryStream stream = new MemoryStream(data);
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
    public byte[] SerializationEncode(object obj)
    {
        MemoryStream stream = new MemoryStream();
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, obj);
        byte[] data = stream.ToArray();
        stream.Close();
        return data;
    }
    public object DeserializationDecode(byte[] data)
    {
        MemoryStream stream = new MemoryStream(data);
        BinaryFormatter formatter = new BinaryFormatter();
        object obj = formatter.Deserialize(stream);
        stream.Close();
        return obj;
    }
}