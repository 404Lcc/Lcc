using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 直线运动，附带正弦摆动
    /// </summary>
    public class LocomotionStraightDirSineCurve : LocomotionStraightDir, ILocomotionSpeed
    {
        public float Timer;
        public float SinSpeedRadio = 0.2f;
        public float SinPeriod = 1.0f;

        public override void Update(float dt, LogicEntity entity)
        {
            Timer += dt;
            if (Timer > SinPeriod)
            {
                Timer = 0;
            }

            var verticalVelocity = new Vector3(Dir.y, -Dir.x, 0) * Mathf.Cos(Timer / SinPeriod * Mathf.PI * 2) * MoveSpeed * SinSpeedRadio;
            var velocity = Dir * MoveSpeed + verticalVelocity;

            DeltaPosition = velocity * dt;
            if (AffectDir)
            {
                DeltaRotation = Quaternion.FromToRotation(entity.comTransform.rotation * Vector3.right, Dir);
            }
        }

    }
}
