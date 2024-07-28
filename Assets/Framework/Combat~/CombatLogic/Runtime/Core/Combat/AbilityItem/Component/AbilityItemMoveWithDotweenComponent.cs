using UnityEngine;
using DG.Tweening;
using System;

namespace LccModel
{
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
        public TransformComponent transformComponent;
        public TransformComponent targetTransformComponent;

        public Vector3 destination;
        public Tweener moveTweener;
        private Action moveFinishAction;


        public override void Awake()
        {
            transformComponent = Parent.GetComponent<TransformComponent>();
        }

        public void Update()
        {
            if (targetTransformComponent != null)
            {
                if (speedType == SpeedType.Speed)
                {
                    DoMoveToWithSpeed(targetTransformComponent, speed);
                }
                if (speedType == SpeedType.Duration)
                {
                    DoMoveToWithTime(targetTransformComponent, duration);
                }
            }
        }

        public AbilityItemMoveWithDotweenComponent DoMoveTo(Vector3 destination, float duration)
        {
            this.destination = destination;
            DOTween.To(() => { return transformComponent.position; }, (x) => transformComponent.position = x, destination, duration).SetEase(Ease.Linear).OnComplete(OnMoveFinish);
            return this;
        }

        public void DoMoveToWithSpeed(TransformComponent target, float speed = 1f)
        {
            this.speed = speed;
            speedType = SpeedType.Speed;
            this.targetTransformComponent = target;
            moveTweener?.Kill();
            var dist = Vector3.Distance(transformComponent.position, targetTransformComponent.position);
            var duration = dist / speed;
            moveTweener = DOTween.To(() => { return transformComponent.position; }, (x) => transformComponent.position = x, targetTransformComponent.position, duration);
        }

        public void DoMoveToWithTime(TransformComponent target, float time = 1f)
        {
            duration = time;
            speedType = SpeedType.Duration;
            this.targetTransformComponent = target;
            moveTweener?.Kill();
            moveTweener = DOTween.To(() => { return transformComponent.position; }, (x) => transformComponent.position = x, targetTransformComponent.position, time);
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