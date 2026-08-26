using System.Collections.Generic;
using UnityEngine;

public class ModelBindPointGetter
{
    //(模型名称,点位) - 路径
    private static Dictionary<(string, string), string> _objNameWithBindPointNamePath = new Dictionary<(string, string), string>();

    /// <summary>
    /// 按资源名查挂点相对路径
    /// </summary>
    public static string GetBindPointPath(string prefabName, string bindPoint)
    {
        if (_objNameWithBindPointNamePath.TryGetValue((prefabName, bindPoint), out var path))
        {
            return path;
        }

        return null;
    }

    public static Transform GetBindPoint(Transform obj, string resName, string bindPoint)
    {
        var path = GetBindPointPath(resName, bindPoint);
        if (path == null)
            return null;
        return obj.Find(path);
    }

    static ModelBindPointGetter()
    {
        _objNameWithBindPointNamePath.Clear();

    }
}