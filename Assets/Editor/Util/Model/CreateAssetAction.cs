using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace LccEditor
{
    public class CreateScriptAction : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            //创建资源
            Object obj = CreateAssetFormTemplate(pathName, resourceFile);
            //高亮显示该资源
            ProjectWindowUtil.ShowCreatedAsset(obj);
        }
        public Object CreateAssetFormTemplate(string pathName, string resourceFile)
        {
            //获取要创建资源的绝对路径
            string fullName = Path.GetFullPath(pathName);
            //读取本地模版文件
            StreamReader reader = new StreamReader(resourceFile);
            string content = reader.ReadToEnd();
            reader.Close();
            //获取资源的文件名
            string fileName = Path.GetFileNameWithoutExtension(pathName);
            //替换默认的文件名
            content = content.Replace(resourceFile.Substring(resourceFile.IndexOf('N'), resourceFile.IndexOf('.') - resourceFile.IndexOf('N')), fileName);
            //写入新文件
            StreamWriter writer = new StreamWriter(fullName, false, Encoding.UTF8);
            writer.Write(content);
            writer.Close();
            //刷新本地资源
            AssetDatabase.ImportAsset(pathName);
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }
        public static string GetSelectedPath()
        {
            //默认路径为Assets
            string selectedPath = "Assets";
            //遍历选中的资源以返回路径
            foreach (Object item in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                selectedPath = AssetDatabase.GetAssetPath(item);
                if (!string.IsNullOrEmpty(selectedPath) && File.Exists(selectedPath))
                {
                    selectedPath = Path.GetDirectoryName(selectedPath);
                    break;
                }
            }
            return selectedPath;
        }
    }
}