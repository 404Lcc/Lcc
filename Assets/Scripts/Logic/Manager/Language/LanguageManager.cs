using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class LanguageManager : Singleton<LanguageManager>
    {
        public Dictionary<string, string> languageDict = new Dictionary<string, string>();
        public void ChangeLanguage(LanguageType type)
        {
            TextAsset asset = AssetManager.Instance.LoadAsset<TextAsset>(type.ToString(), ".txt", false, true, AssetType.Config);
            foreach (string item in asset.text.Split('\n'))
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                string[] KeyValue = item.Split('=');
                if (languageDict.ContainsKey(KeyValue[0])) return;
                languageDict.Add(KeyValue[0], KeyValue[1]);
            }
        }
        public string GetValue(string key)
        {
            if (!languageDict.ContainsKey(key)) return string.Empty;
            string value = languageDict[key];
            return value;
        }
    }
}