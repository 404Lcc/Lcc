using cfg;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal class ConfigManager : Module, IConfigService
    {
        private AssetLoader _loader;
        private Dictionary<string, JSONNode> _configDict = new Dictionary<string, JSONNode>();

        private int _totalConfigCount;
        private int _loadedConfigCount;
        private bool _allConfigsLoaded;

        public Tables Tables { get; set; }

        public bool Initialized => Tables != null;

        public ConfigManager()
        {
            _loader = new AssetLoader();
        }

        public void Init()
        {
            var infos = Main.AssetService.DefaultPackage.GetAssetInfos("luban");
            _totalConfigCount = infos.Length;
            _loadedConfigCount = 0;
            _allConfigsLoaded = false;

            if (_totalConfigCount == 0)
            {
                return;
            }

            foreach (var item in infos)
            {
                _loader.LoadAssetAsync<TextAsset>(item.Address, (x) =>
                {
                    var text = x.AssetObject as TextAsset;
                    var node = JSON.Parse(text.text);
                    _configDict.Add(item.Address, node);

                    OnConfigLoaded();
                });
            }
        }

        private void OnConfigLoaded()
        {
            _loadedConfigCount++;

            // 检查是否所有配置都已加载完成
            if (_loadedConfigCount >= _totalConfigCount && !_allConfigsLoaded)
            {
                _allConfigsLoaded = true;
                Tables = new Tables(Load);
                
                _loader.Release();
            }
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }

        public JSONNode Load(string name)
        {
            _configDict.TryGetValue(name, out JSONNode config);
            return config;
        }
    }
}