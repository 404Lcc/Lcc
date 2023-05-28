using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class EffectViewComponent : Component
    {
        public AssetOperationHandle ShowEffect(string name, Vector3 pos)
        {
            GameObject effect = AssetManager.Instance.InstantiateAsset(out AssetOperationHandle handle, name, AssetType.Effect);
            GameObject go = Object.Instantiate(effect);
            go.transform.position = pos;
            return handle;
        }
    }
}