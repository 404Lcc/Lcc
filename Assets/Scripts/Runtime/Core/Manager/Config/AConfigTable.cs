using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public abstract class AConfigTable<T> : IConfigTable where T : IConfig
    {
        public Dictionary<int, T> configDict = new Dictionary<int, T>();
        public Type ConfigType
        {
            get
            {
                return typeof(T);
            }
        }
        public void InitConfigTable()
        {
            TextAsset asset = AssetManager.Instance.LoadAsset<TextAsset>(typeof(T).Name, ".txt", false, true, AssetType.Config);
            foreach (KeyValuePair<string, T> item in JsonUtil.ToObject<Dictionary<string, T>>(asset.text))
            {
                configDict.Add(int.Parse(item.Key), item.Value);
            }
        }
        public T GetConfig(int id)
        {
            if (configDict.ContainsKey(id))
            {
                return configDict[id];
            }
            else
            {
                return default;
            }
        }
        public Dictionary<int, T> GetConfigs()
        {
            return configDict;
        }
    }
}