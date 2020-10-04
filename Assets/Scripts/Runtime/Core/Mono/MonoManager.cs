using System;
using System.Reflection;
using UnityEngine;

namespace Model
{
    public class MonoManager : Singleton<MonoManager>
    {
        public Assembly assembly;
        public void InitManager()
        {
            LoadHotfixAssembly();
        }
        public void LoadHotfixAssembly()
        {
            TextAsset dllAsset = AssetManager.Instance.LoadAssetData<TextAsset>("Unity.Hotfix.dll", ".bytes", false, true, AssetType.Text);
#if Release
            assembly = AppDomain.CurrentDomain.Load(GameUtil.RijndaelDecrypt(Encoding.UTF8.GetBytes("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"), dllAsset.bytes));
#else
            TextAsset pdbAsset = AssetManager.Instance.LoadAssetData<TextAsset>("Unity.Hotfix.pdb", ".bytes", false, true, AssetType.Text);
            assembly = AppDomain.CurrentDomain.Load(Util.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", dllAsset.bytes), pdbAsset.bytes);
#endif
            OnHotfixLoaded();
        }
        public unsafe void OnHotfixLoaded()
        {
            object instance = assembly.CreateInstance("Hotfix.Init");
            Type type = instance.GetType();
            MethodInfo methodInfo = type.GetMethod("InitHotfix");
            methodInfo.Invoke(null, null);
        }
    }
}