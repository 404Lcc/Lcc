using UnityEngine;

namespace LccModel
{
    public class TransformViewComponent : Component
    {
        public CombatView CombatView => GetParent<CombatView>();
        public Transform Transform => CombatView.GameObject.transform;


        public void SyncTransform(Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            Transform.position = position;
            Transform.rotation = rotation;
            Transform.localScale = localScale;
        }
    }
}