using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using BM;
using LitJson;
using ProtoBuf;
using System.Threading;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Runtime;
using HybridCLR;
using static Org.BouncyCastle.Math.EC.ECCurve;

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
        public ILRuntime.Runtime.Enviorment.AppDomain appDomain;
        public Type GetMonoType(string fullName)
        {
            _monoTypeDict.TryGetValue(fullName, out Type type);
            return type;
        }

        public Type GetHotfixType(string fullName)
        {
            _hotfixTypeDict.TryGetValue(fullName, out Type type);
            return type;
        }
        public List<Type> GetMonoTypeALL()
        {
            return _monoTypeDict.Values.ToList();
        }
        public List<Type> GetHotfixTypeALL()
        {
            return _hotfixTypeDict.Values.ToList();
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
            switch (config.hotfixMode)
            {
                case HotfixMode.Mono:
                    {
                        (byte[], byte[]) hotfix = Load(config);

                        byte[] dllBytes = hotfix.Item1;
                        byte[] pdbBytes = hotfix.Item2;

#if Release
                        assembly = Assembly.Load(dllBytes);
#else
                        assembly = Assembly.Load(dllBytes, pdbBytes);
#endif

                        foreach (Type type in assembly.GetTypes())
                        {
                            _hotfixTypeDict[type.FullName] = type;
                        }
                        AStaticMethod start = new MonoStaticMethod(assembly, "LccHotfix.Init", "Start");
                        start.Run();
                        break;
                    }
                case HotfixMode.ILRuntime:
                    {
                        (byte[], byte[]) hotfix = Load(config);

                        byte[] dllBytes = hotfix.Item1;
                        byte[] pdbBytes = hotfix.Item2;

                        appDomain = new ILRuntime.Runtime.Enviorment.AppDomain(ILRuntimeJITFlags.JITOnDemand);

#if UNITY_EDITOR
                        appDomain.DebugService.StartDebugService(56000);
#endif
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
                        appDomain.UnityMainThreadID = Thread.CurrentThread.ManagedThreadId;
#endif

#if Release
                        using (MemoryStream dllStream = new MemoryStream(dllBytes))
                        {
                            appDomain.LoadAssembly(dllStream, null, new PdbReaderProvider());
                        }
#else
                        using (MemoryStream dllStream = new MemoryStream(dllBytes))
                        {
                            using (MemoryStream pdbStream = new MemoryStream(pdbBytes))
                            {
                                appDomain.LoadAssembly(dllStream, pdbStream, new PdbReaderProvider());
                            }
                        }
#endif





                        Type[] types = appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToArray();

                        foreach (Type type in types)
                        {
                            _hotfixTypeDict[type.FullName] = type;
                        }

                        ILRuntimeHelper.RegisterCrossBindingAdaptor(appDomain);
                        ILRuntimeHelper.RegisterCLRMethodRedirction(appDomain);
                        ILRuntimeHelper.RegisterMethodDelegate(appDomain);
                        ILRuntimeHelper.RegisterValueTypeBinderHelper(appDomain);

                        JsonMapper.RegisterILRuntimeCLRRedirection(appDomain);
                        PType.RegisterILRuntimeCLRRedirection(appDomain);

                        //CLR绑定的注册，一定要记得将CLR绑定的注册写在CLR重定向的注册后面，因为同一个方法只能被重定向一次，只有先注册的那个才能生效
                        Type.GetType("ILRuntime.Runtime.Generated.CLRBindings")?.GetMethod("Initialize")?.Invoke(null, new object[] { appDomain });

                        AStaticMethod start = new ILStaticMethod(appDomain, "LccHotfix.Init", "Start", 0);
                        start.Run();
                        break;
                    }
                case HotfixMode.HybridCLR:
                    {
                        (byte[], byte[]) hotfix = Load(config);

                        byte[] dllBytes = hotfix.Item1;
                        byte[] pdbBytes = hotfix.Item2;

                        // 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
                        // 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
                        // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
                        // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误

                        HomologousImageMode mode = HomologousImageMode.Consistent;
                        foreach (var item in config.aotMetaAssemblyNameList)
                        {
                            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                            TextAsset dllAsset = AssetManager.Instance.LoadAsset<TextAsset>(out LoadHandler dllHandler, item, AssetSuffix.Bytes, AssetType.DLL);
                            LoadImageErrorCode errorCode = RuntimeApi.LoadMetadataForAOTAssembly(dllAsset.bytes, mode);
                            if (dllHandler != null)
                            {
                                dllHandler.UnLoad();
                            }
                        }



#if Release
                        assembly = Assembly.Load(dllBytes);
#else
                        assembly = Assembly.Load(dllBytes, pdbBytes);
#endif

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


            TextAsset dllAsset = AssetManager.Instance.LoadAsset<TextAsset>(out LoadHandler dllHandler, $"{config.hotfix}.dll", AssetSuffix.Bytes, AssetType.DLL);
            dllBytes = RijndaelUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", dllAsset.bytes);

            if (dllHandler != null)
            {
                dllHandler.UnLoad();
            }

#if !Release
            TextAsset pdbAsset = AssetManager.Instance.LoadAsset<TextAsset>(out LoadHandler pdbHandler, $"{config.hotfix}.pdb", AssetSuffix.Bytes, AssetType.DLL);
            pdbBytes = pdbAsset.bytes;

            if (pdbHandler != null)
            {
                pdbHandler.UnLoad();
            }
#endif
            return (dllBytes, pdbBytes);
        }

        protected override void Dispose()
        {
            appDomain?.Dispose();
        }
    }
}