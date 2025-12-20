using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public class FsmRequestServer : FsmLaunchStateNode
    {
        private StateMachine _machine;
        
        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        public void OnEnter()
        {
            StartCoroutine(RequestServer());
        }

        private IEnumerator RequestServer()
        {
            yield return RequestCenterServer();
        }

        public IEnumerator RequestCenterServer()
        {
#if !Offline
            yield break;
#endif
            string url = $"{GameConfig.CenterServerAddress}/{(GameConfig.IsRelease ? "Release" : "Dev")}/{ResPath.PlatformDirectory}/channelList.txt";
            Debug.Log("RequestCenterServer=" + url);

            UnityWebRequest web = UnityWebRequest.Get(url);
            web.SetRequestHeader("pragma", "no-cache");
            web.SetRequestHeader("Cache-Control", "no-cache");
            web.timeout = 20;

            yield return web.SendWebRequest();
            if (!string.IsNullOrEmpty(web.error))
            {
                web.Dispose();
                web = UnityWebRequest.Get(url);
                web.SetRequestHeader("pragma", "no-cache");
                web.SetRequestHeader("Cache-Control", "no-cache");
                web.timeout = 20;
                yield return web.SendWebRequest();
            }
            if (!string.IsNullOrEmpty(web.error))
            {
                Debug.LogError($"RequestCenterServer 请求中心服 失败url= {url}");
            }
            else
            {
                string response = web.downloadHandler.text;

                ReadChannelListConfig(response);

                yield return GetRemoteVersionList();
            }
        }

        private IEnumerator GetRemoteVersionList()
        {
            string url = $"{GameConfig.CenterServerAddress}/{(GameConfig.IsRelease ? "Release" : "Dev")}/{ResPath.PlatformDirectory}/versionList.txt";
            Debug.Log("GetRemoteVersionList=" + url);

            UnityWebRequest web = UnityWebRequest.Get(url);
            web.SetRequestHeader("pragma", "no-cache");
            web.SetRequestHeader("Cache-Control", "no-cache");
            web.timeout = 20;

            yield return web.SendWebRequest();

            if (!string.IsNullOrEmpty(web.error))
            {
                web.Dispose();
                web = UnityWebRequest.Get(url);
                web.timeout = 20;
                yield return web.SendWebRequest();
            }

            if (!string.IsNullOrEmpty(web.error))
            {
                Debug.LogError($"GetRemoteVersionList 请求VersionList 失败url= {url}");
            }
            else
            {
                string response = web.downloadHandler.text;
                ReadVersionListConfig(response);
            }

        }

        public void ReadChannelListConfig(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    JsonData data = JsonMapper.ToObject(text);
                    if (data.ContainsKey("list"))
                    {
                        JsonData channelConfigList = data["list"];
                        PatchConfig.version = JsonUtility.ToObject<Version>(data["list"].ToJson());
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("读取VersionListConfig配置失败" + ex.ToString());
                }
            }
        }
        public void ReadVersionListConfig(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogError("ReadVersionListConfig text = null");
                return;
            }

            try
            {
                JsonData data = JsonMapper.ToObject(text);
                if (!data.ContainsKey("list"))
                {
                    Debug.LogError("ReadVersionListConfig !data.ContainsKey(list)");
                    return;
                }

                JsonData versionConfigList = data["list"];
                for (int i = 0; i < versionConfigList.Count; i++)
                {
                    if (versionConfigList[i].ContainsKey("clientVersion"))
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("读取VersionListConfig配置失败" + ex.ToString());
            }
        }
    }
}