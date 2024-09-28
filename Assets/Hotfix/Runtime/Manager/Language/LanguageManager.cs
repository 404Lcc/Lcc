using LccModel;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    internal class LanguageManager : Module
    {
        public static LanguageManager Instance { get; } = Entry.GetModule<LanguageManager>();
        public Dictionary<string, string> languageDict = new Dictionary<string, string>();

        public GameObject loader;
        public LanguageManager()
        {
            loader = new GameObject("loader");
            GameObject.DontDestroyOnLoad(loader);
        }
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            languageDict.Clear();

            GameObject.Destroy(loader);
        }
        public void ChangeLanguage(LanguageType type)
        {
            TextAsset asset = AssetManager.Instance.LoadRes<TextAsset>(loader, type.ToString());
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