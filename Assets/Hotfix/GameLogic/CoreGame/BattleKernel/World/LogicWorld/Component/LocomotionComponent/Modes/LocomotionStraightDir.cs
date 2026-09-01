using UnityEngine;

namespace LccHotfix
{
    public class LocomotionStraightDir : LocomotionBase, ILocomotionSpeed
    {
        public float MoveSpeed { get; set; }
        public Vector3 Dir { get; protected set; }


        public void SetDir(Vector3 dir)
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

        public override void Update(float dt, LogicEntity entity, MetaWorld metaWorld)
        {
            DeltaPosition = Dir * MoveSpeed * dt;
            if (AffectDir && Dir.sqrMagnitude > 0.0001f)
            {
                var targetRotation = Quaternion.LookRotation(Dir.normalized, Vector3.up);
                entity.comTransform.SetRotation(targetRotation);
            }
        }

    }
}
