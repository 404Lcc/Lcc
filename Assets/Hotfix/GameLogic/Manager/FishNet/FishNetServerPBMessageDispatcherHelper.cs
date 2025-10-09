using System;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

namespace LccHotfix
{
    public class FishNetServerPBMessageDispatcherHelper : IFishNetServerMessageDispatcherHelper
    {
        private Dictionary<int, List<Action<NetworkConnection, MessageObject>>> _handleDict = new Dictionary<int, List<Action<NetworkConnection, MessageObject>>>();

        public void RegisterMessage(int code, Action<NetworkConnection, MessageObject> handle)
        {
            if (handle == null)
            {
                return;
            }

            if (!_handleDict.TryGetValue(code, out var handleList))
            {
                handleList = new List<Action<NetworkConnection, MessageObject>>();
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

        public void UnregisterMessage(int code, Action<NetworkConnection, MessageObject> handle)
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


        public void DispatcherMessage(NetworkConnection client, NetworkMessage message)
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