using System;
using UnityEngine;

namespace LccHotfix
{
    public class DefaultMirrorCallbackHelper : IMirrorCallbackHelper
    {
        /// <summary>
        /// 服务器启动成功
        /// </summary>
        public void OnServerStartCallback()
        {
            Debug.Log("服务器启动成功");
        }

        /// <summary>
        /// 服务器停止
        /// </summary>
        public void OnServerStopCallback()
        {
            Debug.Log("服务器停止");
        }

        /// <summary>
        /// 客户端连接成功
        /// </summary>
        public void OnClientConnectedCallback()
        {
            Debug.Log("客户端连接成功");

            Debug.Log("客户端发个消息");

            var mod = GameUtility.GetModel<ModMirrorTest>();
            mod.ClientSendCGTestInfo();
        }

        /// <summary>
        /// 客户端连接断开
        /// </summary>
        public void OnClientDisconnectedCallback()
        {
            Debug.Log("客户端连接断开");
        }
    }
}