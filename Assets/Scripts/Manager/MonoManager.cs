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
            TextAsset dllasset = IO.assetManager.LoadAssetData<TextAsset>("Unity.Hotfix.dll", ".bytes", false, true, AssetType.Text);
#if Release
            assembly = AppDomain.CurrentDomain.Load(dllasset.bytes);
#else
            TextAsset pdbasset = IO.assetManager.LoadAssetData<TextAsset>("Unity.Hotfix.pdb", ".bytes", false, true, AssetType.Text);
            assembly = AppDomain.CurrentDomain.Load(dllasset.bytes, pdbasset.bytes);
#endif
            OnHotfixLoaded();
        }
        public unsafe void OnHotfixLoaded()
        {
            object instance = assembly.CreateInstance("Hotfix.Hotfix");
            Type type = instance.GetType();
            MethodInfo methodinfo = type.GetMethod("InitHotfix");
            methodinfo.Invoke(null, null);
        }
    }
}