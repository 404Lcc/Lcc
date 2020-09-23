using UnityEngine;

namespace Hotfix
{
    public class ObjectBaseHandler
    {
        public bool keep;
        public bool assetBundleMode;
        public string[] types;
        public ObjectBaseHandler(bool keep, bool assetBundleMode, params string[] types)
        {
            this.keep = keep;
            this.assetBundleMode = assetBundleMode;
            this.types = types;
        }
        public virtual GameObject CreateContainer(string name)
        {
            GameObject obj = Model.AssetManager.Instance.LoadGameObject(name, false, assetBundleMode, AssetType.UI);
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