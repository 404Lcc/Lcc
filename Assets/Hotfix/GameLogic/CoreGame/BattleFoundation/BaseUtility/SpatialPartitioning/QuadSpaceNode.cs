using UnityEngine;

namespace LccHotfix
{
    public class QuadSpaceNode : AQuadSpace
    {
        public int Length => 4;
        public AQuadSpace this[int index]
        {
            get
            {
                if (index == 0)
                    return quadrant0;
                if (index == 1)
                    return quadrant1;
                if (index == 2)
                    return quadrant2;
                if (index == 3)
                    return quadrant3;
                return null;
            }
        }
        public AQuadSpace quadrant0 { get; internal set; }
        public AQuadSpace quadrant1 { get; internal set; }
        public AQuadSpace quadrant2 { get; internal set; }
        public AQuadSpace quadrant3 { get; internal set; }

        protected override void AddObjectInternal(SpaceObject objec)
        {
            this.quadrant0.AddObject(objec);
            this.quadrant1.AddObject(objec);
            this.quadrant2.AddObject(objec);
            this.quadrant3.AddObject(objec);
        }
        protected override void CollectInternal(CollectResult results)
        {
            quadrant0.Collect(results);
            quadrant1.Collect(results);
            quadrant2.Collect(results);
            quadrant3.Collect(results);
        }

        public override void SetAABB(AABB fullSpace)
        {
            base.SetAABB(fullSpace);
            var min = aabb.minPoint;
            var max = aabb.maxPoint;
            var center = (min + max) * 0.5f;
            quadrant0.SetAABB(new AABB(center, max));
            quadrant1.SetAABB(new AABB(new Vector2(min.x, center.y), new Vector2(center.x, max.y)));
            quadrant2.SetAABB(new AABB(min, center));
            quadrant3.SetAABB(new AABB(new Vector2(center.x, min.y), new Vector2(max.x, center.y)));
        }

        public override void Clear()
        {
            base.Clear();
            quadrant0.Clear();
            quadrant1.Clear();
            quadrant2.Clear();
            quadrant3.Clear();
        }
    }
}
