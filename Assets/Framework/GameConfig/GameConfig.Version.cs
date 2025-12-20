using LitJson;
using System;
using UnityEngine;

namespace LccModel
{
    public static partial class GameConfig
    {
        public static string AppVersion
        {
            get { return GetConfig<string>("appVersion"); }
            set { AddConfig("appVersion", value); }
        }

        public static int Channel
        {
            get { return GetConfig<int>("channel"); }
            set { AddConfig("channel", value); }
        }

        public static string LocalPackageVersion
        {
            get { return GetConfig<string>("localPackageVersion"); }
            set { AddConfig("localPackageVersion", value); }
        }


        public static string CenterServerAddress
        {
            get { return GetConfig<string>("centerServerAddress"); }
        }


        public static bool IsReleaseCenterServer
        {
            get { return GetConfig<bool>("isReleaseCenterServer"); }
        }

        public static bool IsRelease
        {
            get { return GetConfig<bool>("isRelease"); }
        }

        public static bool IsEnablePatcher
        {
            get { return GetConfig<bool>("isEnablePatcher"); }
        }

        public static bool IsEnableSDK
        {
            get
            {
#if UNITY_EDITOR
                return false;
#else
                return GetConfig<bool>("isEnableSDK");
#endif
            }
        }


        public static void ReadVersion(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    JsonData data = JsonMapper.ToObject(text);
                    if (data.ContainsKey("appVersion"))
                    {
                        AddConfig("appVersion", data["appVersion"].ToString());
                    }

                    if (data.ContainsKey("channel"))
                    {
                        AddConfig("channel", int.Parse(data["channel"].ToString()));
                    }

                    if (data.ContainsKey("localPackageVersion"))
                    {
                        AddConfig("localPackageVersion", data["localPackageVersion"].ToString());
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

                    if (data.ContainsKey("isEnablePatcher"))
                    {
                        AddConfig("isEnablePatcher", bool.Parse(data["isEnablePatcher"].ToString()));
                    }

                    if (data.ContainsKey("isEnableSDK"))
                    {
                        AddConfig("isEnableSDK", bool.Parse(data["isEnableSDK"].ToString()));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("读取版本配置失败：" + ex.ToString());
                }
            }
        }
        
        public static string GetVersionStr()
        {
            return $"{AppVersion}.{LocalPackageVersion}";
        }
    }
}