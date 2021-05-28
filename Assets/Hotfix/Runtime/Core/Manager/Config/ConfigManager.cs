using LccModel;
using System;
using System.Collections;
using UnityEngine;

namespace LccHotfix
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        public Hashtable configs = new Hashtable();
        public void InitManager()
        {
            foreach (Type item in Manager.Instance.types.Values)
            {
                if (item.IsAbstract) continue;
                ConfigAttribute[] configAttributes = (ConfigAttribute[])item.GetCustomAttributes(typeof(ConfigAttribute), false);
                if (configAttributes.Length > 0)
                {
                    TextAsset asset = AssetManager.Instance.LoadAsset<TextAsset>(item.Name, ".bytes", false, true, AssetType.Config);
                    object obj = ProtobufUtil.Deserialize(item, asset.bytes, 0, asset.bytes.Length);
                    configs.Add(item, obj);
                }
            }
        }
    }
}