using UnityEngine;

namespace LccHotfix
{
    public class LocomotionStraightDir : LocomotionBase, ILocomotionSpeed
    {
        public float MoveSpeed { get; set; }
        public Vector3 Dir { get; protected set; }

        public void SetDir(Vector2 dir)
        {
            Dir = dir;
            IsRuning = true;
        }
        
        public virtual void SetMoveSpeed(float moveSpeed)
        {
            if (moveSpeed <= 0)
            {
                MoveSpeed = 0;
                // BattleLog.Error($"LocomotionStraightDir SetMoveSpeed moveSpeed <= 0, moveSpeed={moveSpeed}");
                return;
            }
            MoveSpeed = moveSpeed;
        }

        public override void Update(float dt, LogicEntity entity)
        {
            DeltaPosition = Dir * MoveSpeed * dt;
            if (AffectDir)
            {
                DeltaRotation = Quaternion.FromToRotation(entity.comTransform.rotation * Vector3.right, Dir);
            }
        }

    }
}
