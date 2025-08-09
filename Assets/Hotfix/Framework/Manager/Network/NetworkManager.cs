using HiSocket.Tcp;
using System;
using System.Collections.Generic;
using LccHotfix;
using UnityEngine;

public class NetworkManager : Module, INetworkService
{
    private const int MaxExecuteCount = 50;

    private string _ip;
    private int _port;
    private TcpConnection _tcp;
    private Action _onConnectedCallback;
    private Action _onDisconnectedCallback;
    private Action<NetworkMessage> _onReceiveMessageCallback;

    private int _timeOut = 5000;
    private object _lockQueue = new object();
    private Queue<byte[]> _reciveQueue; //接收协议队列
    private IPackageHelper _packageHelper;
    private IMessageHelper _messageHelper;

    public bool IsConnected => _tcp != null && _tcp.Socket.Connected;


    public NetworkManager()
    {
        lock (_lockQueue)
        {
            _reciveQueue = new Queue<byte[]>();
        }
    }

    internal override void Update(float elapseSeconds, float realElapseSeconds)
    {
        lock (_lockQueue)
        {
            int idx = 0;
            while (_reciveQueue.Count > 0 && idx <= MaxExecuteCount) //小于单帧执行数才分发协议
            {
                var bytes = _reciveQueue.Dequeue();
                var message = _messageHelper.MessageParse(bytes);
                _onReceiveMessageCallback?.Invoke(message);
                idx++;
            }
        }
    }


    internal override void Shutdown()
    {
        ForceDisconnect();

        _onConnectedCallback = null;
        _onDisconnectedCallback = null;
        _onReceiveMessageCallback = null;
        _packageHelper = null;
        _messageHelper = null;
    }

    public void SetPackageHelper(IPackageHelper packageHelper)
    {
        _packageHelper = packageHelper;
    }

    public void SetMessageHelper(IMessageHelper messageHelper)
    {
        _messageHelper = messageHelper;
    }

    public void Connect(string ip, int port, Action onConnectedCallback, Action onDisconnectedCallback, Action<NetworkMessage> onReceiveMessageCallback)
    {
        Debug.Assert(!IsConnected);

        _ip = ip;
        _port = port;

        _onConnectedCallback = onConnectedCallback;
        _onDisconnectedCallback = onDisconnectedCallback;
        _onReceiveMessageCallback = onReceiveMessageCallback;

        InitSocket();
        _tcp.Connect(ip, port);
        InitTimeout();
    }

    public void Send(object message)
    {
        if (!IsConnected)
        {
            Debug.Log("网络没有连接不能发消息。");
            return;
        }

        var bytes = _messageHelper.GetBytes(message);
        _tcp.Send(bytes);
    }

    public void Disconnect()
    {
        lock (_lockQueue)
        {
            _reciveQueue.Clear();
        }

        if (_tcp != null && _tcp.Socket.Connected)
        {
            var tcp = _tcp;
            _tcp = null;
            tcp.Dispose();

            _onDisconnectedCallback?.Invoke();
        }
    }

    public void ForceDisconnect()
    {
        lock (_lockQueue)
        {
            _reciveQueue.Clear();
        }

        if (_tcp != null)
        {
            var tcp = _tcp;
            _tcp = null;
            tcp.Dispose();
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

    private void InitTimeout()
    {
        if (_tcp != null && _tcp.Socket != null)
        {
            _tcp.Socket.ReceiveTimeout = _timeOut;
            _tcp.Socket.SendTimeout = _timeOut;
        }
    }


    private void OnConnected()
    {
        Debug.LogWarningFormat("网络连接成功。IP={0},Port={1}", _ip, _port.ToString());

        Main.ThreadSyncService.Post(() => { _onConnectedCallback?.Invoke(); });
    }

    private void OnReceive(byte[] message)
    {
        lock (_lockQueue)
        {
            if (message.Length > 0)
            {
                _reciveQueue.Enqueue(message);
            }
        }
    }

    private void OnDisconnected()
    {
        Main.ThreadSyncService.Post(() =>
        {
            if (_tcp != null)
            {
                _tcp = null;
            }

            lock (_lockQueue)
            {
                _reciveQueue.Clear();
            }

            _onDisconnectedCallback?.Invoke();
        });
    }
}