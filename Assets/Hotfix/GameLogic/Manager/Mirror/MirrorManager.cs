using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using kcp2k;
using LccModel;
using Mirror;
using UnityEngine;

namespace LccHotfix
{
    public class NetworkUnit
    {
        private MirrorUnitCtrl networkUnitCtrl;
        public uint NetId => networkUnitCtrl.netId;
        public bool IsLocalPlayer => networkUnitCtrl.isLocalPlayer;

        public void Init(MirrorUnitCtrl ctrl)
        {
            this.networkUnitCtrl = ctrl;
        }
    }

    internal class MirrorManager : Module, IMirrorService
    {
        //服务器才使用的列表
        private DictionaryList<uint, MirrorUnitCtrl> _unitDict = new DictionaryList<uint, MirrorUnitCtrl>();

        private Mirror.NetworkManager _networkManager;
        private NetworkUnit _localUnit;
        private IMirrorServerMessageDispatcherHelper _serverMessageDispatcherHelper;
        private IMessageDispatcherHelper _clientMessageDispatcherHelper;

        public uint LocalNetId => _localUnit.NetId;

        public bool IsLocalPlayer => _localUnit.IsLocalPlayer;
        public bool IsClient => NetworkClient.active;
        public bool IsServer => NetworkServer.active;

        public bool IsHost => NetworkServer.activeHost;


        public MirrorManager()
        {
            GameObject obj = new GameObject("MirrorManager");
            obj.AddComponent<KcpTransport>();
            _networkManager = obj.AddComponent<Mirror.NetworkManager>();

            Main.AssetService.LoadRes<GameObject>(obj, "MirrorUnit", out var res);

            _networkManager.playerPrefab = res;

            GameObject.DontDestroyOnLoad(obj);

            NetworkServer.RegisterHandler<MirrorMessage>(MirrorServerMessage);
            NetworkClient.RegisterHandler<MirrorMessage>(MirrorClientMessage);
        }

        private void MirrorServerMessage(NetworkConnectionToClient client, MirrorMessage message)
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

        private void MirrorClientMessage(MirrorMessage message)
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

        internal override void Shutdown()
        {
            NetworkServer.UnregisterHandler<MirrorMessage>();
            NetworkClient.UnregisterHandler<MirrorMessage>();

            GameObject.Destroy(_networkManager);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 获取本地IP地址
        /// </summary>
        /// <returns></returns>
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "127.0.0.1";
        }

        /// <summary>
        /// 设置IP
        /// </summary>
        /// <param name="networkAddress"></param>
        public void SetNetworkAddress(string networkAddress = "")
        {
            if (string.IsNullOrEmpty(networkAddress))
            {
                networkAddress = GetLocalIPAddress();
            }

            _networkManager.networkAddress = networkAddress;
        }


        /// <summary>
        /// 设置端口
        /// </summary>
        /// <param name="port"></param>
        public void SetPort(int port = 7788)
        {
            KcpTransport transport = _networkManager.gameObject.GetComponent<KcpTransport>();
            transport.Port = (ushort)port;
        }

        public void SetServerMessageDispatcherHelper(IMirrorServerMessageDispatcherHelper messageDispatcherHelper)
        {
            _serverMessageDispatcherHelper = messageDispatcherHelper;
        }

        public void SetClientMessageDispatcherHelper(IMessageDispatcherHelper messageDispatcherHelper)
        {
            _clientMessageDispatcherHelper = messageDispatcherHelper;
        }


        public bool IsNetworkActive()
        {
            return _networkManager.isNetworkActive;
        }


        /// <summary>
        /// 开启主机
        /// </summary>
        public void StartHost()
        {
            if (_networkManager.isNetworkActive)
                return;
            _networkManager.StartHost();
        }

        /// <summary>
        /// 停止主机
        /// </summary>
        public void StopHost()
        {
            if (!_networkManager.isNetworkActive)
                return;
            _networkManager.StopHost();
        }

        /// <summary>
        /// 客户端开始连接
        /// </summary>
        public void Connect()
        {
            if (_networkManager.isNetworkActive)
                return;

            _networkManager.StartClient();
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public void Disconnect()
        {
            if (!_networkManager.isNetworkActive)
                return;
            _networkManager.StopClient();
        }

        #region 服务器接口

        /// <summary>
        /// 服务器注册消息
        /// </summary>
        /// <param name="handle"></param>
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
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
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
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public void ServerBroadcastMessage<T>(T message) where T : MessageObject
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
                NetworkServer.SendToAll(msg);
            }
        }

        #endregion

        #region 客户端接口

        /// <summary>
        /// 客户端注册消息
        /// </summary>
        /// <param name="handle"></param>
        /// <typeparam name="T"></typeparam>
        public void ClientRegisterMessage(int code, Action<MessageObject> handle)
        {
            _clientMessageDispatcherHelper.RegisterMessage(code, handle);
        }

        /// <summary>
        /// 客户端解绑消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ClientUnregisterMessage(int code, Action<MessageObject> handle)
        {
            _clientMessageDispatcherHelper.UnregisterMessage(code, handle);
        }

        /// <summary>
        /// 客户端发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
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

        /// <summary>
        /// 客户端连接
        /// </summary>
        /// <param name="unit"></param>
        public void AddUnit(MirrorUnitCtrl unit)
        {
            if (IsServer)
            {
                _unitDict.Add(unit.netId, unit);
            }

            if (unit.isLocalPlayer)
            {
                _localUnit = new NetworkUnit();
                _localUnit.Init(unit);
            }
        }


        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="unit"></param>
        public void RemoveUnit(MirrorUnitCtrl unit)
        {
            if (IsServer)
            {
                _unitDict.Remove(unit.netId);

                //如果是主机
                if (unit.isLocalPlayer)
                {
                    _unitDict.Clear();
                }
            }

            if (unit.isLocalPlayer)
            {
                _localUnit = null;
            }
        }

        #endregion
    }
}