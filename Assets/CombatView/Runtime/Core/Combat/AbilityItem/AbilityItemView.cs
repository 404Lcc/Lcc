using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class AbilityItemView : Entity
    {
        public GameObject GameObject => GetComponent<GameObjectComponent>().gameObject;
        public Transform Transform => GameObject.transform;
        public TransformViewComponent TransformViewComponent => GetComponent<TransformViewComponent>();

        public override void Awake()
        {
            base.Awake();


            AddComponent<TransformViewComponent>();

        }
    }
}