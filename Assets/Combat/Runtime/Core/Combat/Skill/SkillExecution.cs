using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class SkillExecution : Entity, IAbilityExecution, IUpdate
    {
        public Entity Ability { get; set; }
        public Combat Owner => GetParent<Combat>();



        public SkillAbility SkillAbility => (SkillAbility)Ability;

        public ExecutionConfigObject executionConfigObject;
        public List<Combat> inputSkillTargetList = new List<Combat>();
        public Combat inputTarget;
        public Vector3 inputPoint;
        public float inputDirection;
        public long originTime;


        public bool actionOccupy = true;


        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);
            Ability = p1 as SkillAbility;

            originTime = Time.Instance.ClientNow();
        }


        public void LoadExecutionEffect()
        {
            AddComponent<ExecutionEffectComponent>();
        }
        public void Update()
        {
            var nowSeconds = (double)(Time.Instance.ClientNow() - originTime) / 1000;

            if (nowSeconds >= executionConfigObject.TotalTime)
            {
                EndExecute();
            }
        }


        public void BeginExecute()
        {
            GetParent<Combat>().spellingSkillExecution = this;
            if (SkillAbility != null)
            {
                SkillAbility.spelling = true;
            }

            GetComponent<ExecutionEffectComponent>().BeginExecute();
        }

        public void EndExecute()
        {
            GetParent<Combat>().spellingSkillExecution = null;
            if (SkillAbility != null)
            {
                SkillAbility.spelling = false;
            }
            inputSkillTargetList.Clear();
            Dispose();

        }


        public void SpawnCollisionItem(ExecuteClipData clipData)
        {
            var abilityItem = CombatContext.Instance.AddAbilityItem(this, clipData);

            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.FixedPosition)
            {
                FixedPositionItem(abilityItem);
            }
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.FixedDirection)
            {
                FixedDirectionItem(abilityItem);
            }
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.TargetFly)
            {
                TargetFlyItem(abilityItem);
            }
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.ForwardFly)
            {
                ForwardFlyItem(abilityItem);
            }
            if (clipData.CollisionExecuteData.MoveType == CollisionMoveType.PathFly)
            {
                PathFlyItem(abilityItem);
            }


            //CreateAbilityItemProxy(abilityItem);
        }

        private void TargetFlyItem(AbilityItem abilityItem)
        {
            abilityItem.TransformComponent.position = Owner.TransformComponent.position;
            ExecuteClipData clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.AddComponent<AbilityItemMoveWithDotweenComponent>().DoMoveToWithTime(inputTarget.TransformComponent, clipData.Duration);
        }

        private void ForwardFlyItem(AbilityItem abilityItem)
        {
            abilityItem.TransformComponent.position = Owner.TransformComponent.position;
            var x = Mathf.Sin(Mathf.Deg2Rad * inputDirection);
            var z = Mathf.Cos(Mathf.Deg2Rad * inputDirection);
            var destination = abilityItem.TransformComponent.position + new Vector3(x, 0, z) * 30;
            abilityItem.AddComponent<AbilityItemMoveWithDotweenComponent>().DoMoveTo(destination, 1f).OnMoveFinish(() =>
            {
                abilityItem.Dispose();
            });
        }

        private void PathFlyItem(AbilityItem abilityItem)
        {
            abilityItem.TransformComponent.position = Owner.TransformComponent.position;
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            var pointList = clipData.CollisionExecuteData.GetPointList();

            var angle = Owner.TransformComponent.rotation.eulerAngles.y - 90;

            abilityItem.TransformComponent.position = pointList[0].position;
            var moveComp = abilityItem.AddComponent<AbilityItemBezierMoveComponent>();
            moveComp.abilityItem = abilityItem;
            moveComp.pointList = pointList;
            moveComp.originPosition = Owner.TransformComponent.position;
            moveComp.rotateAgree = angle * MathF.PI / 180;
            moveComp.speed = clipData.Duration / 10;
            moveComp.DOMove();
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, float>(clipData.Duration);
        }

        private void FixedPositionItem(AbilityItem abilityItem)
        {
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.TransformComponent.position = inputPoint;
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, float>(clipData.Duration);
        }


        private void FixedDirectionItem(AbilityItem abilityItem)
        {
            var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().executeClipData;
            abilityItem.TransformComponent.position = Owner.TransformComponent.position;
            abilityItem.TransformComponent.rotation = Owner.TransformComponent.rotation;
            abilityItem.AddComponent<AbilityItemLifeTimeComponent, float>(clipData.Duration);
        }

        //private GameObject CreateAbilityItemProxy(AbilityItem abilityItem)
        //{
        //    var proxyObj = new GameObject("AbilityItemProxy");
        //    proxyObj.transform.position = abilityItem.TransformComponent.position;
        //    proxyObj.transform.rotation = abilityItem.TransformComponent.rotation;
        //    proxyObj.AddComponent<AbilityItemProxy>().abilityItem = abilityItem;
        //    var clipData = abilityItem.GetComponent<AbilityItemCollisionExecuteComponent>().CollisionExecuteData;

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

        //    proxyObj.AddComponent<OnTriggerEnterCallback>().triggerEnterAction = (other) =>
        //    {

        //    };

        //    proxyObj.GetComponent<Collider>().enabled = true;

        //    if (clipData.AssetName != null)
        //    {
        //        var effectObj = GameObject.Instantiate(clipData.AssetName, proxyObj.transform);
        //        effectObj.transform.localPosition = Vector3.zero;
        //        effectObj.transform.localRotation = Quaternion.identity;
        //    }

        //    return proxyObj;
        //}
    }
}