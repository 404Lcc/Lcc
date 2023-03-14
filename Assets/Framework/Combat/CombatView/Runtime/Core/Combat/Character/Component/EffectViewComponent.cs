using UnityEngine;

namespace LccModel
{
    public class EffectViewComponent : Component
    {
        public void ShowEffect(string name, Vector3 pos)
        {
            GameObject effect = AssetManager.Instance.InstantiateAsset(name, AssetType.Effect);
            GameObject go = Object.Instantiate(effect);
            go.transform.position = pos;
        }
    }
}