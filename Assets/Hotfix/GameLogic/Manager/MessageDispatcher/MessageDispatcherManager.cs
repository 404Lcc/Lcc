using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class MessageDispatcherManager : Module, IMessageDispatcherService
    {
        private Dictionary<int, List<Action<ProtobufObject>>> _handleDict = new Dictionary<int, List<Action<ProtobufObject>>>();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            _handleDict.Clear();
        }

        public void RegisterMessage(int code, Action<ProtobufObject> messageHandle)
        {
            if (messageHandle == null)
            {
                return;
            }

            if (!_handleDict.TryGetValue(code, out var messageHandleList))
            {
                messageHandleList = new List<Action<ProtobufObject>>();
                _handleDict[code] = messageHandleList;
            }

            if (messageHandleList != null)
            {
                if (!messageHandleList.Contains(messageHandle))
                {
                    messageHandleList.Add(messageHandle);
                }
                else
                {
                    Debug.Log(code + "消息处理器重新注册了");
                }
            }
        }

        public void UnregisterMessage(int code, Action<ProtobufObject> messageHandle)
        {
            if (!_handleDict.TryGetValue(code, out var messageHandleList))
            {
                return;
            }

            if (messageHandleList != null)
            {
                messageHandleList.Remove(messageHandle);
            }
        }


        public void DispatcherMessage(NetworkMessage message)
        {
            int code = message.code;

            Debug.LogWarning($"接收消息号 {code}");

            if (!_handleDict.TryGetValue(code, out var messageHandleList))
            {
                Debug.LogWarning($"消息没有处理 {code}");
                return;
            }

            for (int i = messageHandleList.Count - 1; i >= 0; i--)
            {
                var item = messageHandleList[i];
                try
                {
                    Debug.LogWarning($"消息处理 {code} i = {i}");
                    item(message.GetMessage<ProtobufObject>());
                }
                catch (Exception e)
                {
                    Debug.LogError($"消息处理失败 {code} i = {i}" + e);
                }
            }
        }
    }
}