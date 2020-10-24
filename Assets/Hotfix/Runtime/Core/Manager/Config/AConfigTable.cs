using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
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
            string config = LccModel.AssetManager.Instance.LoadAssetData<TextAsset>(typeof(T).Name, ".txt", false, true, AssetType.Text).text;
            configDict = JsonMapper.ToObject<Dictionary<int, T>>(config);
        }
        public T GetConfig(int id)
        {
            return configDict[id];
        }
        public Dictionary<int, T> GetConfigs()
        {
            return configDict;
        }
    }
}