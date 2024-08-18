using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class CombatView : Entity
    {
        public GameObject GameObject => GetComponent<GameObjectComponent>().gameObject;
        public Transform Transform => GameObject.transform;

        public TransformViewComponent TransformViewComponent => GetComponent<TransformViewComponent>();
        public AnimationViewComponent AnimationViewComponent => GetComponent<AnimationViewComponent>();
        public AttributeViewComponent AttributeViewComponent => GetComponent<AttributeViewComponent>();


        public SkinViewComponent SkinViewComponent => GetComponent<SkinViewComponent>();


        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            ((GameObject)(object)p1).ConvertComponent(this);

            AddComponent<TransformViewComponent>();
            AddComponent<AnimationViewComponent>();


            AddComponent<AttributeViewComponent>();


            AddComponent<SkinViewComponent>();


            AddChildren<HealthPointView>();
        }
    }
}