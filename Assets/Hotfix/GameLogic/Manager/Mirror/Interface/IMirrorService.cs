using System;
using LccModel;
using Mirror;

namespace LccHotfix
{
    public interface IMirrorService : IService
    {
        /// <summary>
        /// 设置IP
        /// </summary>
        /// <param name="networkAddress"></param>
        public void SetNetworkAddress(string networkAddress = "");

        /// <summary>
        /// 设置端口
        /// </summary>
        /// <param name="port"></param>
        public void SetPort(int port = 7788);

        void SetServerMessageDispatcherHelper(IMirrorServerMessageDispatcherHelper messageDispatcherHelper);
        void SetClientMessageDispatcherHelper(IMessageDispatcherHelper messageDispatcherHelper);
        public bool IsNetworkActive();

        /// <summary>
        /// 开启主机
        /// </summary>
        public void StartHost();

        /// <summary>
        /// 停止主机
        /// </summary>
        public void StopHost();

        /// <summary>
        /// 客户端开始连接
        /// </summary>
        public void Connect();

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public void Disconnect();

        #region 客户端接口

        /// <summary>
        /// 客户端注册消息
        /// </summary>
        /// <param name="handle"></param>
        public void ClientRegisterMessage(int code, Action<MessageObject> handle);

        /// <summary>
        /// 客户端解绑消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ClientUnregisterMessage(int code, Action<MessageObject> handle);

        /// <summary>
        /// 客户端发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public void ClientSendMessage<T>(T message) where T : MessageObject;

        /// <summary>
        /// 客户端连接
        /// </summary>
        /// <param name="unit"></param>
        public void AddUnit(MirrorUnitCtrl unit);

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="unit"></param>
        public void RemoveUnit(MirrorUnitCtrl unit);

        #endregion

        #region 服务器接口

        /// <summary>
        /// 服务器注册消息
        /// </summary>
        /// <param name="handle"></param>
        public void ServerRegisterMessage(int code, Action<NetworkConnectionToClient, MessageObject> handle);

        /// <summary>
        /// 服务器解绑消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ServerUnregisterMessage(int code, Action<NetworkConnectionToClient, MessageObject> handle);

        /// <summary>
        /// 服务器发送消息
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public void ServerSendMessage<T>(NetworkConnectionToClient client, T message) where T : MessageObject;

        /// <summary>
        /// 服务器广播消息
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public void ServerBroadcastMessage<T>(T message) where T : MessageObject;

        #endregion
    }
}