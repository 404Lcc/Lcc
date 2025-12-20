using LitJson;
using System;
using UnityEngine;

namespace LccModel
{
    public partial class GameConfig
    {
        public int appVersion
        {
            get
            {
                return GetConfig<int>("appVersion");
            }
        }
        public int channel
        {
            get
            {
                return GetConfig<int>("channel");
            }
        }

        public int resVersion
        {
            get
            {
                return GetConfig<int>("resVersion");
            }
        }


        public string centerServerAddress
        {
            get
            {
                return GetConfig<string>("centerServerAddress");
            }
        }


        public bool isReleaseCenterServer
        {
            get
            {
                return GetConfig<bool>("isReleaseCenterServer");
            }
        }

        public bool isRelease
        {
            get
            {

                return GetConfig<bool>("isRelease");
            }
        }

        public bool chargeDirect
        {
            get
            {
                return GetConfig<bool>("chargeDirect");
            }
        }

        public bool selectServer
        {
            get
            {
                return GetConfig<bool>("selectServer");
            }
        }

        public bool checkResUpdate
        {
            get
            {
                return GetConfig<bool>("checkResUpdate");
            }
        }

        public bool useSDK
        {
            get
            {
#if UNITY_EDITOR
                return false;
#else
                return GetConfig<bool>("useSDK");
#endif
            }
        }


        public void ReadVersion(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    JsonData data = JsonMapper.ToObject(text);
                    if (data.ContainsKey("appVersion"))
                    {
                        AddConfig("appVersion", int.Parse(data["appVersion"].ToString()));
                    }
                    if (data.ContainsKey("channel"))
                    {
                        AddConfig("channel", int.Parse(data["channel"].ToString()));
                    }
                    if (data.ContainsKey("resVersion"))
                    {
                        AddConfig("resVersion", int.Parse(data["resVersion"].ToString()));
                    }
                    if (data.ContainsKey("centerServerAddress"))
                    {
                        AddConfig("centerServerAddress", data["centerServerAddress"].ToString());
                    }
                    if (data.ContainsKey("isReleaseCenterServer"))
                    {
                        AddConfig("isReleaseCenterServer", bool.Parse(data["isReleaseCenterServer"].ToString()));
                    }
                    if (data.ContainsKey("isRelease"))
                    {
                        AddConfig("isRelease", bool.Parse(data["isRelease"].ToString()));
                    }
                    if (data.ContainsKey("chargeDirect"))
                    {
                        AddConfig("chargeDirect", bool.Parse(data["chargeDirect"].ToString()));
                    }
                    if (data.ContainsKey("selectServer"))
                    {
                        AddConfig("selectServer", bool.Parse(data["selectServer"].ToString()));
                    }
                    if (data.ContainsKey("checkResUpdate"))
                    {
                        AddConfig("checkResUpdate", bool.Parse(data["checkResUpdate"].ToString()));
                    }
                    if (data.ContainsKey("useSDK"))
                    {
                        AddConfig("useSDK", bool.Parse(data["useSDK"].ToString()));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("读取版本配置失败：" + ex.ToString());
                }
            }
            else
            {
            }
        }
    }
}