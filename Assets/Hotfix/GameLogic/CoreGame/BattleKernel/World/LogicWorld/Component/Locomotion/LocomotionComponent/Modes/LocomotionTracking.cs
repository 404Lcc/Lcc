using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 跟踪目标运动
    /// </summary>
    public class LocomotionTracking : LocomotionBase, ILocomotionSpeed
    {
        public float MoveSpeed { get; set; }
        public long TargetId { get; set; }
        public Vector3 TargetPos { get; set; }

        public void SetTarget(LogicEntity target)
        {
            TargetId = target.ID;
            IsRuning = true;
        }

        public virtual void SetMoveSpeed(float moveSpeed)
        {
            if (moveSpeed <= 0)
            {
                MoveSpeed = 0;
                IsRuning = false;
                return;
            }
            MoveSpeed = moveSpeed;
        }

        public override void Update(float dt, LogicEntity entity)
        {
            var target = entity.OwnerWorld.GetEntityWithComID(TargetId);
            if (target is not null && target.IsValid() && target.hasComTransform)
            {
                TargetPos = target.comTransform.position;
            }

            var dir = TargetPos - entity.comTransform.position;
            DeltaPosition = dir.normalized * MoveSpeed * dt;
            if (dir.magnitude <= MoveSpeed * dt)
            {
                DeltaPosition = dir;
            }

            if (AffectDir)
            {
                DeltaRotation = Quaternion.FromToRotation(entity.comTransform.rotation * Vector3.right, dir);
            }
        }

    }
}
