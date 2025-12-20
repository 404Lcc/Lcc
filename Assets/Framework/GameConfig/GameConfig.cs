using System.Collections.Generic;

namespace LccModel
{
    public partial class GameConfig
    {
        private Dictionary<string, object> configDict = new Dictionary<string, object>();

        public void AddConfig(string configName, object configValue)
        {
            if (configDict.TryGetValue(configName, out var var))
            {
                var = configValue;
            }
            else
            {
                configDict[configName] = configValue;
            }
        }


        public T GetConfig<T>(string configName)
        {
            if (configDict.TryGetValue(configName, out var var))
            {
                return (T)var;
            }
            return default(T);
        }

        public T GetConfig<T>(string configName, T defaultValue = default(T))
        {
            if (configDict.TryGetValue(configName, out var var))
            {
                return (T)var;
            }
            return defaultValue;
        }

        public bool HasConfig(string configName)
        {
            return configDict.ContainsKey(configName);
        }
    }
}