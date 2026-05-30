using System;
using HotUpdate.Framework;
using PBConfig;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace LccHotfix
{

public abstract class HitMaker
{
    internal int _hitCount;
    public RawHit[] RawHits { get; protected set; }
    public virtual void SetCapacity(int capacity)
    {
        RawHits = new RawHit[capacity];
    }
    public bool IsFull()
    {
        return _hitCount >= RawHits.Length;
    }
    public void Add(RawHit hit)
    {
        RawHits[_hitCount] = hit;
        _hitCount++;
    }
    public bool ContainsHit()
    {
        return _hitCount > 0;
    }

    public void Cleanup()
    {
        _hitCount = 0;
        Array.Clear(RawHits, 0, RawHits.Length);
    }

    public void OnRecycle()
    {
        Cleanup();
    }
    public abstract bool IsHit(LogicEntity ownerEntity, float dt, LogicEntity item, out RawHit hit);
}
public class AABBRawHitMaker : HitMaker, IRawHitMaker
{
     internal float _radius;
     public void SetRadius(float radius)
     {
         _radius = radius;
     }
     public virtual bool CheckRawHits(LogicEntity ownerEntity, float dt)
     {
         var group = ownerEntity.OwnerWorld.GetLogicGroup_Bounds_NoneSubobject();
         var entities = group.GetEntities();
         Vector3 position = ownerEntity.comTransform.position;
         for (int i = 0; i < entities.Length; i++)
         {
             var entity = entities[i];
             if(entity == ownerEntity)
                 continue;
 
             var comBounds = entity.comBounds;
             var bounds = comBounds.GetBounds();
   
             // 计算小球到障碍物的最近点
             Vector3 closest = new Vector3(Mathf.Clamp(position.x, bounds.minPoint.x, bounds.maxPoint.x), Mathf.Clamp(position.y, bounds.minPoint.y, bounds.maxPoint.y), position.z);
 
             // 计算距离和方向
             Vector3 dir = position - closest;
             float distance = dir.magnitude;
                 
             // 发生碰撞
             if (distance < _radius)
             {
                 RawHits[_hitCount] = new RawHit(entity, closest);
                 _hitCount++;
             }
             if (_hitCount >= RawHits.Length)
             {
                 break;
             }
         }
         return _hitCount > 0;
     }
    public override bool IsHit(LogicEntity ownerEntity, float dt, LogicEntity entity, out RawHit hit)
    {
        Vector3 position = ownerEntity.comTransform.position;
        var comBounds = entity.comBounds;
        var bounds = comBounds.GetBounds();

        // 计算小球到障碍物的最近点
        Vector3 closest = new Vector3(Mathf.Clamp(position.x, bounds.minPoint.x, bounds.maxPoint.x), Mathf.Clamp(position.y, bounds.minPoint.y, bounds.maxPoint.y), position.z);

        // 计算距离和方向
        Vector3 dir = position - closest;
        float distance = dir.magnitude;
        hit = default;
        bool isHit = false;
        // 发生碰撞
        if (distance < _radius)
        {
            isHit = true;
            hit = new RawHit(entity, closest);
        }
        return isHit;
    }
}
}
