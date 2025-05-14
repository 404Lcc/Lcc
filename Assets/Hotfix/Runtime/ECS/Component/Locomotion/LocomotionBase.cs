using UnityEngine;

namespace LccHotfix
{
    public abstract class LocomotionBase : ILocomotion
    {
        public bool IsRuning { get; set; } = true;
        public Vector3 CurPosition { get; set; } = Vector3.zero;
        public Quaternion CurRotation { get; set; } = Quaternion.identity;
        public Vector3 CurScale { get; set; } = Vector3.one;

        public virtual void Update(float dt)
        {

        }

        public virtual bool IsEnd()
        {
            return !IsRuning;
        }

        public void Dispose()
        {
        }
    }
}