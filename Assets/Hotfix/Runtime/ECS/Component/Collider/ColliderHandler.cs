using cfg;
using System;
using UnityEngine;

namespace LccHotfix
{
    public struct RawHit
    {
        private GameObject _collider;
        private Vector3 _point;
        public RawHit(GameObject collider, Vector3 point)
        {
            this._collider = collider;
            this._point = point;
        }
    }
    public abstract class ColliderHandler : IEntityColliderHandler
    {
        private ContactFilter2D _contactFilter2D;
        private Collider2D[] _collider2Ds;
        private RaycastHit2D[] _raycastHit2Ds;

        private Collider[] _colliders;
        private RaycastHit[] _raycastHits;

        protected RawHit[] _rawHits;

        protected CollisionType _collisionType;
        protected Vector3 _dir;
        protected int _layerMask;

        public virtual void InitMaxHits(int maxHitCount)
        {
            _contactFilter2D = new ContactFilter2D();
            _contactFilter2D.useTriggers = Physics2D.queriesHitTriggers;

            _collider2Ds = new Collider2D[maxHitCount];
            _raycastHit2Ds = new RaycastHit2D[maxHitCount];

            _colliders = new Collider[maxHitCount];
            _raycastHits = new RaycastHit[maxHitCount];

            _rawHits = new RawHit[maxHitCount];
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

            if (_collisionType == CollisionType.Collider2D)
            {
                var collider = ownerEntity.comView.ActorView.GameObject.GetComponent<Collider2D>();
                Physics2D.OverlapCollider(collider, _contactFilter2D, _collider2Ds);
                for (int i = 0; i < _collider2Ds.Length; i++)
                {
                    var item = _collider2Ds[i];
                    if (item == null)
                        continue;
                    if (item.gameObject == null)
                        continue;

                    _rawHits[i] = new RawHit(item.gameObject, item.bounds.ClosestPoint(pos));
                }
            }
            else if (_collisionType == CollisionType.Raycast2D)
            {
                Physics2D.RaycastNonAlloc(pos, _dir.normalized, _raycastHit2Ds, int.MaxValue, _layerMask);
                for (int i = 0; i < _raycastHit2Ds.Length; i++)
                {
                    var item = _raycastHit2Ds[i];
                    if (item.collider == null)
                        continue;

                    _rawHits[i] = new RawHit(item.collider.gameObject, item.point);
                }
            }



            if (_collisionType == CollisionType.BoxCollider)
            {
                var collider = ownerEntity.comView.ActorView.GameObject.GetComponent<BoxCollider>();
                Physics.OverlapBoxNonAlloc(collider.center, collider.size, _colliders);
                for (int i = 0; i < _colliders.Length; i++)
                {
                    var item = _colliders[i];
                    if (item == null)
                        continue;
                    if (item.gameObject == null)
                        continue;

                    _rawHits[i] = new RawHit(item.gameObject, item.bounds.ClosestPoint(pos));
                }
            }
            else if (_collisionType == CollisionType.Raycast)
            {
                Physics.RaycastNonAlloc(pos, _dir.normalized, _raycastHits, int.MaxValue, _layerMask);
                for (int i = 0; i < _raycastHits.Length; i++)
                {
                    var item = _raycastHits[i];
                    if (item.collider == null)
                        continue;

                    _rawHits[i] = new RawHit(item.collider.gameObject, item.point);
                }
            }

            return _rawHits.Length > 0;
        }

        public abstract void HandleRawHits(LogicEntity ownerEntity, float dt);


        public virtual void Cleanup()
        {
            Array.Clear(_collider2Ds, 0, _collider2Ds.Length);
            Array.Clear(_raycastHit2Ds, 0, _raycastHit2Ds.Length);

            Array.Clear(_colliders, 0, _colliders.Length);
            Array.Clear(_raycastHits, 0, _raycastHits.Length);

            Array.Clear(_rawHits, 0, _rawHits.Length);
        }

        public virtual void Dispose()
        {
            Cleanup();
        }
    }
}