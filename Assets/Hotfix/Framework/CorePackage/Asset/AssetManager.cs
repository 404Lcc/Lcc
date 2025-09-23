using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    internal class AssetManager : Module, IAssetService
    {
        private IAssetHelper _assetHelper;

        internal override void Shutdown()
        {
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void SetHelper(IAssetHelper helper)
        {
            this._assetHelper = helper;
        }

        public Object LoadRes<T>(GameObject loader, string asset, out T res) where T : Object
        {
            return _assetHelper.LoadRes<T>(loader, asset, out res);
        }

        public Object StartLoadRes<T>(GameObject loader, string asset, Action<string, Object> onComplete) where T : Object
        {
            return _assetHelper.StartLoadRes<T>(loader, asset, onComplete);
        }

        public Object LoadGameObject(string asset, bool keepHierar, out GameObject res)
        {
            return _assetHelper.LoadGameObject(asset, keepHierar, out res);
        }

        public Object StartLoadGameObject(GameObject loader, string asset, Action<string, GameObject> onComplete, bool keepHierar)
        {
            return _assetHelper.StartLoadGameObject(loader, asset, onComplete, keepHierar);
        }

        public Object LoadObjects(GameObject loader, string asset, out Dictionary<string, Object> res)
        {
            return _assetHelper.LoadObjects(loader, asset, out res);
        }

        public Object LoadShader(GameObject loader, string asset, out Dictionary<string, Shader> res)
        {
            return _assetHelper.LoadShader(loader, asset, out res);
        }
    }
}