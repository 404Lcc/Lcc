using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using LitJson;
using ProtoBuf;
using System.Threading;
//using ILRuntime.Mono.Cecil.Pdb;
//using ILRuntime.Runtime;
using HybridCLR;
using YooAsset;

namespace LccModel
{
    public class Loader : Singleton<Loader>
    {
        public Action FixedUpdate;
        public Action Update;
        public Action LateUpdate;
        public Action OnApplicationQuit;

        private readonly Dictionary<string, Type> _monoTypeDict = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _hotfixTypeDict = new Dictionary<string, Type>();

        public Assembly assembly;
        //public ILRuntime.Runtime.Enviorment.AppDomain appDomain;

        public Dictionary<string, Type> GetHotfixTypeDict()
        {
            return _hotfixTypeDict;
        }


        public void Start(GlobalConfig config)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ass in assemblies)
            {
                foreach (Type type in ass.GetTypes())
                {
                    _monoTypeDict[type.FullName] = type;
                }
            }
            EventSystem.Instance.InitType(_monoTypeDict);

            switch (config.hotfixMode)
            {
                case HotfixMode.Mono:
                    {
                        (byte[], byte[]) hotfix = Load(config);

                        byte[] dllBytes = hotfix.Item1;
                        byte[] pdbBytes = hotfix.Item2;

                        if (config.isRelease)
                        {
                            assembly = Assembly.Load(dllBytes);
                        }
                        else
                        {
                            assembly = Assembly.Load(dllBytes, pdbBytes);
                        }

                        foreach (Type type in assembly.GetTypes())
                        {
                            _hotfixTypeDict[type.FullName] = type;
                        }
                        AStaticMethod start = new MonoStaticMethod(assembly, "LccHotfix.Init", "Start");
                        start.Run();
                        break;
                    }
//                case HotfixMode.ILRuntime:
//                    {
//                        (byte[], byte[]) hotfix = Load(config);

//                        byte[] dllBytes = hotfix.Item1;
//                        byte[] pdbBytes = hotfix.Item2;

//                        appDomain = new ILRuntime.Runtime.Enviorment.AppDomain(ILRuntimeJITFlags.JITOnDemand);

//#if UNITY_EDITOR
//                        appDomain.DebugService.StartDebugService(56000);
//#endif
//#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
//                        appDomain.UnityMainThreadID = Thread.CurrentThread.ManagedThreadId;
//#endif

//                        if (config.isRelease)
//                        {
//                            MemoryStream dllStream = new MemoryStream(dllBytes);
//                            appDomain.LoadAssembly(dllStream, null, new PdbReaderProvider());
//                        }
//                        else
//                        {
//                            MemoryStream dllStream = new MemoryStream(dllBytes);
//                            MemoryStream pdbStream = new MemoryStream(pdbBytes);
//                            appDomain.LoadAssembly(dllStream, pdbStream, new PdbReaderProvider());
//                        }





//                        Type[] types = appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToArray();

//                        foreach (Type type in types)
//                        {
//                            _hotfixTypeDict[type.FullName] = type;
//                        }

//                        ILRuntimeHelper.RegisterCrossBindingAdaptor(appDomain);
//                        ILRuntimeHelper.RegisterCLRMethodRedirction(appDomain);
//                        ILRuntimeHelper.RegisterMethodDelegate(appDomain);
//                        ILRuntimeHelper.RegisterValueTypeBinderHelper(appDomain);

//                        JsonMapper.RegisterILRuntimeCLRRedirection(appDomain);
//                        PType.RegisterILRuntimeCLRRedirection(appDomain);

//                        //CLR绑定的注册，一定要记得将CLR绑定的注册写在CLR重定向的注册后面，因为同一个方法只能被重定向一次，只有先注册的那个才能生效
//                        Type.GetType("ILRuntime.Runtime.Generated.CLRBindings")?.GetMethod("Initialize")?.Invoke(null, new object[] { appDomain });

//                        AStaticMethod start = new ILStaticMethod(appDomain, "LccHotfix.Init", "Start", 0);
//                        start.Run();
//                        break;
//                    }
                case HotfixMode.HybridCLR:
                    {
                        (byte[], byte[]) hotfix = Load(config);

                        byte[] dllBytes = hotfix.Item1;
                        byte[] pdbBytes = hotfix.Item2;

                        // 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
                        // 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
                        // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
                        // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误

                        HomologousImageMode mode = HomologousImageMode.SuperSet;
                        GameObject aot = new GameObject("aot");
                        foreach (var item in AssetManager.Instance.LoadALLRes<TextAsset>(aot, "Unity.Model.dll"))
                        {
                            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                            LoadImageErrorCode errorCode = RuntimeApi.LoadMetadataForAOTAssembly(item.bytes, mode);
                        }
                        GameObject.Destroy(aot);

                        if (config.isRelease)
                        {
                            assembly = Assembly.Load(dllBytes);
                        }
                        else
                        {
                            assembly = Assembly.Load(dllBytes, pdbBytes);
                        }


                        foreach (Type type in assembly.GetTypes())
                        {
                            _hotfixTypeDict[type.FullName] = type;
                        }
                        AStaticMethod start = new MonoStaticMethod(assembly, "LccHotfix.Init", "Start");
                        start.Run();
                        break;
                    }
            }
        }

        private (byte[], byte[]) Load(GlobalConfig config)
        {
            byte[] dllBytes = null;
            byte[] pdbBytes = null;

            GameObject obj = new GameObject("loader");
            TextAsset dllAsset = AssetManager.Instance.LoadRes<TextAsset>(obj, $"Unity.Hotfix.dll");
            dllBytes = RijndaelUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", dllAsset.bytes);


            if (!config.isRelease)
            {
                TextAsset pdbAsset = AssetManager.Instance.LoadRes<TextAsset>(obj, $"Unity.Hotfix.pdb");
                pdbBytes = pdbAsset.bytes;

            }

            GameObject.Destroy(obj);

            return (dllBytes, pdbBytes);
        }

        protected override void Dispose()
        {
            //appDomain?.Dispose();
        }
    }
}