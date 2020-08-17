using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Mono.Cecil.Pdb;
using System.Threading;
using LitJson;
//using ILRuntime.Runtime.Generated;

namespace Model
{
    public class ILRuntimeManager : MonoBehaviour
    {
        public AppDomain appdomain;
        public void InitManager()
        {
            appdomain = new AppDomain();
            LoadHotfixAssembly();
        }
        public void LoadHotfixAssembly()
        {
            TextAsset dllasset = IO.assetManager.LoadAssetData<TextAsset>("Unity.Hotfix.dll", ".bytes", false, true, AssetType.Text);
            MemoryStream dll = new MemoryStream(dllasset.bytes);
#if Release
            appdomain.LoadAssembly(dll, null, new PdbReaderProvider());
#else
            TextAsset pdbasset = IO.assetManager.LoadAssetData<TextAsset>("Unity.Hotfix.pdb", ".bytes", false, true, AssetType.Text);
            MemoryStream pdb = new MemoryStream(pdbasset.bytes);
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
            appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());

            JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);

            //CLRBindings.Initialize(appdomain);

            SetupCLRRedirection();
        }
        public unsafe void OnHotfixLoaded()
        {
            appdomain.Invoke("Hotfix.Hotfix", "InitHotfix", null, null);
        }
        public unsafe void SetupCLRRedirection()
        {
            var arr = typeof(GameObject).GetMethods();
            foreach (var i in arr)
            {
                if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
                {
                    appdomain.RegisterCLRMethodRedirection(i, AddComponent);
                }
                if (i.Name == "GetComponent" && i.GetGenericArguments().Length == 1)
                {
                    appdomain.RegisterCLRMethodRedirection(i, GetComponent);
                }
            }
        }
        public unsafe static StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
            if (instance == null)
            {
                throw new System.NullReferenceException();
            }
            __intp.Free(ptr);

            var genericArgument = __method.GenericArguments;
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res;
                if (type is CLRType)
                {
                    res = instance.AddComponent(type.TypeForCLR);
                }
                else
                {
                    var ilInstance = new ILTypeInstance(type as ILType, false);
                    var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
                    clrInstance.ILInstance = ilInstance;
                    clrInstance.AppDomain = __domain;
                    ilInstance.CLRInstance = clrInstance;

                    res = clrInstance.ILInstance;

                    clrInstance.Awake();
                }
                return ILIntepreter.PushObject(ptr, __mStack, res);
            }
            return __esp;
        }
        public unsafe static StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;

            var ptr = __esp - 1;
            GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
            if (instance == null)
            {
                throw new System.NullReferenceException();
            }
            __intp.Free(ptr);

            var genericArgument = __method.GenericArguments;
            if (genericArgument != null && genericArgument.Length == 1)
            {
                var type = genericArgument[0];
                object res = null;
                if (type is CLRType)
                {
                    res = instance.GetComponent(type.TypeForCLR);
                }
                else
                {
                    var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
                    for (int i = 0; i < clrInstances.Length; i++)
                    {
                        var clrInstance = clrInstances[i];
                        if (clrInstance.ILInstance != null)
                        {
                            if (clrInstance.ILInstance.Type == type)
                            {
                                res = clrInstance.ILInstance;
                                break;
                            }
                        }
                    }
                }
                return ILIntepreter.PushObject(ptr, __mStack, res);
            }
            return __esp;
        }
    }
}