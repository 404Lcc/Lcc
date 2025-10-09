using UnityEngine;

namespace LccHotfix
{
    public enum MirrorType
    {
        ServerStart,
        ServerStop,
        ClientConnected,
        ClientDisconnected,
    }

    public struct MirrorValueEvent : IValueEvent
    {
        public MirrorType type;
    }

    public struct MirrorServerRemoteClientDisconnectedCallback : IValueEvent
    {
        public int connectionId;
    }


    public class DefaultMirrorCallbackHelper : IMirrorCallbackHelper
    {
        /// <summary>
        /// 服务器启动成功
        /// </summary>
        public void OnServerStartCallback()
        {
            Debug.Log("服务器启动成功");
            MirrorValueEvent evt = new MirrorValueEvent();
            evt.type = MirrorType.ServerStart;
            GameUtility.Dispatch(evt);
        }

        /// <summary>
        /// 服务器停止
        /// </summary>
        public void OnServerStopCallback()
        {
            Debug.Log("服务器停止");
            MirrorValueEvent evt = new MirrorValueEvent();
            evt.type = MirrorType.ServerStop;
            GameUtility.Dispatch(evt);
        }

        /// <summary>
        /// 客户端连接成功
        /// </summary>
        public void OnClientConnectedCallback()
        {
            Debug.Log("客户端连接成功");
            MirrorValueEvent evt = new MirrorValueEvent();
            evt.type = MirrorType.ClientConnected;
            GameUtility.Dispatch(evt);
        }

        /// <summary>
        /// 客户端连接断开
        /// </summary>
        public void OnClientDisconnectedCallback()
        {
            Debug.Log("客户端连接断开");
            MirrorValueEvent evt = new MirrorValueEvent();
            evt.type = MirrorType.ClientDisconnected;
            GameUtility.Dispatch(evt);
        }

        /// <summary>
        /// 远程客户端连接断开
        /// </summary>
        /// <param name="connectionId"></param>
        public void OnServerRemoteClientDisconnectedCallback(int connectionId)
        {
            Debug.Log("客户端连接断开" + connectionId);
            MirrorServerRemoteClientDisconnectedCallback evt = new MirrorServerRemoteClientDisconnectedCallback();
            evt.connectionId = connectionId;
            GameUtility.Dispatch(evt);
        }
    }
}