using UnityEngine;

namespace LccHotfix
{
    public abstract class AObjectBaseHandler
    {
        public string[] types;
        public AObjectBaseHandler(params string[] types)
        {
            this.types = types;
        }
        public virtual GameObject CreateGameObject(string name, Transform parent)
        {
            GameObject gameObject = LccModel.AssetManager.Instance.InstantiateAsset(name, types);
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