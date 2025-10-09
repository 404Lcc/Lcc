using UnityEngine;

namespace LccHotfix
{
    public enum FishNetType
    {
        ServerStart,
        ServerStop,
        ClientConnected,
        ClientDisconnected,
    }

    public struct FishNetValueEvent : IValueEvent
    {
        public FishNetType type;
    }

    public struct FishNetServerRemoteClientDisconnectedCallback : IValueEvent
    {
        public int connectionId;
    }


    public class DefaultFishNetCallbackHelper : IFishNetCallbackHelper
    {
        /// <summary>
        /// 服务器启动成功
        /// </summary>
        public void OnServerStartCallback()
        {
            Debug.Log("服务器启动成功");
            FishNetValueEvent evt = new FishNetValueEvent();
            evt.type = FishNetType.ServerStart;
            GameUtility.Dispatch(evt);
        }

        /// <summary>
        /// 服务器停止
        /// </summary>
        public void OnServerStopCallback()
        {
            Debug.Log("服务器停止");
            FishNetValueEvent evt = new FishNetValueEvent();
            evt.type = FishNetType.ServerStop;
            GameUtility.Dispatch(evt);
        }

        /// <summary>
        /// 客户端连接成功
        /// </summary>
        public void OnClientConnectedCallback()
        {
            Debug.Log("客户端连接成功");
            FishNetValueEvent evt = new FishNetValueEvent();
            evt.type = FishNetType.ClientConnected;
            GameUtility.Dispatch(evt);
        }

        /// <summary>
        /// 客户端连接断开
        /// </summary>
        public void OnClientDisconnectedCallback()
        {
            Debug.Log("客户端连接断开");
            FishNetValueEvent evt = new FishNetValueEvent();
            evt.type = FishNetType.ClientDisconnected;
            GameUtility.Dispatch(evt);
        }

        /// <summary>
        /// 远程客户端连接断开
        /// </summary>
        /// <param name="connectionId"></param>
        public void OnServerRemoteClientDisconnectedCallback(int connectionId)
        {
            Debug.Log("客户端连接断开" + connectionId);
            FishNetServerRemoteClientDisconnectedCallback evt = new FishNetServerRemoteClientDisconnectedCallback();
            evt.connectionId = connectionId;
            GameUtility.Dispatch(evt);
        }
    }
}