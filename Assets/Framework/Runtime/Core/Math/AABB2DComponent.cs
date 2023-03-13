using UnityEngine;

namespace LccModel
{
    public class AABB2DComponent : AObjectBase
    {
        public Vector2 minPoint;
        public Vector2 maxPoint;


        public override void Awake<P1, P2>(P1 p1, P2 p2)
        {
            base.Awake(p1, p2);

            this.minPoint = (Vector2)(object)p1;
            this.maxPoint = (Vector2)(object)p2;
        }

        public float Width()
        {
            return maxPoint.x - minPoint.x;
        }

        public float Height()
        {
            return maxPoint.y - minPoint.y;
        }

        public bool Intersects(AABB2DComponent aabb)
        {
            return maxPoint.x >= aabb.minPoint.x && maxPoint.y >= aabb.minPoint.y && aabb.maxPoint.x >= minPoint.x && aabb.maxPoint.y >= minPoint.y;
        }

        public bool Contains(AABB2DComponent aabb)
        {
            return aabb.minPoint.x >= minPoint.x && aabb.minPoint.y >= minPoint.y && aabb.maxPoint.x <= maxPoint.x && aabb.maxPoint.y <= maxPoint.y;
        }

        public bool IsDegenerate()
        {
            return minPoint.x >= maxPoint.x || minPoint.y >= maxPoint.y;
        }

        public bool HasNegativeVolume()
        {
            return maxPoint.x < minPoint.x || maxPoint.y < minPoint.y;
        }

        public static AABB2DComponent operator +(AABB2DComponent aabb, Vector2 point)
        {
            AABB2DComponent a = new AABB2DComponent();
            a.minPoint = aabb.minPoint + point;
            a.maxPoint = aabb.maxPoint + point;
            return a;
        }

        public static AABB2DComponent operator -(AABB2DComponent aabb, Vector2 point)
        {
            AABB2DComponent a = new AABB2DComponent();
            a.minPoint = aabb.minPoint - point;
            a.maxPoint = aabb.maxPoint - point;
            return a;
        }
    }
}