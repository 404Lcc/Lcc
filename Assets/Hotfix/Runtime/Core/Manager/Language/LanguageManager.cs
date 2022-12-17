using BM;
using LccModel;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class LanguageManager : AObjectBase
    {
        public static LanguageManager Instance { get; set; }
        public Dictionary<string, string> languageDict = new Dictionary<string, string>();

        public override void Awake()
        {
            base.Awake();

            Instance = this;

        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
            languageDict.Clear();
        }
        public void ChangeLanguage(LanguageType type)
        {
            TextAsset asset = AssetManager.Instance.LoadAsset<TextAsset>(out LoadHandler handler, type.ToString(), AssetSuffix.Txt, AssetType.Config);
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
            handler.UnLoad();
        }
        public string GetValue(string key)
        {
            if (!languageDict.ContainsKey(key)) return string.Empty;
            string value = languageDict[key];
            return value;
        }
    }
}