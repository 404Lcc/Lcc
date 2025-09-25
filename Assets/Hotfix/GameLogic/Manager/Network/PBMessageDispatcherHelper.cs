using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class PBMessageDispatcherHelper : IMessageDispatcherHelper
    {
        private Dictionary<int, List<Action<MessageObject>>> _handleDict = new Dictionary<int, List<Action<MessageObject>>>();

        public void RegisterMessage(int code, Action<MessageObject> handle)
        {
            if (handle == null)
            {
                return;
            }

            if (!_handleDict.TryGetValue(code, out var handleList))
            {
                handleList = new List<Action<MessageObject>>();
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

        public void UnregisterMessage(int code, Action<MessageObject> handle)
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


        public void DispatcherMessage(NetworkMessage message)
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
                    item(message.GetMessage<MessageObject>());
                }
                catch (Exception e)
                {
                    Debug.LogError($"消息号{(MessageType)code} 处理失败" + e.Message);
                }
            }
        }
    }
}