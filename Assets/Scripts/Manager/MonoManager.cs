using System;
using System.Reflection;
using UnityEngine;

namespace Model
{
    public class MonoManager : MonoBehaviour
    {
        public Assembly assembly;
        public void InitManager()
        {
            LoadHotfixAssembly();
        }
        public void LoadHotfixAssembly()
        {
            TextAsset dllAsset = IO.assetManager.LoadAssetData<TextAsset>("Unity.Hotfix.dll", ".bytes", false, true, AssetType.Text);
#if Release
            assembly = AppDomain.CurrentDomain.Load(dllAsset.bytes);
#else
            TextAsset pdbAsset = IO.assetManager.LoadAssetData<TextAsset>("Unity.Hotfix.pdb", ".bytes", false, true, AssetType.Text);
            assembly = AppDomain.CurrentDomain.Load(dllAsset.bytes, pdbAsset.bytes);
#endif
            OnHotfixLoaded();
        }
        public unsafe void OnHotfixLoaded()
        {
            object instance = assembly.CreateInstance("Hotfix.Hotfix");
            Type type = instance.GetType();
            MethodInfo methodInfo = type.GetMethod("InitHotfix");
            methodInfo.Invoke(null, null);
        }
    }
}