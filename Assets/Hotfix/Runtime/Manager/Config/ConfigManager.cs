using cfg;
using LccModel;
using Luban;
using SimpleJSON;
using System;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    internal class ConfigManager : Module
    {
        public static ConfigManager Instance { get; } = Entry.GetModule<ConfigManager>();

        public Tables Tables { get; set; }

        public GameObject loader;
        public ConfigManager()
        {
            loader = new GameObject("loader");
            GameObject.DontDestroyOnLoad(loader);

            var tablesCtor = typeof(Tables).GetConstructors()[0];
            var loaderReturnType = tablesCtor.GetParameters()[0].ParameterType.GetGenericArguments()[1];
            // 根据cfg.Tables的构造函数的Loader的返回值类型决定使用json还是ByteBuf Loader
            Delegate loaderFun = loaderReturnType == typeof(ByteBuf) ? new Func<string, ByteBuf>(LoadByteBuf) : (Delegate)new Func<string, JSONNode>(LoadJson);
            Tables = (Tables)tablesCtor.Invoke(new object[] { loaderFun });
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            GameObject.Destroy(loader);
        }



        private JSONNode LoadJson(string file)
        {
            var text = AssetManager.Instance.LoadRes<TextAsset>(loader, file);
            return JSON.Parse(text.text);
        }

        private ByteBuf LoadByteBuf(string file)
        {
            var bytes = AssetManager.Instance.LoadRes<TextAsset>(loader, file);
            return new ByteBuf(bytes.bytes);
        }

    }
}