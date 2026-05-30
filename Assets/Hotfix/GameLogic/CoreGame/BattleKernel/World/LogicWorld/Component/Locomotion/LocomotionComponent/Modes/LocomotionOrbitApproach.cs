using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 围绕目标点做螺旋收敛运动，适合星球环绕后逐步贴近目标的表现。
    /// </summary>
    public class LocomotionOrbitApproach : LocomotionBase, ILocomotionSpeed
    {
        // 基础移动速度，仍然走 SysLocomotion 的统一速度体系。
        public float MoveSpeed { get; set; }
        public float SpeedMultiplier { get; private set; } = 1f;
        // 可选的动态目标实体；存在时每帧同步目标位置。
        public long TargetId { get; set; }
        // 当前追踪的目标点；既可来自 TargetId，也可直接指定固定点。
        public Vector3 TargetPos { get; set; }
        // 是否顺时针环绕目标。
        public bool Clockwise { get; private set; } = true;
        // 起始阶段切向权重更高，先形成明显环绕感。
        public float StartOrbitWeight { get; private set; } = 1.6f;
        // 接近目标后降低切向权重，避免在终点附近一直打转。
        public float EndOrbitWeight { get; private set; } = 0.6f;
        // 起始阶段径向权重较低，避免一开始就直冲目标。
        public float StartApproachWeight { get; private set; } = 0.12f;
        // 接近目标后提高径向权重，让轨迹自然收口。
        public float EndApproachWeight { get; private set; } = 0.7f;
        // 到达判定距离，进入该范围后直接贴到目标点并结束。
        public float ArriveDistance { get; private set; } = 0.05f;

        private bool mHasInitDistance;
        private float mInitDistance;

        public void SetTarget(LogicEntity target)
        {
            TargetId = target.ID;
            if (target.hasComTransform)
            {
                TargetPos = target.comTransform.position;
            }
            IsRuning = true;
            mHasInitDistance = false;
        }

        public void SetTargetPos(Vector3 targetPos)
        {
            TargetId = 0;
            TargetPos = targetPos;
            IsRuning = true;
            mHasInitDistance = false;
        }

        public void SetOrbitParams(bool clockwise, float startOrbitWeight = 1.6f, float endOrbitWeight = 0.6f,
            float startApproachWeight = 0.12f, float endApproachWeight = 0.7f, float arriveDistance = 0.05f)
        {
            Clockwise = clockwise;
            StartOrbitWeight = Mathf.Max(0f, startOrbitWeight);
            EndOrbitWeight = Mathf.Max(0f, endOrbitWeight);
            StartApproachWeight = Mathf.Max(0.001f, startApproachWeight);
            EndApproachWeight = Mathf.Max(0.001f, endApproachWeight);
            ArriveDistance = Mathf.Max(0.001f, arriveDistance);
        }

        public void SetSpeedMultiplier(float speedMultiplier)
        {
            SpeedMultiplier = Mathf.Max(0f, speedMultiplier);
        }

        public virtual void SetMoveSpeed(float moveSpeed)
        {
            if (moveSpeed <= 0f)
            {
                MoveSpeed = 0f;
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

            var toTarget = TargetPos - entity.comTransform.position;
            float distance = toTarget.magnitude;
            if (distance <= ArriveDistance)
            {
                DeltaPosition = toTarget;
                IsRuning = false;
                return;
            }

            if (!mHasInitDistance)
            {
                mHasInitDistance = true;
                mInitDistance = Mathf.Max(distance, ArriveDistance);
            }

            var radialDir = toTarget / distance;
            var tangentDir = Clockwise
                ? new Vector3(radialDir.y, -radialDir.x, 0f)
                : new Vector3(-radialDir.y, radialDir.x, 0f);

            // 用“初始距离 -> 当前距离”的进度，逐步把方向从环绕过渡到逼近，形成螺旋收敛轨迹。
            float progress = 1f - Mathf.Clamp01(distance / Mathf.Max(mInitDistance, ArriveDistance));
            float orbitWeight = Mathf.Lerp(StartOrbitWeight, EndOrbitWeight, progress);
            float approachWeight = Mathf.Lerp(StartApproachWeight, EndApproachWeight, progress);
            var moveDir = (tangentDir * orbitWeight + radialDir * approachWeight).normalized;

            var moveDelta = moveDir * (MoveSpeed * SpeedMultiplier * dt);
            if (moveDelta.magnitude >= distance)
            {
                DeltaPosition = toTarget;
                IsRuning = false;
            }
            else
            {
                DeltaPosition = moveDelta;
            }

            if (AffectDir && moveDir.sqrMagnitude > 0.0001f)
            {
                DeltaRotation = Quaternion.FromToRotation(entity.comTransform.rotation * Vector3.right, moveDir);
            }
        }
    }
}
