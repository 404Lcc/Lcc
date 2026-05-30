using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 阻尼弹射：向着一个方向迅速弹射出去，然后逐渐减速。用于泡泡出生一类
    /// </summary>
    public class LocomotionDamping : LocomotionBase
    {
        public float DampingRate { get; protected set; }
        public float Speed { get; protected set; }
        public Vector3 Dir { get; protected set; }
        public float FinalSpeed { get; protected set; }
        public Vector3 FinalDir { get; protected set; }

        private const float Eps = 1E-3f;

        public void SetCurrent(Vector2 currentDir, float currentSpeed)
        {
            Dir = currentDir.normalized;
            Speed = currentSpeed;
            IsRuning = true;
        }

        public void SetFinal(Vector2 finalDir, float finalSpeed)
        {
            FinalDir = finalDir.normalized;
            FinalSpeed = finalSpeed;
        }

        public void SetDampingRate(float dampingRate)
        {
            DampingRate = dampingRate;
        }

        public override void Update(float dt, LogicEntity entity)
        {
            if ((FinalSpeed - Speed) < Eps && (FinalDir - Dir).magnitude < Eps)
            {
                IsRuning = false;
                return;
            }

            Speed = Mathf.Lerp(Speed, FinalSpeed, dt * DampingRate);
            Dir = Vector3.Lerp(Dir, FinalDir, dt * DampingRate).normalized;

            DeltaPosition = Dir * Speed * dt;
            if (AffectDir)
            {
                DeltaRotation = Quaternion.FromToRotation(entity.comTransform.rotation * Vector3.right, Dir);
            }
        }

    }
}
