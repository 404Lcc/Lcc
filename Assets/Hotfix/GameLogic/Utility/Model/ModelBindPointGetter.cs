using System.Collections.Generic;
using UnityEngine;

public class ModelBindPointGetter
{
    //(模型名称,点位) - 路径
    private static Dictionary<(string, string), string> _objNameWithBindPointNamePath = new Dictionary<(string, string), string>();

    public static Transform GetBindPoint(Transform obj, string bindPoint)
    {
        if (_objNameWithBindPointNamePath.TryGetValue((obj.name, bindPoint), out var path))
        {
            return obj.Find(path);
        }

        return null;
    }

    static ModelBindPointGetter()
    {
        _objNameWithBindPointNamePath.Clear();

    }
}