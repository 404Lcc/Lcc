using System;
using System.IO;
using FishNet.Transporting;
using LccModel;
using UnityEngine;
using NetworkConnection = FishNet.Connection.NetworkConnection;

namespace LccHotfix
{
    [Flags]
    public enum FishNetState
    {
        None = 0,
        Loading = 1,
        Initialized = 2,
    }

    internal class FishNetManager : Module, IFishNetService
    {
        private AssetLoader _loader;
        private FishNetState _state;
        private FishNet.Managing.NetworkManager _networkManager;

        private IFishNetHelper _helper;
        private IFishNetServerMessageDispatcherHelper _serverMessageDispatcherHelper;
        private IMessageDispatcherHelper _clientMessageDispatcherHelper;
        private IFishNetCallbackHelper _callbackHelper;

        public bool IsClient => _networkManager.IsClientStarted;
        public bool IsServer => _networkManager.IsServerStarted;
        public bool Finished => _state == FishNetState.Initialized;

        /// <summary>
        /// 判断网络是否连接
        /// </summary>
        /// <returns></returns>
        public bool IsNetworkActive
        {
            get
            {
                if (!Finished)
                    return false;
                return IsClient || IsServer;
            }
        }

        public FishNetManager()
        {
            _loader = new AssetLoader();
        }

        internal override void Shutdown()
        {
            _state = FishNetState.None;

            if (_networkManager != null)
            {
                GameObject.Destroy(_networkManager.transform.parent.gameObject);
                _networkManager = null;
            }

            _loader.Release();
            _loader = null;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Init()
        {
            if (_state != FishNetState.None)
                return;
            _state = FishNetState.Loading;
            _helper.Setup(_loader, (x) =>
            {
                _networkManager = x;
                _state = FishNetState.Initialized;
            });
        }

        /// <summary>
        /// 设置辅助器
        /// </summary>
        /// <param name="helper"></param>
        public void SetHelper(IFishNetHelper helper)
        {
            _helper = helper;
        }

        /// <summary>
        /// 设置IP
        /// </summary>
        /// <param name="networkAddress"></param>
        public void SetNetworkAddress(string networkAddress = "")
        {
            _helper.SetNetworkAddress(_networkManager.gameObject, networkAddress);
        }

        /// <summary>
        /// 设置服务器消息分发器
        /// </summary>
        /// <param name="messageDispatcherHelper"></param>
        public void SetServerMessageDispatcherHelper(IFishNetServerMessageDispatcherHelper messageDispatcherHelper)
        {
            _serverMessageDispatcherHelper = messageDispatcherHelper;
        }

        /// <summary>
        /// 设置客户端消息分发器
        /// </summary>
        /// <param name="messageDispatcherHelper"></param>
        public void SetClientMessageDispatcherHelper(IMessageDispatcherHelper messageDispatcherHelper)
        {
            _clientMessageDispatcherHelper = messageDispatcherHelper;
        }

        /// <summary>
        /// 设置回调
        /// </summary>
        public void SetCallbackHelper(IFishNetCallbackHelper callbackHelper)
        {
            _callbackHelper = callbackHelper;
        }

        /// <summary>
        /// 注册
        /// </summary>
        public void Register()
        {
            _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
            _networkManager.ServerManager.OnRemoteConnectionState += OnServerRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;

            _networkManager.ServerManager.RegisterBroadcast<FishNetMessage>(ServerMessage);
            _networkManager.ClientManager.RegisterBroadcast<FishNetMessage>(ClientMessage);
        }

        /// <summary>
        /// 反注册
        /// </summary>
        public void Unregister()
        {
            _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
            _networkManager.ServerManager.OnRemoteConnectionState -= OnServerRemoteConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;

            _networkManager.ServerManager.UnregisterBroadcast<FishNetMessage>(ServerMessage);
            _networkManager.ClientManager.UnregisterBroadcast<FishNetMessage>(ClientMessage);
        }

        /// <summary>
        /// 开启服务器
        /// </summary>
        public void StartServer()
        {
            if (!Finished)
                return;

            if (IsServer)
                return;

            _networkManager.ServerManager.StartConnection();

        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void StopServer()
        {
            if (!Finished)
                return;

            if (!IsServer)
                return;

            _networkManager.ServerManager.StopConnection(true);
        }

        /// <summary>
        /// 客户端开始连接
        /// </summary>
        public void Connect()
        {
            if (!Finished)
                return;

            if (IsClient)
                return;

            _networkManager.ClientManager.StartConnection();
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public void Disconnect()
        {
            if (!Finished)
                return;

            if (!IsClient)
                return;

            _networkManager.ClientManager.StopConnection();
        }

        #region 服务器接口

        /// <summary>
        /// 服务器注册消息
        /// </summary>
        public void ServerRegisterMessage(int code, Action<NetworkConnection, MessageObject> handle)
        {
            _serverMessageDispatcherHelper.RegisterMessage(code, handle);
        }

        /// <summary>
        /// 服务器解绑消息
        /// </summary>
        public void ServerUnregisterMessage(int code, Action<NetworkConnection, MessageObject> handle)
        {
            _serverMessageDispatcherHelper.UnregisterMessage(code, handle);
        }

        /// <summary>
        /// 服务器发送消息
        /// </summary>
        public void ServerSendMessage<T>(NetworkConnection client, T message) where T : MessageObject
        {
            if (IsServer)
            {
                var typeName = message.GetType().Name;
                var enumName = typeName.Substring(0, typeName.Length - 4);
                MessageType messageType = (MessageType)Enum.Parse(typeof(MessageType), enumName);
                var code = (int)messageType;
                FishNetMessage msg = new FishNetMessage();
                msg.code = code;
                msg.bytes = ProtobufUtility.Serialize(message);
                client.Broadcast(msg);
            }
        }

        /// <summary>
        /// 服务器广播消息
        /// </summary>
        public void ServerBroadcastMessage<T>(T message, NetworkConnection exclusionClient = null) where T : MessageObject
        {
            if (IsServer)
            {
                var typeName = message.GetType().Name;
                var enumName = typeName.Substring(0, typeName.Length - 4);
                MessageType messageType = (MessageType)Enum.Parse(typeof(MessageType), enumName);
                var code = (int)messageType;
                FishNetMessage msg = new FishNetMessage();
                msg.code = code;
                msg.bytes = ProtobufUtility.Serialize(message);

                foreach (var conn in _networkManager.ServerManager.Clients.Values)
                {
                    if (exclusionClient != null && conn.ClientId == exclusionClient.ClientId)
                        continue;
                    conn.Broadcast(msg);
                }
            }
        }

        #endregion

        #region 客户端接口

        /// <summary>
        /// 客户端注册消息
        /// </summary>
        public void ClientRegisterMessage(int code, Action<MessageObject> handle)
        {
            _clientMessageDispatcherHelper.RegisterMessage(code, handle);
        }

        /// <summary>
        /// 客户端解绑消息
        /// </summary>
        public void ClientUnregisterMessage(int code, Action<MessageObject> handle)
        {
            _clientMessageDispatcherHelper.UnregisterMessage(code, handle);
        }

        /// <summary>
        /// 客户端发送消息
        /// </summary>
        public void ClientSendMessage<T>(T message) where T : MessageObject
        {
            if (IsClient)
            {
                var typeName = message.GetType().Name;
                var enumName = typeName.Substring(0, typeName.Length - 4);
                MessageType messageType = (MessageType)Enum.Parse(typeof(MessageType), enumName);
                var code = (int)messageType;
                FishNetMessage msg = new FishNetMessage();
                msg.code = code;
                msg.bytes = ProtobufUtility.Serialize(message);
                _networkManager.ClientManager.Connection.Broadcast(msg);
            }
        }

        #endregion

        private void ServerMessage(NetworkConnection client, FishNetMessage message, Channel channel)
        {
            NetworkMessage networkMessage = new NetworkMessage();
            networkMessage.code = message.code;

            MemoryStream stream = new MemoryStream();
            stream.Write(message.bytes, 0, message.bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);

            string typeName = "LccHotfix." + Enum.GetName(typeof(MessageType), (MessageType)message.code) + "Info";
            object obj = ProtobufUtility.Deserialize(Main.CodeTypesService.GetType(typeName), stream);
            networkMessage.message = obj;

            _serverMessageDispatcherHelper.DispatcherMessage(client, networkMessage);
        }

        private void ClientMessage(FishNetMessage message, Channel channel)
        {
            NetworkMessage networkMessage = new NetworkMessage();
            networkMessage.code = message.code;

            MemoryStream stream = new MemoryStream();
            stream.Write(message.bytes, 0, message.bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);

            string typeName = "LccHotfix." + Enum.GetName(typeof(MessageType), (MessageType)message.code) + "Info";
            object obj = ProtobufUtility.Deserialize(Main.CodeTypesService.GetType(typeName), stream);
            networkMessage.message = obj;

            _clientMessageDispatcherHelper.DispatcherMessage(networkMessage);
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                _callbackHelper.OnClientConnectedCallback();
            }

            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                _callbackHelper.OnClientDisconnectedCallback();
            }
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                _callbackHelper.OnServerStartCallback();
            }

            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                _callbackHelper.OnServerStopCallback();
            }
        }

        private void OnServerRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState == RemoteConnectionState.Stopped)
            {
                _callbackHelper.OnServerRemoteClientDisconnectedCallback(connection.ClientId);
            }
        }
    }
}