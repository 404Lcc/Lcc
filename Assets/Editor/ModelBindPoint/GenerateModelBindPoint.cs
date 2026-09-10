using System.Text;
using UnityEditor;
using UnityEngine;

namespace LccEditor.ModelBindPoint
{
    public static class ModelBindPointGetterCode
    {
        public static string Class =
            @"
using System.Collections.Generic;
using UnityEngine;

public class ModelBindPointGetter
{
    private static Dictionary<(string, string), string> mObjNameWithBindPointNamePath =
        new Dictionary<(string, string), string>();

    /// <summary>
    /// 按资源名查挂点相对路径
    /// </summary>
    public static string GetBindPointPath(string prefabName, string bindPoint)
    {
        if(mObjNameWithBindPointNamePath.TryGetValue((prefabName, bindPoint), out var path))
        {
            return path;
        }
        return null;
    }

    /// <summary>
    /// 按资源名查挂点 Transform
    /// </summary>
    public static Transform GetBindPoint(Transform obj, string resName, string bindPoint)
    {
        var path = GetBindPointPath(resName, bindPoint);
        if (path == null)
            return null;
        return obj.Find(path);
    }

    //本文件是代码生成的，手动修改的内容 下次生成时会丢失 ！！！
    //本文件是代码生成的，手动修改的内容 下次生成时会丢失 ！！！
    //本文件是代码生成的，手动修改的内容 下次生成时会丢失 ！！！
    static ModelBindPointGetter()
    {
        mObjNameWithBindPointNamePath.Clear();
        #添加#
    }
}";

        public static string Add =
            @"
        mObjNameWithBindPointNamePath.Add((#预制体#, #挂点#), #路径#);";
    }


    public class GenerateModelBindPoint : EditorWindow
    {
        [MenuItem("Assets/生成模型挂点")]
        public static void Generate()
        {
            var setting = GetSetting();
            setting.Refresh();
            GenDirPath =
                System.IO.Path.GetFullPath(Application.dataPath + setting.CodeGeneratePath);
            var paths = setting.PrefabPaths;
            var bindPoints = setting.BindPointNameList;
            var code = GenerateCode(paths, bindPoints);
            SaveFileToPath(code);
        }

        static ModelBindPointSettings GetSetting()
        {
            var settingType = typeof(ModelBindPointSettings);
            var guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Create new {settingType.Name}.asset");
                var setting = ScriptableObject.CreateInstance<ModelBindPointSettings>();
                string filePath = $"Assets/{settingType.Name}.asset";
                AssetDatabase.CreateAsset(setting, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return setting;
            }
            else
            {
                if (guids.Length != 1)
                {
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Debug.LogWarning($"Found multiple file : {path}");
                    }

                    throw new System.Exception($"Found multiple {settingType.Name} files !");
                }

                string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var setting = AssetDatabase.LoadAssetAtPath<ModelBindPointSettings>(filePath);
                return setting;
            }
        }

        #region 代码生成

        private static string GenDirPath = "";


        private static void SaveFileToPath(string content)
        {
            string fileName = "ModelBindPointGetter.cs";
            string path = GenDirPath;

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            System.IO.File.WriteAllText(System.IO.Path.Combine(path, fileName), content, System.Text.Encoding.UTF8);

            Debug.Log("脚本保存成功:" + System.IO.Path.Combine(path, fileName));
            // EditorUtility.DisplayDialog("保存成功", "成功保存在:" + path + fileName, "确认");
            AssetDatabase.Refresh();
        }
        
        static string GenerateCode(string[] paths, string[] bindPoints)
        {
            string codeStr = "";

            string addStr = "";
            if (paths == null || bindPoints == null)
            {
                Debug.LogError("ModelBindPointSettings is incomplete. Check model root path and bind point list.");
                return ModelBindPointGetterCode.Class.Replace("#添加#", addStr);
            }

            for (int i = 0; i < paths.Length; i++)
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (obj == null)
                {
                    Debug.LogError($"Generate model bind point failed: invalid or unloaded prefab path = {paths[i]}");
                    continue;
                }

                for (int j = 0; j < bindPoints.Length; j++)
                {
                    var res = BFSBindPointFinder.Search(obj.transform, bindPoints[j]);
                    if (!string.IsNullOrEmpty(res))
                    {
                        Debug.LogError($"res is {res}");
                        addStr += GenerateAddCode(obj.name, bindPoints[j], res);
                    }
                }
            }

            codeStr = ModelBindPointGetterCode.Class.Replace("#添加#", addStr);
            return codeStr;
        }

        static string GenerateAddCode(string prefabName, string bindPoint, string path)
        {
            string codeStr = "";

            codeStr = ModelBindPointGetterCode.Add.Replace("#预制体#", $"\"{prefabName}\"");
            codeStr = codeStr.Replace("#挂点#", $"\"{bindPoint}\"");
            codeStr = codeStr.Replace("#路径#", $"\"{path}\"");
            return codeStr;
        }

        #endregion
    }
}
