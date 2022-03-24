using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        public Dictionary<Type, ProtobufObject> configDict = new Dictionary<Type, ProtobufObject>();
        public void InitManager()
        {
            foreach (Type item in Manager.Instance.typeDict.Values)
            {
                if (item.IsAbstract) continue;
                ConfigAttribute[] configAttributes = (ConfigAttribute[])item.GetCustomAttributes(typeof(ConfigAttribute), false);
                if (configAttributes.Length > 0)
                {
                    TextAsset asset = AssetManager.Instance.LoadAsset<TextAsset>(item.Name, AssetSuffix.Bytes, AssetType.Config);
                    ProtobufObject obj = (ProtobufObject)ProtobufUtil.Deserialize(item, asset.bytes, 0, asset.bytes.Length);
                    obj.AfterDeserialization();
                    configDict.Add(item, obj);
                }
            }
        }
    }
}