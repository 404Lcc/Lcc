using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class CombatView : Entity
    {
        public GameObject gameObject => GetParent<GameObjectEntity>().gameObject;
        public Transform transform => gameObject.transform;

        public TransformViewComponent TransformViewComponent => GetComponent<TransformViewComponent>();
        public AnimationViewComponent AnimationViewComponent => GetComponent<AnimationViewComponent>();
        public SkinViewComponent SkinViewComponent => GetComponent<SkinViewComponent>();

        public override void Awake()
        {
            base.Awake();


            AddComponent<TransformViewComponent>();
            AddComponent<AnimationViewComponent>();
            AddComponent<SkinViewComponent>();
        }
    }
}