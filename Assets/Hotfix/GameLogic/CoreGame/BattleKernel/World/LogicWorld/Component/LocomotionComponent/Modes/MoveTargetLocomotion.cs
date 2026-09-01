using System;
using Entitas;
using UnityEngine;

namespace LccHotfix
{

public class MoveTargetLocomotion : LocomotionBase
{
    private float moveSpeed;

    private Vector3 target;

    private Action end;

    public void Init(float moveSpeed, Vector3 target, Action end)
    {
        this.moveSpeed = moveSpeed;
        this.target = target;
        this.end = end;
        IsRuning = true;
    }


    public override void Update(float dt, LogicEntity curPosition, MetaWorld metaWorld)
    {
        // 计算帧时间
        float deltaTime = Time.deltaTime;

        var dir = (target - curPosition.position).normalized;
        // 应用移动
        curPosition.SetPosition(curPosition.position + dir * deltaTime * moveSpeed);
   
        if (Vector3.Distance(curPosition.position, target) <= 1f)
        {
            IsRuning = false;
            end?.Invoke();
        }
    }
}
}
