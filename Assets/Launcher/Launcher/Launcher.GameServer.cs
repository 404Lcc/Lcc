using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LccModel
{
    public partial class Launcher
    {
        //需要重新校验热更数据
        public bool reCheckVersionUpdate = false;

        //远程渠道
        public int svrChannel;
        //远程版本
        public int svrVersion;

        //远程推荐服
        public string svrLoginServer;
        //远程服务器列表
        public List<string> svrLoginServerList;
        //远程资源服
        public string svrResourceServerUrl;
        //远程资源版本号
        public int svrResVersion;
        //远程更包地址
        public string svrAppForceUpdateUrl;

        //公告地址
        public string noticeUrl;

        /// <summary>
        /// 是否成功请求中心服地址
        /// </summary>
        public bool requestCenterServerSucc;
        public IEnumerator RequestCenterServer(bool restart = false)
        {
            requestCenterServerSucc = false;

            string url = $"{GameConfig.centerServerAddress}/{(Launcher.GameConfig.isRelease ? "Release" : "Dev")}/{ResPath.PlatformDirectory}/channelList.txt";
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
                if (!restart)
                {
                    StartServerLoad();
                }
            }
            else
            {
                string response = web.downloadHandler.text;

                ReadChannelListConfig(response);

                yield return GetRemoteVersionList(restart);
            }
        }

        private IEnumerator GetRemoteVersionList(bool restart = false)
        {
            requestCenterServerSucc = false;

            string url = $"{GameConfig.centerServerAddress}/{(Launcher.GameConfig.isRelease ? "Release" : "Dev")}/{ResPath.PlatformDirectory}/versionList.txt";
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
                if (!restart)
                {
                    StartServerLoad();
                }
            }
            else
            {
                string response = web.downloadHandler.text;

                ReadVersionListConfig(response);




                requestCenterServerSucc = true;
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
                                if (GameConfig.channel == channel)
                                {
                                    this.svrChannel = channel;
                                    int defaultVersion = 0;
                                    int fetchVersion = 0;

                                    int appVersion = Launcher.GameConfig.appVersion;
                                    int resVersion = Launcher.GameConfig.resVersion;

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
                                        this.svrVersion = fetchVersion;
                                        Launcher.Instance.GameState = GameState.Fetch;
                                    }
                                    else
                                    {
                                        Debug.Log($"GetRemoteVersionList 判走普通包 svrVersion = 远端版本={defaultVersion}");
                                        this.svrVersion = defaultVersion;
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
                        if (svrVersion == clientVersion)
                        {
                            findClientVersion = true;

                            if (versionConfigList[i].ContainsKey("loginServer"))
                            {
                                svrLoginServer = versionConfigList[i]["loginServer"].ToString();
                            }
                            if (versionConfigList[i].ContainsKey("loginServerList"))
                            {
                                svrLoginServerList = JsonMapper.ToObject<List<string>>(versionConfigList[i]["loginServerList"].ToJson());
                            }
                            if (versionConfigList[i].ContainsKey("resourceServerUrl"))
                            {
                                svrResourceServerUrl = versionConfigList[i]["resourceServerUrl"].ToString();
                            }
                            if (versionConfigList[i].ContainsKey("resourceVersion"))
                            {
                                svrResVersion = int.Parse(versionConfigList[i]["resourceVersion"].ToString());
                            }
                            if (versionConfigList[i].ContainsKey("appForceUpdateUrl"))
                            {
                                svrAppForceUpdateUrl = versionConfigList[i]["appForceUpdateUrl"].ToString();
                            }
                            if (versionConfigList[i].ContainsKey("noticeUrl"))
                            {
                                noticeUrl = versionConfigList[i]["noticeUrl"].ToString();
                            }

                            Launcher.Instance.SetUpdateInfo(svrVersion, svrAppForceUpdateUrl, "");

                            break;
                        }
                    }
                }

                if (!findClientVersion)
                {
                    Debug.LogError($"ReadVersionListConfig !findClientVersion svrVersion={svrVersion}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("读取VersionListConfig配置失败" + ex.ToString());
            }
        }

        public string GetClientVersion()
        {
            var showApp = int.Parse(Application.version.Split('.')[0]);

            string clientVersion = string.Empty;
#if !UNITY_EDITOR
            clientVersion = "version " + showApp + "." + GameConfig.appVersion + "." + GameConfig.channel + "." + svrResVersion;
#else
            if (IsAuditServer() || !GameConfig.checkResUpdate)
                clientVersion = "version " + showApp + "." + GameConfig.appVersion + "." + GameConfig.channel + "." + svrResVersion;
            else
                clientVersion = "version " + showApp + "." + GameConfig.appVersion + "." + GameConfig.channel + "." + GameConfig.resVersion;
#endif
            return clientVersion;
        }

        public bool IsAuditServer()
        {
#if !UNITY_EDITOR
            if (Launcher.Instance.GameState == GameState.Official)
            {
                return false;
            }
            else
            {
                return true;

            }
#endif
            return false;
        }
    }
}