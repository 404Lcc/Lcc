using System.Collections.Generic;

namespace LccModel
{
    public static partial class GameConfig
    {
        private static Dictionary<string, object> _configDict = new Dictionary<string, object>();

        public static void AddConfig(string configName, object configValue)
        {
            if (_configDict.TryGetValue(configName, out var var))
            {
                var = configValue;
            }
            else
            {
                _configDict[configName] = configValue;
            }
        }


        public static T GetConfig<T>(string configName)
        {
            if (_configDict.TryGetValue(configName, out var var))
            {
                return (T)var;
            }
            return default(T);
        }

        public static T GetConfig<T>(string configName, T defaultValue = default(T))
        {
            if (_configDict.TryGetValue(configName, out var var))
            {
                return (T)var;
            }
            return defaultValue;
        }

        public static bool HasConfig(string configName)
        {
            return _configDict.ContainsKey(configName);
        }
    }
}