using LccModel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        public Dictionary<Type, ProtobufObject> configDict = new Dictionary<Type, ProtobufObject>();
        public void InitManager()
        {
            foreach (Type item in Manager.Instance.GetTypesByAttribute(typeof(ConfigAttribute)))
            {
                TextAsset asset = AssetManager.Instance.LoadAsset<TextAsset>(item.Name, AssetSuffix.Bytes, AssetType.Config);
                ProtobufObject obj = (ProtobufObject)ProtobufUtil.Deserialize(item, asset.bytes, 0, asset.bytes.Length);
                obj.AfterDeserialization();
                configDict.Add(item, obj);
            }
        }
    }
}