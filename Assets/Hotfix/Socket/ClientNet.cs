using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class ClientNet
{
    public Socket socket;
    public CenterHandler handler;
    public string ip;
    public int port;
    public byte[] data;
    public List<byte> cache;
    public bool breceive;
    public ClientNet(CenterHandler handler, string ip, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        this.handler = handler;
        this.ip = ip;
        this.port = port;
        data = new byte[1024];
        cache = new List<byte>();
    }
    public void Init()
    {
        socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
    }
    public void Receive()
    {
        socket.BeginReceive(data, 0, 1024, SocketFlags.None, ReceiveCompleted, data);
    }
    public void ReceiveCompleted(IAsyncResult result)
    {
        int length = socket.EndReceive(result);
        byte[] data = new byte[length];
        Buffer.BlockCopy(this.data, 0, data, 0, length);
        cache.AddRange(data);
        if (!breceive)
        {
            breceive = true;
            ReceiveHandle();
        }
        Receive();
    }
    public void ReceiveHandle()
    {
        if (cache.Count < 4)
        {
            breceive = false;
            return;
        }
        byte[] data = EncodingTool.Instance.LengthDecode(ref cache);
        if (data == null)
        {
            breceive = false;
            return;
        }
        SocketModel model = EncodingTool.Instance.SocketModelDncode(data);
        handler.Receive(model);
        ReceiveHandle();
    }
    public void Send(byte[] data)
    {
        socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCompleted, data);
    }
    public void SendCompleted(IAsyncResult result)
    {

    }
    public void Close()
    {
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        socket = null;
        cache.Clear();
        breceive = false;
    }
}
