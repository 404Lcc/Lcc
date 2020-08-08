using System.Collections;
using UnityEngine;

namespace Hotfix
{
    public class LanguageManager : MonoBehaviour
    {
        public Hashtable languages;
        void Awake()
        {
            InitManager();
        }
        public void InitManager()
        {
            languages = new Hashtable();
        }
        public void ChangeLanguage(LanguageType type)
        {
            TextAsset asset = Model.IO.assetManager.LoadAssetData<TextAsset>(type.ToString(), ".txt", false, true, AssetType.Game);
            string text = asset.text;
            string[] all = text.Split('\n');
            foreach (string item in all)
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
            string value = languages[key] as string;
            return value;
        }
    }
}