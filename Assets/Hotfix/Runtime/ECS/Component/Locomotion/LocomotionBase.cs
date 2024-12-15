using UnityEngine;

namespace LccHotfix
{
    public abstract class LocomotionBase : ILocomotion
    {
        public bool IsRuning { get; set; } = true;
        public Vector3 CurPosition { get; set; }
        public Quaternion CurRotation { get; set; }
        public Vector3 CurScale { get; set; }

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