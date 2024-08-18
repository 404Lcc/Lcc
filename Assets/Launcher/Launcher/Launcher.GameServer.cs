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


        /// <summary>
        /// 是否成功请求中心服地址
        /// </summary>
        public bool RequestCenterServerSucc;
        public IEnumerator RequestCenterServer()
        {
            RequestCenterServerSucc = false;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                UILoadingPanel.Instance.ShowMessageBox(GetLanguage("msg_retry_after_internet"), StartServerLoad);
                yield break;
            }


            string url = $"{GameConfig.centerServerAddress}/{(GameConfig.isRelease ? "Release" : "Dev")}/{GetPlatform()}/channelList.txt";
            Debug.Log("RequestCenterServer url=" + url);
            UnityWebRequest uwr = UnityWebRequest.Get(url);

            var send = uwr.SendWebRequest();
            //避免unity Curl error 28 错误
            yield return send;

            if (uwr.isDone)
            {
                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    string json = uwr.downloadHandler.text;

                    ReadChannelListConfig(json);

                    uwr.Dispose();

                    yield return GetRemoteVersionList();
                }
                else
                {
                    uwr.Dispose();

                    UILoadingPanel.Instance.ShowMessageBox(GetLanguage("msg_retrieve_server_data"), StartServerLoad);
                }
            }
            else
            {
                uwr.Dispose();
                UILoadingPanel.Instance.ShowMessageBox(GetLanguage("msg_retrieve_server_data"), StartServerLoad);
            }
        }

        private IEnumerator GetRemoteVersionList()
        {
            RequestCenterServerSucc = false;

            string url = $"{GameConfig.centerServerAddress}/{(GameConfig.isRelease ? "Release" : "Dev")}/{GetPlatform()}/versionList.txt";
            Debug.Log("GetRemoteVersionList url=" + url);
            UnityWebRequest uwr = UnityWebRequest.Get(url);

            var send = uwr.SendWebRequest();
            //避免unity Curl error 28 错误
            yield return send;

            if (uwr.isDone)
            {
                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    string json = uwr.downloadHandler.text;

                    ReadVersionListConfig(json);

                    yield return GetNoticeBoard();
                    yield return GetNotice();


                    RequestCenterServerSucc = true;


                    uwr.Dispose();
                }
                else
                {
                    uwr.Dispose();
                    UILoadingPanel.Instance.ShowMessageBox(GetLanguage("msg_retrieve_server_data"), StartServerLoad);
                }

            }
            else
            {
                uwr.Dispose();
                UILoadingPanel.Instance.ShowMessageBox(GetLanguage("msg_retrieve_server_data"), StartServerLoad);
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

                                    int appVersion = GameConfig.appVersion;
                                    int resVersion = GameConfig.resVersion;

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
                                        Debug.Log($"GetRemoteVersionList 判定走提审包 mSvrVersion = {fetchVersion}");
                                        this.mSvrVersion = fetchVersion;
                                        GameState = GameState.Fetch;
                                    }
                                    else
                                    {
                                        Debug.Log($"GetRemoteVersionList 判走普通包 mSvrVersion = {defaultVersion}");
                                        this.mSvrVersion = defaultVersion;
                                        GameState = GameState.Official;
                                    }

                                    break;
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("读取VersionListConfig配置失败：" + ex.ToString());
                }
            }
        }
        public void ReadVersionListConfig(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogError("ReadVersionListConfig text IsNullOrEmpty");
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

                            SetUpdateInfo(mSvrVersion, mSvrAppForceUpdateUrl, "");

                            break;
                        }
                    }
                }

                if (!findClientVersion)
                {
                    Debug.LogError($"ReadVersionListConfig !findClientVersion  mSvrVersion={mSvrVersion}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("读取VersionListConfig配置失败：" + ex.ToString());
            }
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