using System;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace LccModel
{
    public class LanguageConfig
    {
        public string name;
        public string font;
        public string setName;
        public string shortName;
    }

    public partial class GameConfig
    {
        /// <summary>
        /// 默认语言
        /// </summary>
        public string defaultLanguage
        {
            get
            {
                return GetConfig<string>("defaultLanguage", "English");
            }
        }

        /// <summary>
        /// 当前平台支持的多语言
        /// </summary>
        public List<string> languageNameList
        {
            get
            {
                return GetConfig<List<string>>("languageNameList");
            }
        }

        /// <summary>
        /// 语言显示信息
        /// </summary>
        public Dictionary<string, LanguageConfig> languageDict
        {
            get
            {
                return GetConfig<Dictionary<string, LanguageConfig>>("languageDict");
            }
        }


        public Dictionary<string, SystemLanguage> languageShortNameDict
        {
            get
            {
                return GetConfig<Dictionary<string, SystemLanguage>>("languageShortNameDict");
            }
        }
        /// <summary>
        /// 当前地区
        /// </summary>
        public string region
        {
            get
            {
                return GetConfig<string>("region");
            }
        }
        /// <summary>
        /// 当前平台支持的地区货币
        /// </summary>
        public List<string> regionList
        {
            get
            {
                return GetConfig<List<string>>("regionList");
            }
        }
        public string GetLanguageShortName(SystemLanguage lang)
        {
            if (languageDict.TryGetValue(lang.ToString(), out var langData))
            {
                return langData.shortName;
            }
            return null;
        }
        public SystemLanguage GetLanguageName(string shortName)
        {
            if (languageShortNameDict.TryGetValue(shortName.ToString(), out var lang))
            {
                return lang;
            }
            return SystemLanguage.Unknown;
        }



        public void ReadLanguage(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    List<string> languageNameList = null;
                    Dictionary<string, LanguageConfig> languageDict = new Dictionary<string, LanguageConfig>();
                    Dictionary<string, SystemLanguage> languageShortNameDict = new Dictionary<string, SystemLanguage>();

                    JsonData data = JsonMapper.ToObject(text);
                    if (data.ContainsKey("language"))
                    {
                        JsonData languageData = data["language"];
                        if (languageData.ContainsKey("defaultLanguage"))
                        {
                            AddConfig("defaultLanguage", languageData["defaultLanguage"].ToString());
                        }

                        if (languageData.ContainsKey("languageList"))
                        {
                            var languageList = JsonMapper.ToObject<LanguageConfig[]>(languageData["languageList"].ToJson());
                            for (int i = 0; i < languageList.Length; i++)
                            {
                                languageDict.Add(languageList[i].name, languageList[i]);
                                languageShortNameDict.Add(languageList[i].shortName, (SystemLanguage)Enum.Parse(typeof(SystemLanguage), languageList[i].name));
                            }

                            AddConfig("languageDict", languageDict);
                            AddConfig("languageShortNameDict", languageShortNameDict);
                        }


                        if (languageData.ContainsKey("languageNameList"))
                        {
                            languageNameList = JsonMapper.ToObject<List<string>>(languageData["languageNameList"].ToJson());
                        }
                        else if (languageDict != null)
                        {
                            foreach (var item in languageDict)
                            {
                                languageNameList.Add(item.Key);
                            }
                        }

                        AddConfig("languageNameList", languageNameList);

                    }
                    if (data.ContainsKey("regionList"))
                    {
                        var regionList = JsonMapper.ToObject<List<string>>(data["regionList"].ToJson());
                        AddConfig("regionList", regionList);
                        //ReadUserRegion();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("读取语言配置失败：" + ex.ToString());
                }
            }
            else
            {
            }
        }
    }
}