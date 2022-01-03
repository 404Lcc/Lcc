using LccModel;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using FileUtil = LccModel.FileUtil;

namespace LccEditor
{
    public class CreateScriptUtil : EndNameEditAction
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
            //获取资源的文件名
            string fileName = Path.GetFileNameWithoutExtension(pathName);
            //读取本地模版文件 替换默认的文件名
            string content = FileUtil.GetAsset(resourceFile).GetString().Replace("(Class)", fileName).Replace("(ViewModel)", fileName.Replace("Panel", string.Empty));
            //写入新文件
            FileUtil.SaveAsset(fullName, content);
            //刷新本地资源
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }
        public static string GetSelectedPath()
        {
            //默认路径为Assets
            string selectedPath = PathUtil.GetDataPath();
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