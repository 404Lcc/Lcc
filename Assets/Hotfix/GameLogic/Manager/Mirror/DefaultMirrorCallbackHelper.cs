using System;
using UnityEngine;

namespace LccHotfix
{
    public class DefaultMirrorCallbackHelper : IMirrorCallbackHelper
    {
        /// <summary>
        /// 服务器启动成功
        /// </summary>
        /// <param name="onServerStartCallback"></param>
        public void OnServerStartCallback()
        {
            Debug.LogError("服务器启动成功");
        }

        /// <summary>
        /// 服务器停止
        /// </summary>
        /// <param name="onServerStopCallback"></param>
        public void OnServerStopCallback()
        {
            Debug.LogError("服务器停止");
        }

        /// <summary>
        /// 客户端连接成功
        /// </summary>
        /// <param name="onConnectedCallback"></param>
        public void OnClientConnectedCallback()
        {
            Debug.LogError("客户端连接成功");

            Debug.LogError("客户端发个消息");
            var mod = GameUtility.GetModel<ModMirrorTest>();
            mod.ClientSendCGTestInfo();
        }

        /// <summary>
        /// 客户端连接断开
        /// </summary>
        public void OnClientDisconnectedCallback()
        {
            Debug.LogError("客户端连接断开");
        }
    }
}