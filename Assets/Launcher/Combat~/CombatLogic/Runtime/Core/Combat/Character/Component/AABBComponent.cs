using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class AABBComponent : AObjectBase
    {
        public AABB aabb;


        public override void Awake<P1>(P1 p1)
        {
            base.Awake(p1);

            this.aabb = (AABB)(object)p1;
        }
        public bool Intersects(AABBComponent aabbComponent)
        {
            return aabb.Intersects(aabbComponent.aabb);
        }
    }
}