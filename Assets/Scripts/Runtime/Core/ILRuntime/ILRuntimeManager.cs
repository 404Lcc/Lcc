using UnityEngine;
using System.IO;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Mono.Cecil.Pdb;
using System.Threading;
using LitJson;
//using ILRuntime.Runtime.Generated;

namespace Model
{
    public class ILRuntimeManager : Singleton<ILRuntimeManager>
    {
        public AppDomain appDomain = new AppDomain();
        public void InitManager()
        {
            LoadHotfixAssembly();
        }
        public void LoadHotfixAssembly()
        {
            TextAsset dllAsset = AssetManager.Instance.LoadAssetData<TextAsset>("Unity.Hotfix.dll", ".bytes", false, true, AssetType.Text);
            MemoryStream dll = new MemoryStream(Util.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", dllAsset.bytes));
#if Release
            appDomain.LoadAssembly(dll, null, new PdbReaderProvider());
#else
            TextAsset pdbAsset = AssetManager.Instance.LoadAssetData<TextAsset>("Unity.Hotfix.pdb", ".bytes", false, true, AssetType.Text);
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
        }
        public unsafe void OnHotfixLoaded()
        {
            appDomain.Invoke("Hotfix.Init", "InitHotfix", null, null);
        }
    }
}