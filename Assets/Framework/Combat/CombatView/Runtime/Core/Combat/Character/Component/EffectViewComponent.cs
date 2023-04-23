using BM;
using UnityEngine;

namespace LccModel
{
    public class EffectViewComponent : Component
    {
        public LoadHandler ShowEffect(string name, Vector3 pos)
        {
            GameObject effect = AssetManager.Instance.InstantiateAsset(out LoadHandler handler, name, AssetType.Effect);
            GameObject go = Object.Instantiate(effect);
            go.transform.position = pos;
            return handler;
        }
    }
}