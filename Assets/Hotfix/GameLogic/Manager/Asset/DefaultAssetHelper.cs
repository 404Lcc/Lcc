using System;
using System.Collections.Generic;
using LccModel;
using UnityEngine;
using Object = UnityEngine.Object;

public class DefaultAssetHelper : IAssetHelper
{
    public Object LoadRes<T>(GameObject loader, string asset, out T res) where T : Object
    {
        var handle = ResObject.LoadRes<T>(loader, asset);
        res = handle.GetAsset<T>();
        return handle;
    }

    public Object StartLoadRes<T>(GameObject loader, string asset, System.Action<string, Object> onComplete) where T : Object
    {
        var handle = ResObject.StartLoadRes<T>(loader, asset, onComplete);
        return handle;
    }

    public Object LoadGameObject(string asset, bool keepHierar, out GameObject res)
    {
        var handle = ResGameObject.LoadGameObject(asset, keepHierar);
        res = handle.ResGO;
        return handle;
    }

    public Object StartLoadGameObject(GameObject loader, string asset, Action<string, GameObject> onComplete, bool keepHierar)
    {
        var handle = ResGameObject.StartLoadGameObject(loader, asset, onComplete, keepHierar);
        return handle;
    }

    public Object LoadObjects(GameObject loader, string asset, out Dictionary<string, Object> res)
    {
        var handle = ResALLObject.LoadObjects(loader, asset);
        res = handle.FindObjects();
        return handle;
    }

    public Object LoadShader(GameObject loader, string asset, out Dictionary<string, Shader> res)
    {
        var handle = ResShader.LoadShader(loader, asset);
        res = handle.FindShaders();
        return handle;
    }
}