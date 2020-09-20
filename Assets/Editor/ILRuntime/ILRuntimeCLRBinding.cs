#if UNITY_EDITOR
using ILRuntime.Runtime.Enviorment;
using Model;
using System.IO;
using System.Reflection;
using UnityEditor;

[Obfuscation(Exclude = true)]
public class ILRuntimeCLRBinding
{
   [MenuItem("ILRuntime/通过自动分析热更DLL生成CLR绑定")]
    public static void GenerateCLRBindingByAnalysis()
    {
        //用新的分析热更dll调用引用来生成绑定代码
        AppDomain domain = new AppDomain();
        using (FileStream fs = new FileStream("Assets/Resources/Text/Unity.Hotfix.dll.bytes", FileMode.Open, FileAccess.Read))
        {
            byte[] bytes = new byte[fs.ReadByte()];
            fs.Read(bytes, 0, bytes.Length);
            MemoryStream dll = new MemoryStream(GameUtil.RijndaelDecrypt("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", bytes));
            domain.LoadAssembly(dll);
            //Crossbind Adapter is needed to generate the correct binding code
            InitILRuntime(domain);
            ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, "Assets/Scripts/Runtime/ILRuntime/Generated");
        }
        AssetDatabase.Refresh();
    }
    public static void InitILRuntime(AppDomain domain)
    {
        //这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
    }
}
#endif