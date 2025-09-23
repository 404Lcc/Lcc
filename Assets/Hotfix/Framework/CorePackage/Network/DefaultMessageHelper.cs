using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DefaultMessageHelper : IMessageHelper
{
    //消息粘包处理
    public byte[] GetBytes(object message)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        
        MemoryStream stream = new MemoryStream();
        formatter.Serialize(stream, message);
        stream.Seek(0, SeekOrigin.Begin);

        var code = message.GetType().GetHashCode();
        //消息号
        var codeBytes = BitConverter.GetBytes(code);
        
        //消息体
        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        stream.Dispose();


        //消息号长度+消息体长度
        byte[] lengthBytes = BitConverter.GetBytes(codeBytes.Length + buffer.Length);
        
        // 预分配总长度
        byte[] bytes = new byte[lengthBytes.Length + codeBytes.Length + buffer.Length];
        int offset = 0;

        // 依次复制数组
        Array.Copy(lengthBytes, 0, bytes, offset, lengthBytes.Length);
        offset += lengthBytes.Length;
        Array.Copy(codeBytes, 0, bytes, offset, codeBytes.Length);
        offset += codeBytes.Length;
        Array.Copy(buffer, 0, bytes, offset, buffer.Length);
        return bytes;
    }

    //消息号+消息体 字节数组解析
    public NetworkMessage MessageParse(byte[] bytes)
    {
        //消息号
        byte[] codeBytes = new byte[4];
        Array.Copy(bytes, 0, codeBytes, 0, 4); // [1,2,3,4]

        //消息体
        byte[] messageBytes = new byte[bytes.Length - 4];
        Array.Copy(bytes, 4, messageBytes, 0, bytes.Length - 4); // [5,6,7,8,9]
        
        
        //消息号
        int code = BitConverter.ToInt32(codeBytes, 0);

        MemoryStream stream = new MemoryStream();
        stream.Write(messageBytes, 0, messageBytes.Length);
        stream.Seek(0, SeekOrigin.Begin);
            
        BinaryFormatter formatter = new BinaryFormatter();
        //消息体
        object obj = formatter.Deserialize(stream);

        NetworkMessage message = new NetworkMessage();
        message.code = code;
        message.message = obj;
        return message;
    }
}