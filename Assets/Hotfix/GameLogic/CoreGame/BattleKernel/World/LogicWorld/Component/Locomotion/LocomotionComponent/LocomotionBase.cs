using System;
using HotUpdate.Framework;
using UnityEngine;

namespace LccHotfix
{
    public abstract class LocomotionBase : ILocomotion
    {
        public bool IsRuning { get; set; } = true;
        
        public Vector3 DeltaPosition { get; set; } = Vector3.zero;
        public Quaternion DeltaRotation { get; set; } = Quaternion.identity;
        public bool AffectDir { get; set; } = true;
        public Action EndNotify { get; set; }
        
        public void BeforeUpdate()
        {
            DeltaPosition = Vector3.zero;
            DeltaRotation = Quaternion.identity;
        }

        public abstract void Update(float dt, LogicEntity entity);

        public virtual bool IsEnd()
        {
            return !IsRuning;
        }

        public void OnEnd()
        {
            if (EndNotify != null)
            {
                EndNotify();
            }
        }
        
        public void OnRecycle()
        {
            EndNotify = null;
            IsRuning = true;
        }
    }
}
