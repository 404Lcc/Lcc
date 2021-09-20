using ILRuntime.Mono.Cecil.Pdb;
using LitJson;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace LccModel
{
    public class ILRuntimeManager : Singleton<ILRuntimeManager>
    {
        public AppDomain appdomain = new AppDomain();
        public List<Type> typeList = new List<Type>();
        public void InitManager()
        {
            LoadHotfixAssembly();
        }
        public void LoadHotfixAssembly()
        {
            TextAsset dllAsset = AssetManager.Instance.LoadAsset<TextAsset>("Unity.Hotfix.dll", ".bytes", false, true, AssetType.DLL);
            MemoryStream dll = new MemoryStream(RijndaelUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", dllAsset.bytes));
#if Release
            appdomain.LoadAssembly(dll, null, new PdbReaderProvider());
#else
            TextAsset pdbAsset = AssetManager.Instance.LoadAsset<TextAsset>("Unity.Hotfix.pdb", ".bytes", false, true, AssetType.DLL);
            MemoryStream pdb = new MemoryStream(pdbAsset.bytes);
            appdomain.LoadAssembly(dll, pdb, new PdbReaderProvider());
#endif
            InitializeILRuntime();
            OnHotfixLoaded();
        }
        public unsafe void InitializeILRuntime()
        {
#if UNITY_EDITOR
            appdomain.DebugService.StartDebugService(56000);
#endif
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            appdomain.UnityMainThreadID = Thread.CurrentThread.ManagedThreadId;
#endif
            ILRuntimeHelper.RegisterCrossBindingAdaptor(appdomain);
            ILRuntimeHelper.RegisterCLRMethodRedirction(appdomain);
            ILRuntimeHelper.RegisterMethodDelegate(appdomain);
            ILRuntimeHelper.RegisterValueTypeBinderHelper(appdomain);

            JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);
            PType.RegisterILRuntimeCLRRedirection(appdomain);

            Type.GetType("ILRuntime.Runtime.Generated.CLRBindings")?.GetMethod("Initialize")?.Invoke(null, new object[] { appdomain });

            typeList = appdomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToList();
        }
        public unsafe void OnHotfixLoaded()
        {
            appdomain.Invoke("LccHotfix.Init", "InitHotfix", null, null);
        }
    }
}