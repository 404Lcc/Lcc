using UnityEngine;

namespace LccHotfix
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

        public AABB(Vector3 pos, Vector2 minPoint, Vector2 maxPoint)
        {
            this.minPoint = new Vector2(pos.x, pos.y) + minPoint;
            this.maxPoint = new Vector2(pos.x, pos.y) + maxPoint;
        }

        public AABB(Vector2 pos, float radius)
        {
            Vector2 halfSize = new Vector2(radius, radius);
            this.minPoint = pos - halfSize;
            this.maxPoint = pos + halfSize;
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

        public bool Collision(AABB aabb)
        {
            bool intersects = Intersects(aabb);
            bool containsAABB = Contains(aabb);
            bool AABBContains = aabb.Contains(this);
            return intersects || containsAABB || AABBContains;
        }

        public bool IsDegenerate()
        {
            return minPoint.x >= maxPoint.x || minPoint.y >= maxPoint.y;
        }

        public bool HasNegativeVolume()
        {
            return maxPoint.x < minPoint.x || maxPoint.y < minPoint.y;
        }

        public static bool Intersect(AABB aabb, Vector2 begin, Vector2 end, out Vector2 intersectionPoint)
        {
            intersectionPoint = Vector2.zero;

            Vector2 direction = end - begin;
            float length = direction.magnitude;

            // 如果线段长度为0，检查点是否在AABB内
            if (length < float.Epsilon)
            {
                if (PointInAABB(begin, aabb))
                {
                    intersectionPoint = begin;
                    return true;
                }

                return false;
            }

            Vector2 invDirection = new Vector2(1.0f / direction.x, 1.0f / direction.y);

            float tMin = 0.0f;
            float tMax = 1.0f;

            // 分别检查x和y方向的相交区间
            for (int i = 0; i < 2; i++)
            {
                float t1 = (aabb.minPoint[i] - begin[i]) * invDirection[i];
                float t2 = (aabb.maxPoint[i] - begin[i]) * invDirection[i];

                if (invDirection[i] < 0.0f)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                tMin = Mathf.Max(tMin, t1);
                tMax = Mathf.Min(tMax, t2);

                if (tMin > tMax)
                {
                    return false;
                }
            }

            // 检查是否在有效范围内
            if (tMin >= 0.0f && tMin <= 1.0f)
            {
                intersectionPoint = begin + direction * tMin;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查点是否在AABB内
        /// </summary>
        /// <param name="point"></param>
        /// <param name="aabb"></param>
        /// <returns></returns>
        private static bool PointInAABB(Vector2 point, AABB aabb)
        {
            return point.x >= aabb.minPoint.x && point.x <= aabb.maxPoint.x && point.y >= aabb.minPoint.y && point.y <= aabb.maxPoint.y;
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
        
        
        /// <summary>
        /// 在 XY 平面（世界 Z=0）上绘制轴对齐矩形线框（与 min/max 一致，宽高可不等）。
        /// </summary>
        public void DrawGizmo(Color color = default)
        {
            if (IsDegenerate() || HasNegativeVolume())
            {
                return;
            }

            Gizmos.color = color.a < 0.001f ? Color.white : color;

            const float z = 0f;
            DrawRectCornersXY(z, minPoint, maxPoint);
        }

        /// <summary>
        /// 按四个角点绘制 XY 平面矩形线框。
        /// </summary>
        private static void DrawRectCornersXY(float z, Vector2 min, Vector2 max)
        {
            var p0 = new Vector3(min.x, min.y, z);
            var p1 = new Vector3(max.x, min.y, z);
            var p2 = new Vector3(max.x, max.y, z);
            var p3 = new Vector3(min.x, max.y, z);
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);
        }
    }
}
