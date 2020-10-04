using UnityEngine;

namespace Hotfix
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
            GameObject obj = Model.AssetManager.Instance.LoadGameObject(name, false, isAssetBundle, AssetType.UI);
            if (obj == null) return null;
            obj.name = name;
            obj.transform.SetParent(Model.Objects.gui.transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            return obj;
        }
    }
}