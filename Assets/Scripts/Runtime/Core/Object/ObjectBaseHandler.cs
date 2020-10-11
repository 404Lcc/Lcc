using UnityEngine;

namespace Model
{
    public class ObjectBaseHandler
    {
        public bool isKeep;
        public bool isAssetBundle;
        public string[] types;
        public ObjectBaseHandler(bool isKeep, bool isAssetBundle, params string[] types)
        {
            this.isKeep = isKeep;
            this.isAssetBundle = isAssetBundle;
            this.types = types;
        }
        public virtual GameObject CreateContainer(string name)
        {
            GameObject gameObject = AssetManager.Instance.LoadGameObject(name, false, isAssetBundle, AssetType.UI);
            if (gameObject == null) return null;
            gameObject.name = name;
            gameObject.transform.SetParent(Objects.gui.transform);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            return gameObject;
        }
    }
}