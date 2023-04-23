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

        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            ((GameObject)(object)p1).ConvertComponent(this);

            AddComponent<TransformViewComponent>();
        }
    }
}