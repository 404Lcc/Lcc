using UnityEngine;

namespace LccHotfix
{
    public abstract class AObjectBaseHandler
    {
        public bool isKeep;
        public bool isAssetBundle;
        public string[] types;
        public AObjectBaseHandler(bool isKeep, bool isAssetBundle, params string[] types)
        {
            this.isKeep = isKeep;
            this.isAssetBundle = isAssetBundle;
            this.types = types;
        }
        public virtual GameObject CreateGameObject(string name, Transform parent)
        {
            GameObject gameObject = LccModel.AssetManager.Instance.LoadGameObject(name, false, isAssetBundle, AssetType.UI);
            if (gameObject == null) return null;
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            return gameObject;
        }
    }
}