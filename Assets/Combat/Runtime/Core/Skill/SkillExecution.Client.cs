﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using System;

namespace LccModel
{
    /// <summary>
    /// 技能执行体
    /// </summary>
    public partial class SkillExecution
    {
        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);


            AbilityEntity = p1 as SkillAbility;
            OwnerEntity = GetParent<CombatEntity>();
            OriginTime = Time.Instance.ClientNow();
        }


        public void LoadExecutionEffects()
        {
            AddComponent<ExecutionEffectComponent>();
        }
        public void Update() 
        {
            var nowSeconds = (double)(Time.Instance.ClientNow() - OriginTime) / 1000;

            if (nowSeconds >= ExecutionObject.TotalTime)
            {
                EndExecute();
            }
        }


        public void BeginExecute()
        {
            GetParent<CombatEntity>().SpellingExecution = this;
            if (SkillAbility != null)
            {
                SkillAbility.Spelling = true;
            }

            GetComponent<ExecutionEffectComponent>().BeginExecute();

            FireEvent(nameof(BeginExecute));
        }

        public void EndExecute()
        {
            GetParent<CombatEntity>().SpellingExecution = null;
            if (SkillAbility != null)
            {
                SkillAbility.Spelling = false;
            }
            SkillTargets.Clear();
            Dispose();

        }

        ///// <summary>   技能碰撞体生成事件   </summary>
        //public void SpawnCollisionItem(ExecuteClipData clipData)
        //{
        //    //Log.Debug($"SkillExecution SpawnCollisionItem {clipData.StartTime} {clipData.Duration}");

        //    var abilityItem = Entity.Create<AbilityItem>(this);
        //    abilityItem.AddComponent<AbilityItemCollisionExecuteComponent>(clipData);

        //    if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.PathFly) PathFlyProcess(abilityItem);
        //    if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.SelectedDirectionPathFly) PathFlyProcess(abilityItem);
        //    if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.TargetFly) TargetFlyProcess(abilityItem);
        //    if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.ForwardFly) ForwardFlyProcess(abilityItem);
        //    if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.SelectedPosition) FixedPositionProcess(abilityItem);
        //    if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.SelectedDirection) FixedDirectionProcess(abilityItem);

        //    CreateAbilityItemProxyObj(abilityItem);
        //}

        ///// <summary>   目标飞行碰撞体     </summary>
        //private void TargetFlyProcess(AbilityItem abilityItem)
        //{
        //    var clipData = abilityItem.Get<AbilityItemCollisionExecuteComponent>().ExecuteClipData;
        //    abilityItem.TargetEntity = InputTarget;
        //    abilityItem.Position = OwnerEntity.Position;
        //    abilityItem.AddComponent<MoveWithDotweenComponent>().DoMoveToWithTime(InputTarget, clipData.Duration);
        //}

        ///// <summary>   前向飞行碰撞体     </summary>
        //private void ForwardFlyProcess(AbilityItem abilityItem)
        //{
        //    abilityItem.Position = OwnerEntity.Position;
        //    var x = Mathf.Sin(Mathf.Deg2Rad * InputDirection);
        //    var z = Mathf.Cos(Mathf.Deg2Rad * InputDirection);
        //    var destination = abilityItem.Position + new Vector3(x, 0, z) * 30;
        //    abilityItem.AddComponent<MoveWithDotweenComponent>().DoMoveTo(destination, 1f).OnMoveFinish(() => { Entity.Destroy(abilityItem); });
        //}

        ///// <summary>   路径飞行     </summary>
        //private void PathFlyProcess(AbilityItem abilityItem)
        //{
        //    //abilityItem.Position = OwnerEntity.Position;
        //    var clipData = abilityItem.Get<AbilityItemCollisionExecuteComponent>().ExecuteClipData;
        //    var tempPoints = clipData.CollisionExecuteData.GetCtrlPoints();

        //    var angle = OwnerEntity.Rotation.eulerAngles.y - 90;
        //    var a = angle * MathF.PI / 180;
        //    //Log.Debug($"agree={agree}");
        //    //foreach (var item in tempPoints)
        //    //{
        //    //    var x = item.position.x;
        //    //    var y = item.position.z;
        //    //    var x1 = x * Mathf.Cos(a) - y * Mathf.Sin(a);
        //    //    var y1 = -(y * Mathf.Cos(a) + x * Mathf.Sin(a));
        //    //    Log.Debug($"x={x},y={y} x1={x1},y={y1}");
        //    //    item.position = OwnerEntity.Position + new Vector3(x1, item.position.y, y1);
        //    //}

        //    abilityItem.Position = tempPoints[0].position;
        //    var moveComp = abilityItem.Add<AbilityItemBezierMoveComponent>();
        //    moveComp.PositionEntity = abilityItem;
        //    moveComp.ctrlPoints = tempPoints;
        //    moveComp.OriginPosition = OwnerEntity.Position;
        //    moveComp.RotateAgree = a;
        //    moveComp.Speed = clipData.Duration / 10;
        //    moveComp.DOMove();
        //    abilityItem.Add<LifeTimeComponent>(clipData.Duration);
        //}

        ///// <summary>   固定位置碰撞体     </summary>
        //private void FixedPositionProcess(AbilityItem abilityItem)
        //{
        //    var clipData = abilityItem.Get<AbilityItemCollisionExecuteComponent>().ExecuteClipData;
        //    abilityItem.Position = InputPoint;
        //    abilityItem.AddComponent<LifeTimeComponent>(clipData.Duration);
        //}

        ///// <summary>   固定方向碰撞体     </summary>
        //private void FixedDirectionProcess(AbilityItem abilityItem)
        //{
        //    var clipData = abilityItem.Get<AbilityItemCollisionExecuteComponent>().ExecuteClipData;
        //    abilityItem.Position = OwnerEntity.Position;
        //    abilityItem.Rotation = OwnerEntity.Rotation;
        //    abilityItem.AddComponent<LifeTimeComponent>(clipData.Duration);
        //}

        ///// <summary>   创建技能碰撞体     </summary>
        //public GameObject CreateAbilityItemProxyObj(AbilityItem abilityItem)
        //{
        //    var proxyObj = new GameObject("AbilityItemProxy");
        //    proxyObj.transform.position = abilityItem.Position;
        //    proxyObj.transform.rotation = abilityItem.Rotation;
        //    proxyObj.AddComponent<AbilityItemProxyObj>().AbilityItem = abilityItem;
        //    var clipData = abilityItem.Get<AbilityItemCollisionExecuteComponent>().CollisionExecuteData;

        //    if (clipData.Shape == CollisionShape.Sphere)
        //    {
        //        proxyObj.AddComponent<SphereCollider>().enabled = false;
        //        proxyObj.GetComponent<SphereCollider>().radius = clipData.Radius;
        //    }
        //    if (clipData.Shape == CollisionShape.Box)
        //    {
        //        proxyObj.AddComponent<BoxCollider>().enabled = false;
        //        proxyObj.GetComponent<BoxCollider>().center = clipData.Center;
        //        proxyObj.GetComponent<BoxCollider>().size = clipData.Size;
        //    }

        //    proxyObj.AddComponent<OnTriggerEnterCallback>().OnTriggerEnterCallbackAction = (other) =>
        //    {
        //        var combatEntity = CombatContext.Instance.Object2Entities[other.gameObject];
        //        abilityItem.OnCollision(combatEntity);
        //    };

        //    proxyObj.GetComponent<Collider>().enabled = true;

        //    if (clipData.ObjAsset != null)
        //    {
        //        abilityItem.Name = clipData.ObjAsset.name;
        //        var effectObj = GameObject.Instantiate(clipData.ObjAsset, proxyObj.transform);
        //        effectObj.transform.localPosition = Vector3.zero;
        //        effectObj.transform.localRotation = Quaternion.identity;
        //    }

        //    return proxyObj;
        //}
    }
}
