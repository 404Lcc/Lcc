using System;
using System.Collections;
using UnityEngine;

namespace LccModel
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
                    TextAsset asset = AssetManager.Instance.LoadAsset<TextAsset>(item.Name, ".bytes", false, false, AssetType.Config);
                    ProtobufObject obj = (ProtobufObject)ProtobufUtil.Deserialize(item, asset.bytes, 0, asset.bytes.Length);
                    obj.AfterDeserialization();
                    configs.Add(item, obj);
                }
            }
        }
    }
}