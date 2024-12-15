using System;
using UnityEngine;

namespace LccHotfix
{
    public class RawHit
    {
        private Vector2 _point;
        private Vector2 _normal;
    }
    public abstract class RaycastHitColliderHandler : IEntityColliderHandler
    {
        protected RaycastHit2D[] _rawHits;
        protected float _rayRadius;
        protected HitMethod _hitMethod;
        protected Vector3 _dir;
        protected int _layerMask;

        public virtual void InitMaxHits(int maxHitCount)
        {
            _rawHits = new RaycastHit2D[maxHitCount];
        }

        public void SetRayRadius(float rayRadius)
        {
            _rayRadius = rayRadius;
        }

        public void SetDir(Vector3 dir)
        {
            _dir = dir;
        }

        public void SetLayerMask(params string[] layerNames)
        {
            _layerMask = LayerMask.GetMask(layerNames);
        }

        public virtual bool CheckRawHits(LogicEntity ownerEntity, float dt)
        {
            if (!ownerEntity.hasComTransform)
                return false;

            var pos = ownerEntity.comTransform.position;

            if (_hitMethod == HitMethod.Point2D)
            {
                if (_rayRadius <= 0f)
                {
                    Physics2D.RaycastNonAlloc(pos, _dir, _rawHits, 0f);
                }
                else
                {
                    Physics2D.CircleCastNonAlloc(pos, _rayRadius, _dir, _rawHits, 0f);
                }

            }
            else if (_hitMethod == HitMethod.Line2D)
            {
                //Physics.RaycastNonAlloc
                Physics2D.RaycastNonAlloc(pos, _dir.normalized, _rawHits, int.MaxValue, _layerMask);
            }


            var hit = _rawHits[0].collider;

            if (hit == null)
                return false;

            if (hit.gameObject == null)
                return false;

            return true;
        }

        public abstract void HandleRawHits(LogicEntity ownerEntity, float dt);


        public virtual void Cleanup()
        {
            Array.Clear(_rawHits, 0, _rawHits.Length);
        }

        public virtual void Dispose()
        {
            if (_rawHits != null)
            {
                Array.Clear(_rawHits, 0, _rawHits.Length);
            }
        }
    }
}