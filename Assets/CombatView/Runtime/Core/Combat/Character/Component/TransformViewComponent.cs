using UnityEngine;

namespace LccModel
{
    public class TransformViewComponent : Component
    {
        public CombatView entity => GetParent<CombatView>();
        public Transform transform => entity.gameObject.transform;


        public void SyncTransform(Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = localScale;
        }
    }
}