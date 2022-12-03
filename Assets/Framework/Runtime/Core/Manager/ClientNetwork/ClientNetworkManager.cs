using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LccModel
{
    public class ClientNetworkManager : AObjectBase
    {
        public static ClientNetworkManager Instance { get; set; }
        public Socket socket;
        public ICenterHandler iCenterHandler;
        public string ip;
        public int port;
        public byte[] bytes = new byte[1024];
        public List<byte> cacheList = new List<byte>();
        public bool receive;

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Close();
            Instance = null;
        }



        public void Connect(ICenterHandler iCenterHandler, string ip, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.iCenterHandler = iCenterHandler;
            this.ip = ip;
            this.port = port;
            socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        }
        public void Receive()
        {
            socket.BeginReceive(bytes, 0, 1024, SocketFlags.None, ReceiveCompleted, bytes);
        }
        public void ReceiveCompleted(IAsyncResult result)
        {
            int length = socket.EndReceive(result);
            byte[] bytes = new byte[length];
            Buffer.BlockCopy(this.bytes, 0, bytes, 0, length);
            cacheList.AddRange(bytes);
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
            byte[] data = EncodingUtil.LengthDecode(ref cacheList);
            if (data == null)
            {
                receive = false;
                return;
            }
            SocketModel model = EncodingUtil.SocketModelDncode(data);
            iCenterHandler.Receive(model);
            ReceiveHandle();
        }
        public void Send(byte[] bytes)
        {
            socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendCompleted, bytes);
        }
        public void Send(byte type, int area, int command, object message)
        {
            SocketModel model = new SocketModel(type, area, command, message);
            byte[] bytes = EncodingUtil.LengthEncode(EncodingUtil.SocketModelEncode(model));
            Send(bytes);
        }
        public void SendCompleted(IAsyncResult result)
        {
        }
        public void Close()
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
                cacheList.Clear();
                receive = false;
            }
        }
    }
}