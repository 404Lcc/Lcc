using System;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace LccModel
{
    public class LanguageConfig
    {
        public string name;
        public string shortName;
        public string font;
        public string desc;
    }

    public static partial class GameConfig
    {
        /// <summary>
        /// 默认语言
        /// </summary>
        public static string AppLanguage
        {
            get { return GetConfig<string>("appLanguage", "English"); }
            set { AddConfig("appLanguage", "English"); }
        }

        /// <summary>
        /// 语言显示信息
        /// </summary>
        public static Dictionary<string, LanguageConfig> LanguageDict
        {
            get { return GetConfig<Dictionary<string, LanguageConfig>>("languageDict"); }
        }

        public static Dictionary<string, SystemLanguage> LanguageShortNameDict
        {
            get { return GetConfig<Dictionary<string, SystemLanguage>>("languageShortNameDict"); }
        }

        /// <summary>
        /// 当前平台支持的多语言
        /// </summary>
        public static List<string> LanguageNameList
        {
            get { return GetConfig<List<string>>("languageNameList"); }
        }

        public static void ReadLanguage(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    Dictionary<string, LanguageConfig> languageDict = new Dictionary<string, LanguageConfig>();
                    Dictionary<string, SystemLanguage> languageShortNameDict = new Dictionary<string, SystemLanguage>();
                    List<string> languageNameList = null;


                    JsonData data = JsonMapper.ToObject(text);
                    if (data.ContainsKey("language"))
                    {
                        JsonData languageData = data["language"];
                        if (languageData.ContainsKey("appLanguage"))
                        {
                            AddConfig("appLanguage", languageData["appLanguage"].ToString());
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
                }
                catch (Exception ex)
                {
                    Debug.LogError("读取语言配置失败：" + ex.ToString());
                }
            }
        }

        public static string GetLanguageShortName(SystemLanguage lang)
        {
            if (LanguageDict.TryGetValue(lang.ToString(), out var langData))
            {
                return langData.shortName;
            }

            return null;
        }

        public static SystemLanguage GetLanguageName(string shortName)
        {
            if (LanguageShortNameDict.TryGetValue(shortName.ToString(), out var lang))
            {
                return lang;
            }

            return SystemLanguage.Unknown;
        }
    }
}