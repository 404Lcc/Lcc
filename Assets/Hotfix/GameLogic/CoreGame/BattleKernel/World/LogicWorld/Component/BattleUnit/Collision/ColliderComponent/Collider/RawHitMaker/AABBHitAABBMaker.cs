using System;
using Vector3 = UnityEngine.Vector3;

namespace LccHotfix
{

public class AABBHitAABBMaker : IRawHitMaker
{
    internal int _hitCount;
    public RawHit[] RawHits { get; private set; }


    public virtual void SetCapacity(int capacity)
    {
        RawHits = new RawHit[capacity];
    }
    
    public virtual bool CheckRawHits(LogicEntity ownerEntity, float dt)
    {
        var group = ownerEntity.OwnerWorld.GetLogicGroup_Bounds_NoneSubobject();
        var entities = group.GetEntities();
        Vector3 position = ownerEntity.comTransform.position;
        for (int i = 0; i < entities.Length; i++)
        {
            if (i >= RawHits.Length)
            {
                continue;
            }

            var item = entities[i];
            if (item == ownerEntity)
                continue;

            var comBounds = item.comBounds;
            var bounds = comBounds.GetBounds();
            if (ownerEntity.hasComBounds)
            {
                var ownerBounds = ownerEntity.comBounds.GetBounds();
                if (ownerBounds.Collision(bounds))
                {
                    RawHits[i] = new RawHit(item, item.comTransform.position);
                    _hitCount++;
                }
            }
        }
        return _hitCount > 0;
    }


    public virtual void Cleanup()
    {
        _hitCount = 0;
        Array.Clear(RawHits, 0, RawHits.Length);
    }
    
    public virtual void OnRecycle()
    {
        Cleanup();
    }
}

// public class Collider2DGenRawHitHelper : IGenRawHitHelper
// {
//     private int _hitCount;
//     private ContactFilter2D _contactFilter2D;
//     private Collider2D[] _collider2Ds;
//
//     public RawHit[] RawHits { get; private set; }
//
//
//     public virtual void SetCapacity(int capacity)
//     {
//         _contactFilter2D = new ContactFilter2D();
//         _contactFilter2D.useTriggers = Physics2D.queriesHitTriggers;
//         _collider2Ds = new Collider2D[capacity];
//
//         RawHits = new RawHit[capacity];
//     }
//
//     public virtual bool CheckRawHits(LogicEntity ownerEntity, float dt)
//     {
//         if (!ownerEntity.hasComTransform)
//             return false;
//
//         var pos = ownerEntity.comTransform.position;
//         if (!ownerEntity.hasComView)
//             return false;
//         var view = ownerEntity.comView.MainActorView<IMainAnimatorView>();
//         if (view == null)
//             return false;
//         if (view.GameObject == null)
//             return false;
//         if (view.GameObject.GetComponent<Collider2D>() == null)
//             return false;
//
//         var collider = view.GameObject.GetComponent<Collider2D>();
//         Physics2D.OverlapCollider(collider, _contactFilter2D, _collider2Ds);
//         for (int i = 0; i < _collider2Ds.Length; i++)
//         {
//             var item = _collider2Ds[i];
//             if (item == null)
//                 continue;
//             if (item.gameObject == null)
//                 continue;
//
//             RawHits[i] = new RawHit(item.gameObject, item.bounds.ClosestPoint(pos));
//             _hitCount++;
//         }
//
//
//         return _hitCount > 0;
//     }
//
//
//     public virtual void Cleanup()
//     {
//         _hitCount = 0;
//         Array.Clear(_collider2Ds, 0, _collider2Ds.Length);
//         Array.Clear(RawHits, 0, RawHits.Length);
//     }
//     
//     public virtual void OnRecycle()
//     {
//         Cleanup();
//     }
// }
//
// public class RaycastHit2DGenRawHitHelper : IGenRawHitHelper
// {
//     private int _hitCount;
//     private RaycastHit2D[] _raycastHit2Ds;
//     private Vector3 _dir;
//     private int _layerMask;
//
//     
//     public RawHit[] RawHits { get; private set; }
//
//     public virtual void SetCapacity(int capacity)
//     {
//         _raycastHit2Ds = new RaycastHit2D[capacity];
//         RawHits = new RawHit[capacity];
//     }
//
//     public void SetDir(Vector3 dir)
//     {
//         _dir = dir;
//     }
//     
//     public void SetLayerMask(params string[] layerNames)
//     {
//         _layerMask = LayerMask.GetMask(layerNames);
//     }
//
//     public virtual bool CheckRawHits(LogicEntity ownerEntity, float dt)
//     {
//         if (!ownerEntity.hasComTransform)
//             return false;
//
//         var pos = ownerEntity.comTransform.position;
//         Physics2D.RaycastNonAlloc(pos, _dir.normalized, _raycastHit2Ds, int.MaxValue, _layerMask);
//         for (int i = 0; i < _raycastHit2Ds.Length; i++)
//         {
//             var item = _raycastHit2Ds[i];
//             if (item.collider == null)
//                 continue;
//
//             RawHits[i] = new RawHit(item.collider.gameObject, item.point);
//         }
//
//         return _hitCount > 0;
//     }
//
//
//     public virtual void Cleanup()
//     {
//         _hitCount = 0;
//         Array.Clear(_raycastHit2Ds, 0, _raycastHit2Ds.Length);
//         Array.Clear(RawHits, 0, RawHits.Length);
//
//     }
//     
//     public virtual void OnRecycle()
//     {
//         Cleanup();
//     }
// }
//
// public class ColliderGenRawHitHelper : IGenRawHitHelper
// {
//     private int _hitCount;
//     private Collider[] _colliders;
//
//     public RawHit[] RawHits { get; private set; }
//
//     public virtual void SetCapacity(int capacity)
//     {
//         _colliders = new Collider[capacity];
//         RawHits = new RawHit[capacity];
//     }
//
//     public virtual bool CheckRawHits(LogicEntity ownerEntity, float dt)
//     {
//         if (!ownerEntity.hasComTransform)
//             return false;
//
//         var pos = ownerEntity.comTransform.position;
//         if (!ownerEntity.hasComView)
//             return false;
//         var view = ownerEntity.comView.MainActorView<IMainAnimatorView>();
//         if (view == null)
//             return false;
//         if (view.GameObject == null)
//             return false;
//         if (view.GameObject.GetComponent<BoxCollider>() == null)
//             return false;
//
//         var collider = view.GameObject.GetComponent<BoxCollider>();
//         Physics.OverlapBoxNonAlloc(collider.center, collider.size, _colliders);
//         for (int i = 0; i < _colliders.Length; i++)
//         {
//             var item = _colliders[i];
//             if (item == null)
//                 continue;
//             if (item.gameObject == null)
//                 continue;
//
//             RawHits[i] = new RawHit(item.gameObject, item.bounds.ClosestPoint(pos));
//             _hitCount++;
//         }
//
//
//         return _hitCount > 0;
//     }
//
//
//     public virtual void Cleanup()
//     {
//         _hitCount = 0;
//         Array.Clear(_colliders, 0, _colliders.Length);
//         Array.Clear(RawHits, 0, RawHits.Length);
//     }
//     
//     public virtual void OnRecycle()
//     {
//         Cleanup();
//     }
// }
//
// public class RaycastHitGenRawHitHelper : IGenRawHitHelper
// {
//     private int _hitCount;
//     private RaycastHit[] _raycastHits;
//     private Vector3 _dir;
//     private int _layerMask;
//
//     public RawHit[] RawHits { get; private set; }
//
//     public virtual void SetCapacity(int capacity)
//     {
//         _raycastHits = new RaycastHit[capacity];
//         RawHits = new RawHit[capacity];
//     }
//
//     public void SetDir(Vector3 dir)
//     {
//         _dir = dir;
//     }
//     
//     public void SetLayerMask(params string[] layerNames)
//     {
//         _layerMask = LayerMask.GetMask(layerNames);
//     }
//
//
//     public virtual bool CheckRawHits(LogicEntity ownerEntity, float dt)
//     {
//         if (!ownerEntity.hasComTransform)
//             return false;
//
//         var pos = ownerEntity.comTransform.position;
//         Physics.RaycastNonAlloc(pos, _dir.normalized, _raycastHits, int.MaxValue, _layerMask);
//         for (int i = 0; i < _raycastHits.Length; i++)
//         {
//             var item = _raycastHits[i];
//             if (item.collider == null)
//                 continue;
//
//             RawHits[i] = new RawHit(item.collider.gameObject, item.point);
//             _hitCount++;
//         }
//
//
//         return _hitCount > 0;
//     }
//
//
//     public virtual void Cleanup()
//     {
//         _hitCount = 0;
//         Array.Clear(_raycastHits, 0, _raycastHits.Length);
//         Array.Clear(RawHits, 0, RawHits.Length);
//     }
//
//     public virtual void OnRecycle()
//     {
//         Cleanup();
//     }
// }

}
