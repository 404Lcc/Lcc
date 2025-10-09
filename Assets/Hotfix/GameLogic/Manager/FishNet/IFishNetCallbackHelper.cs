namespace LccHotfix
{
    public interface IFishNetCallbackHelper
    {
        /// <summary>
        /// 服务器启动成功
        /// </summary>
        void OnServerStartCallback();

        /// <summary>
        /// 服务器停止
        /// </summary>
        void OnServerStopCallback();

        /// <summary>
        /// 客户端连接成功
        /// </summary>
        void OnClientConnectedCallback();

        /// <summary>
        /// 客户端连接断开
        /// </summary>
        void OnClientDisconnectedCallback();

        /// <summary>
        /// 远程客户端连接断开
        /// </summary>
        void OnServerRemoteClientDisconnectedCallback(int connectionId);
    }
}