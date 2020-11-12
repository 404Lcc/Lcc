using ILRuntime.Mono.Cecil.Pdb;
using LitJson;
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
        public AppDomain appDomain = new AppDomain();
        public List<Type> typeList = new List<Type>();
        public void InitManager()
        {
            LoadHotfixAssembly();
        }
        public async void LoadHotfixAssembly()
        {
            TextAsset dllAsset = await AssetManager.Instance.LoadAsset<TextAsset>("Unity.Hotfix.dll", ".bytes", false, true, AssetType.Text);
            MemoryStream dll = new MemoryStream(RijndaelUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", dllAsset.bytes));
#if Release
            appDomain.LoadAssembly(dll, null, new PdbReaderProvider());
#else
            TextAsset pdbAsset = await AssetManager.Instance.LoadAsset<TextAsset>("Unity.Hotfix.pdb", ".bytes", false, true, AssetType.Text);
            MemoryStream pdb = new MemoryStream(pdbAsset.bytes);
            appDomain.LoadAssembly(dll, pdb, new PdbReaderProvider());
#endif
            InitializeILRuntime();
            OnHotfixLoaded();
        }
        public unsafe void InitializeILRuntime()
        {
#if UNITY_EDITOR
            appDomain.DebugService.StartDebugService(56000);
#endif
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            appDomain.UnityMainThreadID = Thread.CurrentThread.ManagedThreadId;
#endif
            JsonMapper.RegisterILRuntimeCLRRedirection(appDomain);

            //CLRBindings.Initialize(appDomain);

            typeList = appDomain.LoadedTypes.Values.Select(x => x.ReflectionType).ToList();
        }
        public unsafe void OnHotfixLoaded()
        {
            appDomain.Invoke("LccHotfix.Init", "InitHotfix", null, null);
        }
    }
}