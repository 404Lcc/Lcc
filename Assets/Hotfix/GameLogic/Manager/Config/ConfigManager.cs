using cfg;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal class ConfigManager : Module, IConfigService
    {
        private AssetLoader _loader;
        public Tables Tables { get; set; }
        public Dictionary<string, JSONNode> ConfigDict { get; set; }

        public ConfigManager()
        {
            _loader = new AssetLoader();
            Tables = new Tables(Load);
            //todo 代办
            // _loader.LoadAssetAsync<TextAsset>(file, (x) =>
            // {
            //     var res = x.AssetObject as TextAsset;
            //     var node = JSON.Parse(res.text);
            // });
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            _loader.Release();
        }

        public JSONNode Load(string name)
        {
            ConfigDict.TryGetValue(name, out JSONNode config);
            return config;
        }
    }
}