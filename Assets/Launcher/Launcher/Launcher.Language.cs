using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public partial class Launcher
    {
        public const string ALLLanguageKey = "ALLLanguage";
        public const string CacheLanguageKey = "MutiLanguage";

        private string languageTxt = null;

        private bool localizationHasBeenSet = false;
        private Dictionary<string, string> mLanguageDict = new Dictionary<string, string>();

        private string _curLanguage;

        public void ClearLocal()
        {

        }


        //游戏启动时第一次初始化语言
        private IEnumerator InitLanguage()
        {
            curLanguage = GetSelectedLanguage();

            // 加载多语言文本
            var txtRes = Resources.LoadAsync<TextAsset>("LanguageConfig_" + curLanguage);
            while (!txtRes.isDone)
                yield return null;
            TextAsset txtAsset = txtRes.asset as TextAsset;
            if (txtAsset != null)
            {
                languageTxt = txtAsset.text;
                Resources.UnloadAsset(txtAsset);
            }

            yield return null;
            OnLanguageAssetLoad(languageTxt);
            Set(curLanguage);

            Debug.Log("完成多语言初始化加载");


        }

        public string curLanguage
        {
            get
            {
                return _curLanguage;
            }
            set
            {
                _curLanguage = value;
                PlayerPrefs.SetString(CacheLanguageKey, _curLanguage);
            }
        }

        public SystemLanguage curLanguageType
        {
            get
            {
                SystemLanguage languageType = SystemLanguage.Afrikaans;
                Enum.TryParse(curLanguage, out languageType);
                return languageType;
            }
        }

        //public List<string> languages;

        //public void AddLanguage(string languageName)
        //{

        //}

        ///// <summary>
        ///// 更新已有语言
        ///// </summary>
        ///// <returns></returns>
        //public void UpdateALLLanguages()
        //{
        //    try
        //    {
        //        string lang = PlayerPrefs.GetString(ALLLanguageKey);
        //        if (!string.IsNullOrEmpty(lang))
        //        {
        //            languages = JsonHelper.ToObject<List<string>>(lang);
        //            if (!languages.Contains(curLanguage))
        //            {
        //                languages.Add(curLanguage);
        //            }
        //        }
        //        else
        //        {
        //            languages = new List<string>() { curLanguage };
        //        }
        //        PlayerPrefs.SetString(ALLLanguageKey, JsonHelper.ToJson(languages));
        //    }
        //    catch
        //    {
        //        languages = new List<string>() { curLanguage };
        //        PlayerPrefs.SetString(ALLLanguageKey, JsonHelper.ToJson(languages));
        //    }
        //}

        public string GetSelectedLanguage()
        {
            try
            {
                string lang = PlayerPrefs.GetString(CacheLanguageKey);
                if (string.IsNullOrEmpty(lang))
                {
                    lang = GetSystemLanguage().ToString();
                }
                if (string.IsNullOrEmpty(lang))
                {
                    return GameConfig.defaultLanguage;
                }
                if (GameConfig.languageNameList.Count <= 0)
                {
                    return GameConfig.defaultLanguage;
                }
                if (GameConfig.languageNameList.Contains(lang))
                {
                    return lang;
                }
                return GameConfig.defaultLanguage;
            }
            catch
            {
                return GameConfig.defaultLanguage;
            }
        }

        private SystemLanguage GetSystemLanguage()
        {
            SystemLanguage language = Application.systemLanguage;
            if (language == SystemLanguage.Chinese)
            {
#if UNITY_EDITOR
                return SystemLanguage.ChineseSimplified;
#endif
            }
            return language;
        }

        /// <summary>
        /// 切换多语言
        /// </summary>
        /// <param name="newLanguage"></param>
        /// <returns></returns>
        public bool SetLanguage(string newLanguage)
        {
            if (!string.IsNullOrEmpty(curLanguage) && newLanguage == curLanguage)
                return true;
            for (int i = 0; i < GameConfig.languageNameList.Count; i++)
            {
                if (newLanguage == GameConfig.languageNameList[i])
                {
                    //if (languages.Contains(newLanguage)) 不需要分开下载多语言，先不考虑 todo
                    {
                        PlayerPrefs.SetString(CacheLanguageKey, newLanguage);
                        StartCoroutine(UpdateLanguage(newLanguage));
                    }
                    return true;
                }
            }
            Debug.LogError($"The language {newLanguage} is not exited");
            return false;
        }

        /// <summary>
        /// 重新加载多语言
        /// </summary>
        /// <returns></returns>
        public IEnumerator UpdateLanguage(string languageName)
        {
            // 更新语言
            string txtBundle = "LanguageConfig_" + languageName;
            var txtRes = Resources.LoadAsync<TextAsset>(txtBundle);
            while (!txtRes.isDone)
                yield return null;
            TextAsset txtAsset = txtRes.asset as TextAsset;
            if (txtAsset == null)
            {
                Debug.LogError("[language update] 加载多语言失败 " + txtBundle);
                yield break;
            }
            languageTxt = txtAsset.text;

            if (string.IsNullOrEmpty(languageTxt))
                yield break;
            yield return null;

            curLanguage = languageName;
            //UpdateALLLanguages();
            OnLanguageAssetLoad(languageTxt);

            Set(curLanguage);
        }

        public void OnLanguageAssetLoad(string languageTxt)
        {
            try
            {
                if (languageTxt == null)
                {
                    Debug.LogError("localized text is not loaded");
                    return;
                }
                foreach (var item in ReadDictionary(languageTxt))
                {
                    if (mLanguageDict.ContainsKey(item.Key))
                    {
                        mLanguageDict[item.Key] = item.Value;
                    }
                    else
                    {
                        mLanguageDict.Add(item.Key, item.Value);
                    }
                }
            }
            catch
            {
            }
        }
        void Set(string languageName)
        {
            localizationHasBeenSet = true;
            //ok 了 如果有本地回调可以在重新触发
        }

        private Dictionary<string, string> ReadDictionary(string txt)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            char[] separator = new char[] { '=' };
            string[] lineSeparator = new string[] { "\n" };
            var lines = txt.Split(lineSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line == null) break;
                if (line.StartsWith("//")) continue;

                string[] split = line.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length == 2)
                {
                    string key = split[0].Trim();
                    string val = split[1].TrimEnd().Replace("\\n", "\n");
                    if (val.Length > 0)
                        val = val.Remove(0, 1);
                    if (dict.ContainsKey(key))
                    {
                        Debug.LogError("多语言key: " + key + " 重复了");
                    }
                    dict[key] = val;
                }
            }
            return dict;
        }


        public string GetLanguage(string key, params string[] args)
        {
            if (string.IsNullOrEmpty(key))
                return key;
            if (!localizationHasBeenSet)
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("Local Language has not been initilized");
                }
                return key;
            }

            if (!mLanguageDict.ContainsKey(key))
            {
                Debug.LogError($"多语言配置里不包含key = {key}。 请策划检查");
                return key;
            }
            string value = mLanguageDict[key];

            if (args.Length > 0)
            {
                value = string.Format(value, args);
            }
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError($"多语言配置key = {key} value == null。 请策划检查");
            }
            return value;
        }
    }
}