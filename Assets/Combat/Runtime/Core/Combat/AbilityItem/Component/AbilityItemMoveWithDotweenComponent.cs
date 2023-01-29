using UnityEngine;
using DG.Tweening;
using System;

namespace LccModel
{
    public enum MoveType
    {
        TargetMove,
        PathMove,
    }

    public enum SpeedType
    {
        Speed,
        Duration,
    }

    public class AbilityItemMoveWithDotweenComponent : Component, IUpdate
    {
        public SpeedType speedType;
        public float speed;
        public float duration;
        public IPosition positionEntity;
        public IPosition targetPositionEntity;
        public Vector3 destination;
        public Tweener moveTweener;
        private Action moveFinishAction;


        public override void Awake()
        {
            positionEntity = (IPosition)Parent;
        }

        public void Update()
        {
            if (targetPositionEntity != null)
            {
                if (speedType == SpeedType.Speed) DoMoveToWithSpeed(targetPositionEntity, speed);
                if (speedType == SpeedType.Duration) DoMoveToWithTime(targetPositionEntity, duration);
            }
        }

        public AbilityItemMoveWithDotweenComponent DoMoveTo(Vector3 destination, float duration)
        {
            this.destination = destination;
            DOTween.To(() => { return positionEntity.Position; }, (x) => positionEntity.Position = x, destination, duration).SetEase(Ease.Linear).OnComplete(OnMoveFinish);
            return this;
        }

        public void DoMoveToWithSpeed(IPosition targetPositionEntity, float speed = 1f)
        {
            this.speed = speed;
            speedType = SpeedType.Speed;
            this.targetPositionEntity = targetPositionEntity;
            moveTweener?.Kill();
            var dist = Vector3.Distance(positionEntity.Position, this.targetPositionEntity.Position);
            var duration = dist / speed;
            moveTweener = DOTween.To(() => { return positionEntity.Position; }, (x) => positionEntity.Position = x, this.targetPositionEntity.Position, duration);
        }

        public void DoMoveToWithTime(IPosition targetPositionEntity, float time = 1f)
        {
            duration = time;
            speedType = SpeedType.Duration;
            this.targetPositionEntity = targetPositionEntity;
            moveTweener?.Kill();
            moveTweener = DOTween.To(() => { return positionEntity.Position; }, (x) => positionEntity.Position = x, this.targetPositionEntity.Position, time);
        }


        public void OnMoveFinish(Action action)
        {
            moveFinishAction = action;
        }

        private void OnMoveFinish()
        {
            moveFinishAction?.Invoke();
        }
    }
}