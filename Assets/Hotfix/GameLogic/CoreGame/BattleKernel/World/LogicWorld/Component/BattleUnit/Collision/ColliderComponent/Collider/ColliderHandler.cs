using System;
using System.Collections.Generic;
using HotUpdate.Framework;
using PBConfig;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace LccHotfix
{


public struct RawHit
{
    private LogicEntity _collider;
    private Vector3 _point;
    public LogicEntity Collider => _collider;
    public Vector3 Point => _point;

    public RawHit(LogicEntity collider, Vector3 point)
    {
        this._collider = collider;
        this._point = point;
    }
}

public abstract class ColliderHandler : IEntityColliderHandler
{
    public IRawHitMaker RawHitMaker { get; private set; }

    public virtual void SetRawHitMaker<T>() where T : class, IRawHitMaker, new()
    {
        if (RawHitMaker != null)
        {
            UnityEngine.Debug.LogError("已经存在一个GenRawHitHelper了 有问题请检查" + this.GetType().Name);
        }

        var helper = ReferencePool.Acquire<T>();
        helper.SetCapacity(32);
        RawHitMaker = helper;
    }

    public virtual bool CheckRawHits(LogicEntity ownerEntity, float dt)
    {
        return RawHitMaker.CheckRawHits(ownerEntity, dt);
    }

    public abstract void HandleRawHits(LogicEntity ownerEntity, RawHit[] rawHits, float dt);

    public void Cleanup()
    {
        RawHitMaker.Cleanup();
    }

    public virtual void OnRecycle()
    {
        Cleanup();
        ReferencePool.Release(RawHitMaker);
        RawHitMaker = null;
    }
}
}
