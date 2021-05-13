#if UNITY_EDITOR
using ILRuntime.Runtime.CLRBinding;
using ILRuntime.Runtime.Enviorment;
using LccModel;
using System.IO;
using System.Reflection;
using UnityEditor;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    [Obfuscation(Exclude = true)]
    public class ILRuntimeCLRBindingMenuItem
    {
        [MenuItem("ILRuntime/通过自动分析热更DLL生成CLR绑定")]
        public static void GenerateCLRBindingByAnalysis()
        {
            //用新的分析热更dll调用引用来生成绑定代码
            AppDomain domain = new AppDomain();
            using (MemoryStream dll = new MemoryStream(RijndaelUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", FileUtil.GetAsset("Assets/Resources/DLL/Unity.Hotfix.dll.bytes"))))
            {
                domain.LoadAssembly(dll);
                //Crossbind Adapter is needed to generate the correct binding code
                InitILRuntime(domain);
                BindingCodeGenerator.GenerateBindingCode(domain, "Assets/Scripts/Runtime/Core/Manager/ILRuntime/Generated");
            }
            AssetDatabase.Refresh();
        }
        public static void InitILRuntime(AppDomain domain)
        {
            //这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
        }
    }
}
#endif