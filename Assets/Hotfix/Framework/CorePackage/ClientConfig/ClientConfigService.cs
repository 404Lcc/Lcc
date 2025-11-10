using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class ClientConfigAttribute : AttributeBase
    {
    }

    public interface IClientConfig
    {
        public void LoadConfig();
        public void UnloadConfig();
    }

    internal class ClientConfigService : Module, IClientConfigService
    {
        private Dictionary<Type, IClientConfig> _configs;

        public ClientConfigService()
        {
            _configs = new Dictionary<Type, IClientConfig>();

            foreach (var item in Main.CodeTypesService.GetTypes(typeof(ClientConfigAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(ClientConfigAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    _configs[item] = Activator.CreateInstance(item) as IClientConfig;
                }
            }

            LoadConfig();
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        internal override void Shutdown()
        {
            UnloadConfig();
            _configs.Clear();
        }

        private void LoadConfig()
        {
            foreach (var item in _configs)
            {
                var configInstance = item.Value;
                configInstance.LoadConfig();
            }
        }

        private void UnloadConfig()
        {
            foreach (var item in _configs)
            {
                var configInstance = item.Value;
                configInstance.UnloadConfig();
            }
        }

        public T GetConfig<T>() where T : class, IClientConfig
        {
            var type = typeof(T);

            if (_configs.TryGetValue(type, out var config))
            {
                return (T)config;
            }

            return null;
        }
    }
}