using PBConfig;
using System;
using System.Collections.Generic;
using System.Numerics;
using static UnityEngine.UI.Image;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace LccHotfix
{

/// <summary>
/// 己方Raycast，对方AABB
/// </summary>
public class RaycastHitAABBMaker : HitMaker, IRawHitMaker
{
    public float Distance { get; set; } = 2f;

    public virtual void SetDistance(float distance)
    {
        Distance = distance;
    }

    public virtual bool CheckRawHits(LogicEntity ownerEntity, float dt)
    {
        var group = ownerEntity.OwnerWorld.GetLogicGroup_Bounds_NoneSubobject();
        var entities = group.GetEntities();
        Vector3 position = ownerEntity.comTransform.position;
        Vector3 dir = ownerEntity.comTransform.rotation * Vector3.right * Distance * dt / 0.016f;
        for (int i = 0; i < entities.Length; i++)
        {
            var item = entities[i];
            if (item == ownerEntity)
                continue;

            var comBounds = item.comBounds;
            var bounds = comBounds.GetBounds();
            var ownerBounds = ownerEntity.comBounds.GetBounds();
            if (AABB.Intersect(bounds, position - dir, position, out Vector2 intersectionPoint))
            {
                RawHits[_hitCount] = new RawHit(item, (Vector3)intersectionPoint);
                _hitCount++;
                if (_hitCount >= RawHits.Length)
                {
                    break;
                }
            }
        }


        SortRawHits(dir);

        return _hitCount > 0;
    }

    private static Comparer<RawHit> _rawHitComparerX1 = Comparer<RawHit>.Create((a, b) => a.Point.x.CompareTo(b.Point.x));
    private static Comparer<RawHit> _rawHitComparerX2 = Comparer<RawHit>.Create((a, b) => -a.Point.x.CompareTo(b.Point.x));
    private static Comparer<RawHit> _rawHitComparerY1 = Comparer<RawHit>.Create((a, b) => a.Point.y.CompareTo(b.Point.y));
    private static Comparer<RawHit> _rawHitComparerY2 = Comparer<RawHit>.Create((a, b) => -a.Point.y.CompareTo(b.Point.y));

    /// <summary>
    /// 点集按一个方向做逻辑排序
    /// 保证这些点都在一条直线上，所以做一些小trick，直接按x或y中的某一坐标排序
    /// </summary>
    /// <param name="dir">方向</param>
    public virtual void SortRawHits(Vector3 dir)
    {
        if (Math.Abs(dir.x) > Math.Abs(dir.y))
        {
            if (dir.x > 0)
            {
                Array.Sort(RawHits, 0, _hitCount, _rawHitComparerX1);
            }
            else
            {
                Array.Sort(RawHits, 0, _hitCount, _rawHitComparerX2);
            }
        }
        else
        {
            if (dir.y > 0)
            {
                Array.Sort(RawHits, 0, _hitCount, _rawHitComparerY1);
            }
            else
            {
                Array.Sort(RawHits, 0, _hitCount, _rawHitComparerY2);
            }
        }
    }


    public override bool IsHit(LogicEntity ownerEntity, float dt, LogicEntity item, out RawHit hit)
    {
        Vector3 dir = ownerEntity.comTransform.rotation * Vector3.right * Distance * dt / 0.016f;
        Vector3 position = ownerEntity.comTransform.position;
        var ownerBounds = ownerEntity.comBounds.GetBounds();
        bool ishit = false;
        hit = default;
        var comBounds = item.comBounds;
        var bounds = comBounds.GetBounds();
        if (AABB.Intersect(bounds, position - dir, position, out Vector2 intersectionPoint))
        {
            hit = new RawHit(item, (Vector3)intersectionPoint);
            ishit = true;
        }
        return ishit;
    }
  
}
}
