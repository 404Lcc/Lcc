using BM;
using LccModel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class ConfigManager : AObjectBase
    {
        public static ConfigManager Instance { get; set; }
        public Dictionary<Type, ProtobufObject> configDict = new Dictionary<Type, ProtobufObject>();


        public override void Awake()
        {
            base.Awake();

            Instance = this;
            foreach (Type item in Manager.Instance.GetTypesByAttribute(typeof(ConfigAttribute)))
            {
                TextAsset asset = AssetManager.Instance.LoadAsset<TextAsset>(out LoadHandler handler, item.Name, AssetSuffix.Bytes, AssetType.Config);
                ProtobufObject obj = (ProtobufObject)ProtobufUtil.Deserialize(item, asset.bytes, 0, asset.bytes.Length);
                obj.AfterDeserialization();
                configDict.Add(item, obj);
                AssetManager.Instance.UnLoadAsset(handler);
            }
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            configDict.Clear();
            
            Instance = null;
        }

    }
}