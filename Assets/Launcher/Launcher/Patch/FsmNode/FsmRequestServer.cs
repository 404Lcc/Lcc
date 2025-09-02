using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public class FsmRequestServer : IStateNode
    {
        private StateMachine _machine;
        
        public void OnCreate(StateMachine machine)
        {
            _machine = machine;
        }

        public void OnEnter()
        {
            Launcher.Instance.StartCoroutine(RequestServer());
        }

        private IEnumerator RequestServer()
        {
            UILoadingPanel.Instance.Show(Launcher.Instance.GameLanguage.GetLanguage("msg_retrieve_server_data"));
            Launcher.Instance.GameControl.ChangeFPS();
            UILoadingPanel.Instance.UpdateLoadingPercent(0, 3);
            yield return null;

            UILoadingPanel.Instance.UpdateLoadingPercent(4, 20, 0.5f);
            yield return RequestCenterServer();

            if (Launcher.Instance.GameServerConfig.status != RequestServerStatus.Succeed)
            {
                yield break;
            }

            UILoadingPanel.Instance.UpdateLoadingPercent(19, 20);

            //检测是否需要重新下载安装包
            if (CheckIfAppShouldUpdate())
            {
                Debug.Log($"初始化 需要重新下载安装包 GameConfig.appVersion:{Launcher.Instance.GameConfig.appVersion}, svrVersion:{Launcher.Instance.GameServerConfig.svrVersion}");
                ForceUpdate();
                yield break;
            }

            UILoadingPanel.Instance.UpdateLoadingPercent(21, 40);
            //读取本地版本信息
            if (Launcher.Instance.GameConfig.checkResUpdate && !Launcher.Instance.IsAuditServer())
            {
                Launcher.Instance.GameConfig.AddConfig("resVersion", Launcher.Instance.GameServerConfig.svrResVersion);
            }
            UILoadingPanel.Instance.UpdateLoadingPercent(41, 50);
            yield return null;
            
            _machine.ChangeState<FsmGetNotice>();
        }

        public IEnumerator RequestCenterServer()
        {
#if Offline
            Launcher.Instance.GameServerConfig.status = RequestServerStatus.Succeed;
            yield break;
#endif
            Launcher.Instance.GameServerConfig.status = RequestServerStatus.None;

            string url = $"{Launcher.Instance.GameConfig.centerServerAddress}/{(Launcher.Instance.GameConfig.isRelease ? "Release" : "Dev")}/{ResPath.PlatformDirectory}/channelList.txt";
            Debug.Log("RequestCenterServer=" + url);

            UnityWebRequest web = UnityWebRequest.Get(url);
            web.SetRequestHeader("pragma", "no-cache");
            web.SetRequestHeader("Cache-Control", "no-cache");
#if UNITY_EDITOR
            web.timeout = 2;
#else
		    web.timeout = 20;
#endif

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
                Debug.LogError($"RequestCenterServer 请求中心服 失败url= {url}");
                PatchEventDefine.RequestServerFailed.SendEventMessage();
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
            string url = $"{Launcher.Instance.GameConfig.centerServerAddress}/{(Launcher.Instance.GameConfig.isRelease ? "Release" : "Dev")}/{ResPath.PlatformDirectory}/versionList.txt";
            Debug.Log("GetRemoteVersionList=" + url);

            UnityWebRequest web = UnityWebRequest.Get(url);
            web.SetRequestHeader("pragma", "no-cache");
            web.SetRequestHeader("Cache-Control", "no-cache");
#if UNITY_EDITOR
            web.timeout = 2;
#else
		    web.timeout = 20;
#endif

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
                PatchEventDefine.RequestServerFailed.SendEventMessage();
            }
            else
            {
                string response = web.downloadHandler.text;

                ReadVersionListConfig(response);
                
                Launcher.Instance.GameServerConfig.status = RequestServerStatus.Succeed;
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
                        for (int i = 0; i < channelConfigList.Count; i++)
                        {
                            if (channelConfigList[i].ContainsKey("channel"))
                            {
                                var channel = int.Parse(channelConfigList[i]["channel"].ToString());
                                if (Launcher.Instance.GameConfig.channel == channel)
                                {
                                    Launcher.Instance.GameServerConfig.svrChannel = channel;
                                    int defaultVersion = 0;
                                    int fetchVersion = 0;

                                    int appVersion = Launcher.Instance.GameConfig.appVersion;
                                    int resVersion = Launcher.Instance.GameConfig.resVersion;

                                    if (channelConfigList[i].ContainsKey("defaultVersion"))
                                    {
                                        defaultVersion = int.Parse(channelConfigList[i]["defaultVersion"].ToString());
                                    }
                                    if (channelConfigList[i].ContainsKey("fetchVersion"))
                                    {
                                        fetchVersion = int.Parse(channelConfigList[i]["fetchVersion"].ToString());
                                    }

                                    //如果版本号跟提审号的一样就走提审包，其他情况走默认的
                                    if (appVersion == fetchVersion)
                                    {
                                        Debug.Log($"GetRemoteVersionList 判定走提审包 svrVersion = 远端版本={fetchVersion}");
                                        Launcher.Instance.GameServerConfig.svrVersion = fetchVersion;
                                        Launcher.Instance.GameState = GameState.Fetch;
                                    }
                                    else
                                    {
                                        Debug.Log($"GetRemoteVersionList 判走普通包 svrVersion = 远端版本={defaultVersion}");
                                        Launcher.Instance.GameServerConfig.svrVersion = defaultVersion;
                                        Launcher.Instance.GameState = GameState.Official;
                                    }

                                    break;
                                }

                            }
                        }
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
                bool findClientVersion = false;
                for (int i = 0; i < versionConfigList.Count; i++)
                {
                    if (versionConfigList[i].ContainsKey("clientVersion"))
                    {
                        var clientVersion = int.Parse(versionConfigList[i]["clientVersion"].ToString());
                        if (Launcher.Instance.GameServerConfig.svrVersion == clientVersion)
                        {
                            findClientVersion = true;

                            if (versionConfigList[i].ContainsKey("loginServer"))
                            {
                                Launcher.Instance.GameServerConfig.svrLoginServer = versionConfigList[i]["loginServer"].ToString();
                            }
                            if (versionConfigList[i].ContainsKey("loginServerList"))
                            {
                                Launcher.Instance.GameServerConfig.svrLoginServerList = JsonMapper.ToObject<List<string>>(versionConfigList[i]["loginServerList"].ToJson());
                            }
                            if (versionConfigList[i].ContainsKey("resourceServerUrl"))
                            {
                                Launcher.Instance.GameServerConfig.svrResourceServerUrl = versionConfigList[i]["resourceServerUrl"].ToString();
                            }
                            if (versionConfigList[i].ContainsKey("resourceVersion"))
                            {
                                Launcher.Instance.GameServerConfig.svrResVersion = int.Parse(versionConfigList[i]["resourceVersion"].ToString());
                            }
                            if (versionConfigList[i].ContainsKey("appForceUpdateUrl"))
                            {
                                Launcher.Instance.GameServerConfig.svrAppForceUpdateUrl = versionConfigList[i]["appForceUpdateUrl"].ToString();
                            }
                            if (versionConfigList[i].ContainsKey("noticeUrl"))
                            {
                                Launcher.Instance.GameServerConfig.noticeUrl = versionConfigList[i]["noticeUrl"].ToString();
                            }

                            break;
                        }
                    }
                }

                if (!findClientVersion)
                {
                    Debug.LogError($"ReadVersionListConfig !findClientVersion svrVersion={Launcher.Instance.GameServerConfig.svrVersion}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("读取VersionListConfig配置失败" + ex.ToString());
            }
        }
        
        private void UpdateNewVersion()
        {
            Application.OpenURL(Launcher.Instance.GameServerConfig.svrAppForceUpdateUrl);
        }

        public void ForceUpdate()
        {
            UILoadingPanel.Instance.ShowMessageBox(Launcher.Instance.GameLanguage.GetLanguage("msg_update"), () =>
            {
                UpdateNewVersion();
            }, false);
        }


        /// <summary>
        /// 判断是否要重新安装
        /// </summary>
        /// <returns></returns>
        public bool CheckIfAppShouldUpdate()
        {
            if (string.IsNullOrEmpty(Launcher.Instance.GameServerConfig.svrAppForceUpdateUrl))
                return false;

            return Launcher.Instance.GameConfig.appVersion != Launcher.Instance.GameServerConfig.svrVersion;
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }
    }
}