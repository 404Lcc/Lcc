using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace LccHotfix
{
    public class MirrorServerPBMessageDispatcherHelper : IMirrorServerMessageDispatcherHelper
    {
        private Dictionary<int, List<Action<NetworkConnectionToClient, MessageObject>>> _handleDict = new Dictionary<int, List<Action<NetworkConnectionToClient, MessageObject>>>();

        public void RegisterMessage(int code, Action<NetworkConnectionToClient, MessageObject> handle)
        {
            if (handle == null)
            {
                return;
            }

            if (!_handleDict.TryGetValue(code, out var handleList))
            {
                handleList = new List<Action<NetworkConnectionToClient, MessageObject>>();
                _handleDict[code] = handleList;
            }

            if (handleList != null)
            {
                if (!handleList.Contains(handle))
                {
                    handleList.Add(handle);
                }
                else
                {
                    Debug.Log($"存在相同的handle 消息号{(MessageType)code}");
                }
            }
        }

        public void UnregisterMessage(int code, Action<NetworkConnectionToClient, MessageObject> handle)
        {
            if (!_handleDict.TryGetValue(code, out var handleList))
            {
                return;
            }

            if (handleList != null)
            {
                handleList.Remove(handle);
            }
        }


        public void DispatcherMessage(NetworkConnectionToClient client, NetworkMessage message)
        {
            var code = message.code;

            if (!_handleDict.TryGetValue(code, out var handleList))
            {
                Debug.LogWarning($"消息号{(MessageType)code} 没有处理");
                return;
            }

            for (int i = handleList.Count - 1; i >= 0; i--)
            {
                var item = handleList[i];
                try
                {
                    item(client, message.GetMessage<MessageObject>());
                }
                catch (Exception e)
                {
                    Debug.LogError($"消息号{(MessageType)code} 处理失败" + e.Message);
                }
            }
        }
    }
}