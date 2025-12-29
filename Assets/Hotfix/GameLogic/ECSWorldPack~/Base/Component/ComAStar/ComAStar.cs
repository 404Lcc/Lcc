using System;
using System.Collections.Generic;
using System.Linq;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public class ComAStar : LogicComponent
    {
        public bool isActive;
        public AStarCtrl ctrl;

        public override void PostInitialize(LogicEntity owner)
        {
            base.PostInitialize(owner);
            ctrl = owner.comView.ActorView.GameObject.GetComponent<AStarCtrl>();
        }

        public void SetTarget(Vector3 target)
        {
            isActive = true;
            ctrl.destination = target;
        }

        public void ClearTarget()
        {
            isActive = false;
        }
    }

    public partial class LogicEntity
    {
        public ComAStar comAStar
        {
            get { return (ComAStar)GetComponent(LogicComponentsLookup.ComAStar); }
        }

        public bool hasComAStar
        {
            get { return HasComponent(LogicComponentsLookup.ComAStar); }
        }

        public void AddComAStar()
        {
            var index = LogicComponentsLookup.ComAStar;
            var component = (ComAStar)CreateComponent(index, typeof(ComAStar));
            AddComponent(index, component);
        }

        public void RemoveComAStar()
        {
            RemoveComponent(LogicComponentsLookup.ComAStar);
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex ComAStarIndex = new ComponentTypeIndex(typeof(ComAStar));
        public static int ComAStar => ComAStarIndex.index;
    }
}