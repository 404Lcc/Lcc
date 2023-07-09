using UnityEngine;

namespace LccModel
{
    public class AABB
    {
        public Vector2 minPoint;
        public Vector2 maxPoint;

        public AABB()
        {

        }
        public AABB(Vector2 minPoint, Vector2 maxPoint)
        {
            this.minPoint = minPoint;
            this.maxPoint = maxPoint;
        }

        public float Width()
        {
            return maxPoint.x - minPoint.x;
        }

        public float Height()
        {
            return maxPoint.y - minPoint.y;
        }

        public bool Intersects(AABB aabb)
        {
            return maxPoint.x >= aabb.minPoint.x && maxPoint.y >= aabb.minPoint.y && aabb.maxPoint.x >= minPoint.x && aabb.maxPoint.y >= minPoint.y;
        }

        public bool Contains(AABB aabb)
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

        public static AABB operator +(AABB aabb, Vector2 point)
        {
            AABB a = new AABB();
            a.minPoint = aabb.minPoint + point;
            a.maxPoint = aabb.maxPoint + point;
            return a;
        }

        public static AABB operator -(AABB aabb, Vector2 point)
        {
            AABB a = new AABB();
            a.minPoint = aabb.minPoint - point;
            a.maxPoint = aabb.maxPoint - point;
            return a;
        }
    }
}