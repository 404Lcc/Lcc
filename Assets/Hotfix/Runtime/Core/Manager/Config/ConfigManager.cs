using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        public Hashtable configs = new Hashtable();
        public void InitManager()
        {
            foreach (Type item in GetType().Assembly.GetTypes())
            {
                if (item.IsAbstract) continue;
                ConfigAttribute[] configAttributes = (ConfigAttribute[])item.GetCustomAttributes(typeof(ConfigAttribute), false);
                if (configAttributes.Length > 0)
                {
                    IConfigBase iConfigBase = (IConfigBase)Activator.CreateInstance(item);
                    iConfigBase.InitConfig();
                    configs.Add(iConfigBase.ConfigType, iConfigBase);
                }
            }
        }
        public T GetConfig<T>(int id) where T : IConfig
        {
            Type type = typeof(T);
            if (configs.ContainsKey(type))
            {
                AConfig<T> aConfig = (AConfig<T>)configs[type];
                return aConfig.GetConfig(id);
            }
            else
            {
                Debug.Log("Config不存在" + type.Name);
                return default;
            }
        }
        public Dictionary<int, T> GetConfig<T>() where T : IConfig
        {
            Type type = typeof(T);
            if (configs.ContainsKey(type))
            {
                AConfig<T> aConfig = (AConfig<T>)configs[type];
                return aConfig.GetConfig();
            }
            else
            {
                Debug.Log("Config不存在" + type.Name);
                return default;
            }
        }
    }
}