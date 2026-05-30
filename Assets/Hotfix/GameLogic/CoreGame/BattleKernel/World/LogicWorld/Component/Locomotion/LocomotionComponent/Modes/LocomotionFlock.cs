using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class LocomotionFlock : LocomotionBase, ILocomotionSpeed
    {
        private const float maxSpeed = 5f;
        private const float maxSteerForce = 2f;
        private const float perceptionRadius = 2f;
        private const float cohesionWeight = 2f;
        private const float separationWeight = 8f;
        private const float alignmentWeight = 0.8f;
        private const float targetWeight = 2.2f;
        private const float separationRadius = 1f;

        private float _moveSpeed;
        private Vector3 _targetPos;
        private Quaternion _currentRotation;
        private Vector2 _velocity;
        private readonly Collider2D[] _collect = new Collider2D[5];
        private readonly List<LogicEntity> _nearbyBoids = new List<LogicEntity>();

        public override void Update(float dt, LogicEntity entity)
        {
            List<LogicEntity> nearbyBoids = GetNearbyBoids(entity);

            if (nearbyBoids.Count > 0)
            {
                Vector2 cohesion = CalculateCohesion(entity, nearbyBoids) * cohesionWeight;
                Vector2 separation = CalculateSeparation(entity, nearbyBoids) * separationWeight;
                Vector2 alignment = CalculateAlignment(nearbyBoids) * alignmentWeight;
                Vector2 targetSeek = CalculateTargetSeek(entity) * targetWeight;
                Vector2 acceleration = cohesion + separation + alignment + targetSeek;
                acceleration = Vector2.ClampMagnitude(acceleration, maxSteerForce);
                _velocity += acceleration * dt;
            }
            else
            {
                Vector2 targetSeek = CalculateTargetSeek(entity) * targetWeight;
                _velocity += targetSeek * dt;
            }

            _velocity = Vector2.ClampMagnitude(_velocity, maxSpeed);

            var comTrans = entity.comTransform;
            DeltaPosition = (Vector3)_velocity * dt;

            if (_velocity.normalized != Vector2.zero)
            {
                var targetRotation = Quaternion.FromToRotation(Vector3.right, _velocity.normalized);
                _currentRotation = Quaternion.Lerp(_currentRotation, targetRotation, 1.5f * Time.deltaTime);
                comTrans.SetRotation(_currentRotation);
            }
        }

        public void Init(Vector3 targetPos, Quaternion currentRotation)
        {
            _moveSpeed = 1;
            _targetPos = targetPos;
            _currentRotation = currentRotation;
            _velocity = Vector2.zero;
            IsRuning = true;
        }

        public virtual void SetMoveSpeed(float moveSpeed)
        {
            _moveSpeed = moveSpeed;
        }

        private List<LogicEntity> GetNearbyBoids(LogicEntity entity)
        {
            _nearbyBoids.Clear();
            Physics2D.OverlapCircleNonAlloc(entity.comTransform.position, perceptionRadius, _collect);

            if (!entity.hasComBattleUnitTag)
                return _nearbyBoids;

            foreach (Collider2D item in _collect)
            {
                if (item == null || item.gameObject == null)
                    continue;
                var target = entity.OwnerWorld.GetEntitiesWithComUnityObjectRelated(item.gameObject.GetInstanceID());
                if (target == null)
                    continue;
                if (target.hasComBattleUnitTag && target.GetBattleUnitTag(out var tag))
                {
                    if (tag.FighterId == entity.comBattleUnitTag.Tag.FighterId)
                    {
                        _nearbyBoids.Add(target);
                    }
                }
            }

            return _nearbyBoids;
        }

        private Vector2 CalculateCohesion(LogicEntity entity, List<LogicEntity> boids)
        {
            Vector2 center = Vector2.zero;

            foreach (LogicEntity boid in boids)
            {
                center += (Vector2)boid.comTransform.position;
            }

            center /= boids.Count;
            Vector2 direction = center - (Vector2)entity.comTransform.position;
            return direction.normalized;
        }

        private Vector2 CalculateSeparation(LogicEntity entity, List<LogicEntity> boids)
        {
            Vector2 steer = Vector2.zero;
            int count = 0;

            foreach (LogicEntity boid in boids)
            {
                float distance = Vector2.Distance(entity.comTransform.position, boid.comTransform.position);

                if (distance > 0 && distance < separationRadius)
                {
                    Vector2 difference = (Vector2)entity.comTransform.position - (Vector2)boid.comTransform.position;
                    difference /= distance;
                    steer += difference;
                    count++;
                }
            }

            if (count > 0)
            {
                steer /= count;
            }

            return steer.normalized;
        }

        private Vector2 CalculateAlignment(List<LogicEntity> boids)
        {
            Vector2 averageVelocity = Vector2.zero;

            foreach (LogicEntity boid in boids)
            {
                if (boid.hasComLocomotion && boid.comLocomotion.Locomotion is LocomotionFlock locomotionFlock)
                {
                    averageVelocity += locomotionFlock._velocity;
                }
            }

            averageVelocity /= boids.Count;
            Vector2 steer = averageVelocity - _velocity;
            return steer.normalized;
        }

        private Vector2 CalculateTargetSeek(LogicEntity entity)
        {
            Vector2 desired = (Vector2)_targetPos - (Vector2)entity.comTransform.position;
            desired = desired.normalized * _moveSpeed;
            Vector2 steer = desired - _velocity;
            return steer.normalized;
        }
    }
}
