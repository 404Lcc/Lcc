using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class AssetObject : MonoBehaviour
    {
        public AssetHandle handle;
        public void OnDestroy()
        {
            if (handle != null)
            {
                AssetManager.Instance.UnLoadAsset(handle);
            }


        }
    }
}