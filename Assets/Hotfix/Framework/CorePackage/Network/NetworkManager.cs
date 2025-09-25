using HiSocket.Tcp;
using System;
using System.Collections.Generic;
using LccHotfix;
using UnityEngine;

namespace LccHotfix
{
    public class NetworkManager : Module, INetworkService
    {
        private const int MaxExecuteCount = 50;

        private string _ip;
        private int _port;
        private TcpConnection _tcp;
        private Action _onConnectedCallback;
        private Action _onDisconnectedCallback;

        private int _timeOut = 5000;
        private Queue<byte[]> _reciveQueue; //接收协议队列
        private IPackageHelper _packageHelper;
        private IMessageHelper _messageHelper;
        private IMessageDispatcherHelper _messageDispatcherHelper;

        public bool IsConnected => _tcp != null && _tcp.Socket != null && _tcp.Socket.Connected;


        public NetworkManager()
        {
            _reciveQueue = new Queue<byte[]>();
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            int idx = 0;
            while (_reciveQueue.Count > 0 && idx <= MaxExecuteCount) //小于单帧执行数才分发协议
            {
                var bytes = _reciveQueue.Dequeue();
                var message = _messageHelper.MessageParse(bytes);
                _messageDispatcherHelper.DispatcherMessage(message);
                idx++;
            }
        }


        internal override void Shutdown()
        {
            _packageHelper = null;
            _messageHelper = null;

            Disconnect();
        }

        public void SetPackageHelper(IPackageHelper packageHelper)
        {
            _packageHelper = packageHelper;
        }

        public void SetMessageHelper(IMessageHelper messageHelper)
        {
            _messageHelper = messageHelper;
        }

        public void SetMessageDispatcherHelper(IMessageDispatcherHelper messageDispatcherHelper)
        {
            _messageDispatcherHelper = messageDispatcherHelper;
        }

        private void InitTimeout()
        {
            if (_tcp != null && _tcp.Socket != null)
            {
                _tcp.Socket.ReceiveTimeout = _timeOut;
                _tcp.Socket.SendTimeout = _timeOut;
            }
        }

        private void InitSocket()
        {
            Debug.Assert(_tcp == null);
            var package = new DefaultPackage();
            package.SetPackageHelper(_packageHelper);
            _tcp = new TcpConnection(package);
            _tcp.OnConnected += OnConnected;
            _tcp.OnReceiveMessage += OnReceive;
            _tcp.OnDisconnected += OnDisconnected;
        }

        public void Connect(string ip, int port, Action onConnectedCallback, Action onDisconnectedCallback)
        {
            Debug.Assert(!IsConnected);

            _ip = ip;
            _port = port;

            _onConnectedCallback = onConnectedCallback;
            _onDisconnectedCallback = onDisconnectedCallback;

            InitSocket();
            _tcp.Connect(ip, port);
            InitTimeout();
        }

        public void Send(int code, object message)
        {
            if (!IsConnected)
            {
                Debug.Log("网络没有连接不能发消息。");
                return;
            }

            var bytes = _messageHelper.GetBytes(code, message);
            _tcp.Send(bytes);
        }

        public void Disconnect()
        {
            _onConnectedCallback = null;
            _onDisconnectedCallback = null;

            _reciveQueue.Clear();

            if (IsConnected)
            {
                _tcp.OnConnected -= OnConnected;
                _tcp.OnReceiveMessage -= OnReceive;
                _tcp.OnDisconnected -= OnDisconnected;
                _tcp.Dispose();
            }

            _tcp = null;
        }


        private void OnConnected()
        {
            Debug.LogWarningFormat("网络连接成功。IP={0},Port={1}", _ip, _port.ToString());

            Main.ThreadSyncService.Post(() => { _onConnectedCallback?.Invoke(); });
        }

        private void OnReceive(byte[] message)
        {
            Main.ThreadSyncService.Post(() =>
            {
                if (message.Length > 0)
                {
                    _reciveQueue.Enqueue(message);
                }
            });
        }

        private void OnDisconnected()
        {
            Main.ThreadSyncService.Post(() =>
            {
                _reciveQueue.Clear();

                _tcp = null;

                _onDisconnectedCallback?.Invoke();
            });
        }
    }
}