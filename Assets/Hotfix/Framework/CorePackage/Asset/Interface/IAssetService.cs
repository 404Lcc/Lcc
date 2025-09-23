using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public interface IAssetService : IService
    {
        void SetHelper(IAssetHelper helper);
        Object LoadRes<T>(GameObject loader, string asset, out T res) where T : Object;
        Object StartLoadRes<T>(GameObject loader, string asset, Action<string, Object> onComplete) where T : Object;
        Object LoadGameObject(string asset, bool keepHierar, out GameObject res);

        Object StartLoadGameObject(GameObject loader, string asset, Action<string, GameObject> onComplete, bool keepHierar);
        Object LoadObjects(GameObject loader, string asset, out Dictionary<string, Object> res);
        Object LoadShader(GameObject loader, string asset, out Dictionary<string, Shader> res);
    }
}