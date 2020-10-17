using System.Collections;
using UnityEngine;

namespace Hotfix
{
    public class LanguageManager : Singleton<LanguageManager>
    {
        public Hashtable languages = new Hashtable();
        public void ChangeLanguage(LanguageType type)
        {
            TextAsset asset = Model.AssetManager.Instance.LoadAssetData<TextAsset>(type.ToString(), ".txt", false, true, AssetType.Game);
            foreach (string item in asset.text.Split('\n'))
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                string[] KeyValue = item.Split('=');
                if (languages.ContainsKey(KeyValue[0])) return;
                languages.Add(KeyValue[0], KeyValue[1]);
            }
        }
        public string GetValue(string key)
        {
            if (!languages.ContainsKey(key)) return string.Empty;
            string value = (string)languages[key];
            return value;
        }
    }
}