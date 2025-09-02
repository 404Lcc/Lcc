using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LccModel
{
    public class GameLanguage
    {
        public const string ALLLanguageKey = "ALLLanguage";
        public const string CacheLanguageKey = "MutiLanguage";
        public const string DefaultFont = "Font SDF";  // 默认字体资源
        
        private Dictionary<string, string> _languageDict = new Dictionary<string, string>();
        private List<string> _languages;
        private string _curLanguage;
        private bool _localizationHasBeenSet = false;

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



        /// <summary>
        /// 获取使用过的语言
        /// </summary>
        /// <returns></returns>
        public void GetALLLanguages()
        {
            try
            {
                string lang = PlayerPrefs.GetString(ALLLanguageKey);
                if (!string.IsNullOrEmpty(lang))
                {
                    _languages = JsonUtility.ToObject<List<string>>(lang);
                    if (!_languages.Contains(curLanguage))
                    {
                        _languages.Add(curLanguage);
                    }
                }
                else
                {
                    _languages = new List<string>() { curLanguage };
                }
                PlayerPrefs.SetString(ALLLanguageKey, JsonUtility.ToJson(_languages));
            }
            catch
            {
                _languages = new List<string>() { curLanguage };
                PlayerPrefs.SetString(ALLLanguageKey, JsonUtility.ToJson(_languages));
            }
        }

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
                    return Launcher.Instance.GameConfig.defaultLanguage;
                }
                if (Launcher.Instance.GameConfig.languageNameList.Count <= 0)
                {
                    return Launcher.Instance.GameConfig.defaultLanguage;
                }
                if (Launcher.Instance.GameConfig.languageNameList.Contains(lang))
                {
                    return lang;
                }
                return Launcher.Instance.GameConfig.defaultLanguage;
            }
            catch
            {
                return Launcher.Instance.GameConfig.defaultLanguage;
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
            for (int i = 0; i < Launcher.Instance.GameConfig.languageNameList.Count; i++)
            {
                if (newLanguage == Launcher.Instance.GameConfig.languageNameList[i])
                {
                    //判断目标语言文件是否下载了
                    if (_languages.Contains(newLanguage))
                    {
                        PlayerPrefs.SetString(CacheLanguageKey, newLanguage);
                        Launcher.Instance.StartCoroutine(UpdateLanguage(newLanguage));
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
            
            var languageTxt = txtAsset.text;

            if (string.IsNullOrEmpty(languageTxt))
                yield break;
            yield return null;

            curLanguage = languageName;

            // 加载字体
            InitFontAsset();

            GetALLLanguages();
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
                    if (_languageDict.ContainsKey(item.Key))
                    {
                        _languageDict[item.Key] = item.Value;
                    }
                    else
                    {
                        _languageDict.Add(item.Key, item.Value);
                    }
                }
            }
            catch
            {
            }
        }
        
        public void Set(string languageName)
        {
            _localizationHasBeenSet = true;
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
            if (!_localizationHasBeenSet)
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("Local Language has not been initilized");
                }
                return key;
            }

            if (!_languageDict.ContainsKey(key))
            {
                Debug.LogError($"多语言配置里不包含key = {key}。 请策划检查");
                return key;
            }
            string value = _languageDict[key];

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

        public void InitFontAsset()
        {
            TMP_FontAsset defaultFontAsset = Resources.Load<TMP_FontAsset>("Fonts/" + DefaultFont);

            if (Launcher.Instance.GameConfig.languageDict != null && Launcher.Instance.GameConfig.languageDict.ContainsKey(curLanguage))
            {
                var font = Launcher.Instance.GameConfig.languageDict[curLanguage].font;
                Font fontAssetCur = Resources.Load<Font>("Fonts/" + font);

                //清除之前用的字体资源数据和字符，使fallback略过该字体
                //删除texture至一张， 且将尺寸置为0
                defaultFontAsset.ClearFontAssetData(true);
                defaultFontAsset.characterLookupTable.Clear();

                //设置目标字体为静态生成，阻止新的字符生成
                defaultFontAsset.atlasPopulationMode = AtlasPopulationMode.Static;



                TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(fontAssetCur, 50, 8, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic);
                defaultFontAsset.fallbackFontAssetTable.Clear();
                defaultFontAsset.fallbackFontAssetTable.Add(fontAsset);

            }
        }
    }
}