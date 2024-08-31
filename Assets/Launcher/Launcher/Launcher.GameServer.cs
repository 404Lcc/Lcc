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
        public int mSvrChannel;
        //远程版本
        public int mSvrVersion;

        //远程推荐服
        public string mSvrLoginServer;
        //远程服务器列表
        public List<string> mSvrLoginServerList;
        //远程资源服
        public string mSvrResourceServerUrl;
        //远程资源版本号
        public int mSvrResVersion;
        //远程更包地址
        public string mSvrAppForceUpdateUrl;

        //公告地址
        public string noticeUrl;

        private int index = 0;
        private List<string> centerServerAddressLsit = new List<string>();
        private string GetCenterServerAddress()
        {
            centerServerAddressLsit.Clear();
            var urls = GameConfig.centerServerAddress.Split('#');
            foreach (var item in urls)
            {
                centerServerAddressLsit.Add(item);
            }
            if (index >= centerServerAddressLsit.Count)
            {
                index = 0;
            }
            var url = centerServerAddressLsit[index];
            index++;
            return url;
        }
        private void ResetCenterServerAddress()
        {
            index = 0;
        }

        /// <summary>
        /// 是否成功请求中心服地址
        /// </summary>
        public bool RequestCenterServerSucc;
        public IEnumerator RequestCenterServer()
        {
            RequestCenterServerSucc = false;

            string url = $"{GetCenterServerAddress()}/{(Launcher.GameConfig.isRelease ? "Release" : "Dev")}/{Launcher.Instance.GetPlatform()}/channelList.txt";
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
                Debug.LogError($"RequestCenterServer 请求中心服 失败url= {url} index = {index - 1}");
                StartServerLoad();
            }
            else
            {
                string response = web.downloadHandler.text;

                ReadChannelListConfig(response);

                ResetCenterServerAddress();

                yield return GetRemoteVersionList();
            }
        }

        private IEnumerator GetRemoteVersionList()
        {
            RequestCenterServerSucc = false;

            string url = $"{GetCenterServerAddress()}/{(Launcher.GameConfig.isRelease ? "Release" : "Dev")}/{Launcher.Instance.GetPlatform()}/versionList.txt";
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
                Debug.LogError($"GetRemoteVersionList 请求VersionList 失败url= {url} index = {index - 1}");
                StartServerLoad();
            }
            else
            {
                string response = web.downloadHandler.text;

                ReadVersionListConfig(response);

                ResetCenterServerAddress();

                yield return Launcher.Instance.GetNoticeBoard();
                yield return Launcher.Instance.GetNotice();


                RequestCenterServerSucc = true;
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
                                    this.mSvrChannel = channel;
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
                                        Debug.Log($"GetRemoteVersionList 判定走提审包 mSvrVersion = 远端版本={fetchVersion}");
                                        this.mSvrVersion = fetchVersion;
                                        Launcher.Instance.GameState = GameState.Fetch;
                                    }
                                    else
                                    {
                                        Debug.Log($"GetRemoteVersionList 判走普通包 mSvrVersion = 远端版本={defaultVersion}");
                                        this.mSvrVersion = defaultVersion;
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
                        if (mSvrVersion == clientVersion)
                        {
                            findClientVersion = true;

                            if (versionConfigList[i].ContainsKey("loginServer"))
                            {
                                mSvrLoginServer = versionConfigList[i]["loginServer"].ToString();
                            }
                            if (versionConfigList[i].ContainsKey("loginServerList"))
                            {
                                mSvrLoginServerList = JsonMapper.ToObject<List<string>>(versionConfigList[i]["loginServerList"].ToJson());
                            }
                            if (versionConfigList[i].ContainsKey("resourceServerUrl"))
                            {
                                mSvrResourceServerUrl = versionConfigList[i]["resourceServerUrl"].ToString();
                            }
                            if (versionConfigList[i].ContainsKey("resourceVersion"))
                            {
                                mSvrResVersion = int.Parse(versionConfigList[i]["resourceVersion"].ToString());
                            }
                            if (versionConfigList[i].ContainsKey("appForceUpdateUrl"))
                            {
                                mSvrAppForceUpdateUrl = versionConfigList[i]["appForceUpdateUrl"].ToString();
                            }
                            if (versionConfigList[i].ContainsKey("noticeUrl"))
                            {
                                noticeUrl = versionConfigList[i]["noticeUrl"].ToString();
                            }

                            Launcher.Instance.SetUpdateInfo(mSvrVersion, mSvrAppForceUpdateUrl, "");

                            break;
                        }
                    }
                }

                if (!findClientVersion)
                {
                    Debug.LogError($"ReadVersionListConfig !findClientVersion mSvrVersion={mSvrVersion}");
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
            clientVersion = "version " + showApp + "." + GameConfig.appVersion + "." + GameConfig.channel + "." + mSvrResVersion;
#else
            if (IsAuditServer() || !GameConfig.checkResUpdate)
                clientVersion = "version " + showApp + "." + GameConfig.appVersion + "." + GameConfig.channel + "." + mSvrResVersion;
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