using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 匀加速直线运动，带一个最大速度
    /// </summary>
    public class LocomotionStraightAccelerate : LocomotionStraightDir
    {
        public float SpeedRatio { get; set; } = 1;
        public float MaxSpeedRatio { get; set; }
        public float AccelerationRatio { get; set; }

        public override void Update(float dt, LogicEntity entity)
        {
            SpeedRatio += AccelerationRatio * dt;
            if (SpeedRatio > MaxSpeedRatio)
            {
                SpeedRatio = MaxSpeedRatio;
            }

            DeltaPosition = Dir * MoveSpeed * SpeedRatio * dt;
            if (AffectDir)
            {
                DeltaRotation = Quaternion.FromToRotation(entity.comTransform.rotation * Vector3.right, Dir);
            }
        }
    }
}
