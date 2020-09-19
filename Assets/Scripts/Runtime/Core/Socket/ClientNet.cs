using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Model
{
    public class ClientNet
    {
        public Socket socket;
        public CenterHandler handler;
        public string ip;
        public int port;
        public byte[] data;
        public List<byte> cacheList;
        public bool receive;
        public ClientNet(CenterHandler handler, string ip, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.handler = handler;
            this.ip = ip;
            this.port = port;
            data = new byte[1024];
            cacheList = new List<byte>();
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
            cacheList.AddRange(data);
            if (!receive)
            {
                receive = true;
                ReceiveHandle();
            }
            Receive();
        }
        public void ReceiveHandle()
        {
            if (cacheList.Count < 4)
            {
                receive = false;
                return;
            }
            byte[] data = EncodingTool.Instance.LengthDecode(ref cacheList);
            if (data == null)
            {
                receive = false;
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
            cacheList.Clear();
            receive = false;
        }
    }
}