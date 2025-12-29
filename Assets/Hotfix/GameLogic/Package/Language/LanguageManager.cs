using LccModel;
using System;
using System.Collections.Generic;
using cfg;

namespace LccHotfix
{
    internal class LanguageManager : Module, ILanguageService
    {
        public Dictionary<string, Language> languageDict = new Dictionary<string, Language>();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            languageDict.Clear();
        }

        public void Init()
        {
            foreach (var item in Main.ConfigService.Tables.TBLanguage.DataList)
            {
                if (string.IsNullOrEmpty(item.Key))
                    continue;
                if (languageDict.ContainsKey(item.Key))
                {
                    Log.Error($"多语言key添加重复 key = {item.Key}");
                    continue;
                }

                languageDict.Add(item.Key, item);
            }
        }

        public string GetValue(string key, params object[] args)
        {
            if (!languageDict.ContainsKey(key))
            {
                Log.Error("多语言配置里不包含key = {0} 请检查", key);
                return key;
            }

            var config = languageDict[key];
            string value = string.Empty;
            switch (GameConfig.AppLanguage)
            {
                case "ChineseSimplified":
                    value = config.Chinese;
                    break;
                case "ChineseTraditional":
                    value = config.TraditionalChinese;
                    break;
                case "English":
                    value = config.English;
                    break;
                case "Korean":
                    value = config.Korean;
                    break;
                case "Russian":
                    value = config.Russian;
                    break;
                case "German":
                    value = config.German;
                    break;
                case "Vietnamese":
                    value = config.Vietnamese;
                    break;
                case "Thai":
                    value = config.Thai;
                    break;
                case "French":
                    value = config.French;
                    break;
                case "Japanese":
                    value = config.Japanese;
                    break;
                case "Spanish":
                    value = config.Spanish;
                    break;
                case "Arabic":
                    value = config.Arabic;
                    break;

                default:
                    value = key;
                    break;
            }

            try
            {
                if (args.Length > 0)
                {
                    value = string.Format(value, args);
                }

                value = value.Replace("\\n", "\n");
            }
            catch (Exception e)
            {
                Log.Error($"多语言有误 key = {key} value = {value}");
            }

            if (string.IsNullOrEmpty(value))
            {
                Log.Error("多语言配置key = {0} value == null", key);
            }

            return value;
        }

        public string GetValue(int id, params object[] args)
        {
            if (id == 0)
            {
                Log.Error("id不能等于0");
                return "";
            }

            var key = GetKey(id);
            return GetValue(key, args);
        }

        public string GetValue(uint id, params object[] args)
        {
            return GetValue((int)id, args);
        }

        public string GetKey(int id)
        {
            if (id == 0)
            {
                Log.Error("id不能等于0");
                return "";
            }

            return Main.ConfigService.Tables.TBLanguage.GetOrDefault(id).Key;
        }

        public string GetKey(uint id)
        {
            return GetKey((int)id);
        }
    }
}