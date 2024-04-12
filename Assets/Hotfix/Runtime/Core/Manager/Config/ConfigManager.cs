using LccModel;
using Luban;
using SimpleJSON;
using System;
using UnityEngine;
using YooAsset;

namespace LccHotfix
{
    public class ConfigManager : AObjectBase
    {
        public static ConfigManager Instance { get; set; }


        public override void Awake()
        {
            base.Awake();

            Instance = this;


            var tablesCtor = typeof(cfg.Tables).GetConstructors()[0];
            var loaderReturnType = tablesCtor.GetParameters()[0].ParameterType.GetGenericArguments()[1];
            // 根据cfg.Tables的构造函数的Loader的返回值类型决定使用json还是ByteBuf Loader
            Delegate loader = loaderReturnType == typeof(ByteBuf) ? new Func<string, ByteBuf>(LoadByteBuf) : (Delegate)new Func<string, JSONNode>(LoadJson);
            var tables = (cfg.Tables)tablesCtor.Invoke(new object[] { loader });
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
        }
        private static JSONNode LoadJson(string file)
        {
            var text = AssetManager.Instance.LoadAsset<TextAsset>(out AssetHandle handle, file, AssetSuffix.Json, AssetType.Config);
            return JSON.Parse(text.text);
        }

        private static ByteBuf LoadByteBuf(string file)
        {
            var bytes = AssetManager.Instance.LoadAsset<TextAsset>(out AssetHandle handle, file, AssetSuffix.Bytes, AssetType.Config);
            return new ByteBuf(bytes.bytes);
        }
    }
}