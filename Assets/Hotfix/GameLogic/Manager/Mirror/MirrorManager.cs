using System;
using System.IO;
using LccModel;
using Mirror;
using UnityEngine;

namespace LccHotfix
{
    internal class MirrorManager : Module, IMirrorService
    {
        private bool _init;
        private MirrorNetworkManager _networkManager;
        private IMirrorTransportHelper _transportHelper;
        private IMirrorServerMessageDispatcherHelper _serverMessageDispatcherHelper;
        private IMessageDispatcherHelper _clientMessageDispatcherHelper;
        private IMirrorCallbackHelper _callbackHelper;

        public bool IsClient => NetworkClient.active;
        public bool IsServer => NetworkServer.active;

        public bool IsNetworkActive
        {
            get
            {
                if (!_init)
                    return false;
                return IsClient || IsServer;
            }
        }

        internal override void Shutdown()
        {
            if (!_init)
                return;
            _init = false;

            GameObject.Destroy(_networkManager);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Init()
        {
            if (_init)
                return;

            GameObject obj = new GameObject("MirrorNetworkManager");
            _transportHelper.SetupTransport(obj);
            _networkManager = obj.AddComponent<MirrorNetworkManager>();

            Main.AssetService.LoadRes<GameObject>(obj, "MirrorUnit", out var res);

            _networkManager.playerPrefab = res;

            GameObject.DontDestroyOnLoad(obj);

            _init = true;
        }

        /// <summary>
        /// 设置传输器
        /// </summary>
        /// <param name="transportHelper"></param>
        public void SetTransportHelper(IMirrorTransportHelper transportHelper)
        {
            _transportHelper = transportHelper;
        }

        /// <summary>
        /// 设置IP
        /// </summary>
        /// <param name="networkAddress"></param>
        public void SetNetworkAddress(string networkAddress = "")
        {
            if (!_init)
                return;
            if (string.IsNullOrEmpty(networkAddress))
            {
                networkAddress = GameUtility.GetLocalIPAddress();
            }

            _networkManager.networkAddress = networkAddress;
        }

        /// <summary>
        /// 设置服务器消息分发器
        /// </summary>
        /// <param name="messageDispatcherHelper"></param>
        public void SetServerMessageDispatcherHelper(IMirrorServerMessageDispatcherHelper messageDispatcherHelper)
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
        public void SetCallbackHelper(IMirrorCallbackHelper callbackHelper)
        {
            _callbackHelper = callbackHelper;
        }

        /// <summary>
        /// 开启主机
        /// </summary>
        public void StartHost()
        {
            if (!_init)
                return;

            if (IsNetworkActive)
                return;

            NetworkServer.RegisterHandler<MirrorMessage>(ServerMessage);
            NetworkClient.RegisterHandler<MirrorMessage>(ClientMessage);

            _networkManager.StartServer();
            _networkManager.StartClient();
        }

        /// <summary>
        /// 停止主机
        /// </summary>
        public void StopHost()
        {
            if (!_init)
                return;

            if (!IsNetworkActive)
                return;

            NetworkServer.UnregisterHandler<MirrorMessage>();
            NetworkClient.UnregisterHandler<MirrorMessage>();

            _networkManager.StopClient();
            _networkManager.StopServer();
        }

        /// <summary>
        /// 客户端开始连接
        /// </summary>
        public void Connect()
        {
            if (!_init)
                return;

            if (IsNetworkActive)
                return;

            NetworkServer.RegisterHandler<MirrorMessage>(ServerMessage);
            NetworkClient.RegisterHandler<MirrorMessage>(ClientMessage);

            _networkManager.StartClient();
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public void Disconnect()
        {
            if (!_init)
                return;

            if (!IsNetworkActive)
                return;

            NetworkServer.UnregisterHandler<MirrorMessage>();
            NetworkClient.UnregisterHandler<MirrorMessage>();

            _networkManager.StopClient();
        }

        #region 服务器接口

        /// <summary>
        /// 服务器注册消息
        /// </summary>
        public void ServerRegisterMessage(int code, Action<NetworkConnectionToClient, MessageObject> handle)
        {
            _serverMessageDispatcherHelper.RegisterMessage(code, handle);
        }

        /// <summary>
        /// 服务器解绑消息
        /// </summary>
        public void ServerUnregisterMessage(int code, Action<NetworkConnectionToClient, MessageObject> handle)
        {
            _serverMessageDispatcherHelper.UnregisterMessage(code, handle);
        }

        /// <summary>
        /// 服务器发送消息
        /// </summary>
        public void ServerSendMessage<T>(NetworkConnectionToClient client, T message) where T : MessageObject
        {
            if (IsServer)
            {
                var typeName = message.GetType().Name;
                var enumName = typeName.Substring(0, typeName.Length - 4);
                MessageType messageType = (MessageType)Enum.Parse(typeof(MessageType), enumName);
                var code = (int)messageType;
                MirrorMessage msg = new MirrorMessage();
                msg.code = code;
                msg.bytes = ProtobufUtility.Serialize(message);
                client.Send(msg);
            }
        }

        /// <summary>
        /// 服务器广播消息
        /// </summary>
        public void ServerBroadcastMessage<T>(T message, NetworkConnectionToClient exclusionClient = null) where T : MessageObject
        {
            if (IsServer)
            {
                var typeName = message.GetType().Name;
                var enumName = typeName.Substring(0, typeName.Length - 4);
                MessageType messageType = (MessageType)Enum.Parse(typeof(MessageType), enumName);
                var code = (int)messageType;
                MirrorMessage msg = new MirrorMessage();
                msg.code = code;
                msg.bytes = ProtobufUtility.Serialize(message);

                foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
                {
                    if (exclusionClient != null && conn.connectionId == exclusionClient.connectionId)
                        continue;
                    conn.Send(msg);
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
                MirrorMessage msg = new MirrorMessage();
                msg.code = code;
                msg.bytes = ProtobufUtility.Serialize(message);
                NetworkClient.Send(msg);
            }
        }

        #endregion

        private void ServerMessage(NetworkConnectionToClient client, MirrorMessage message)
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

        private void ClientMessage(MirrorMessage message)
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

        private void OnServerStart()
        {
            _callbackHelper.OnServerStartCallback();
        }

        private void OnServerStop()
        {
            _callbackHelper.OnServerStopCallback();
        }

        private void OnClientConnected()
        {
            _callbackHelper.OnClientConnectedCallback();
        }

        private void OnClientDisconnected()
        {
            _callbackHelper.OnClientDisconnectedCallback();
        }

        private void OnServerRemoteClientDisconnected(int connectionId)
        {
            _callbackHelper.OnServerRemoteClientDisconnectedCallback(connectionId);
        }
    }
}