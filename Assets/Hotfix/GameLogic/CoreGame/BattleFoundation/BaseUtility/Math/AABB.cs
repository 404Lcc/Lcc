using UnityEngine;

namespace LccHotfix
{
    public enum BattlePlane
    {
        XY,
        XZ,
    }
    
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

        public AABB(Vector3 pos, Vector2 minPoint, Vector2 maxPoint, BattlePlane plane)
        {
            var planePoint = ToPlanePoint(pos, plane);
            this.minPoint = planePoint + minPoint;
            this.maxPoint = planePoint + maxPoint;
        }

        public AABB(Vector3 pos, float radius, BattlePlane plane)
        {
            Vector2 halfSize = new Vector2(radius, radius);
            var planePoint = ToPlanePoint(pos, plane);
            this.minPoint = planePoint - halfSize;
            this.maxPoint = planePoint + halfSize;
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
        
        public static Vector2 ToPlanePoint(Vector3 point, BattlePlane plane)
        {
            return plane == BattlePlane.XY ? new Vector2(point.x, point.y) : new Vector2(point.x, point.z);
        }

        public static Vector3 ToWorldPoint(Vector2 point, float fixedAxis, BattlePlane plane)
        {
            return plane == BattlePlane.XY ? new Vector3(point.x, point.y, fixedAxis) : new Vector3(point.x, fixedAxis, point.y);
        }

        public static bool TryGetNormalizedPlaneDirection(Vector3 direction, BattlePlane plane, out Vector3 planeDirection)
        {
            planeDirection = plane == BattlePlane.XY ? new Vector3(direction.x, direction.y, 0f) : new Vector3(direction.x, 0f, direction.z);
            if (planeDirection.sqrMagnitude <= float.Epsilon)
            {
                return false;
            }

            planeDirection.Normalize();
            return true;
        }
        
        public static bool Intersect(AABB aabb, Vector3 begin, Vector3 end, BattlePlane plane, out Vector3 intersectionPoint)
        {
            intersectionPoint = Vector3.zero;
            if (!Intersect(aabb, ToPlanePoint(begin, plane), ToPlanePoint(end, plane), out var planePoint))
            {
                return false;
            }

            var fixedAxis = plane == BattlePlane.XY ? end.z : end.y;
            intersectionPoint = ToWorldPoint(planePoint, fixedAxis, plane);
            return true;
        }
        
        private static bool Intersect(AABB aabb, Vector2 begin, Vector2 end, out Vector2 intersectionPoint)
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

            float tMin = 0.0f;
            float tMax = 1.0f;

            // 分别检查x和y方向的相交区间
            for (int i = 0; i < 2; i++)
            {
                if (Mathf.Abs(direction[i]) <= float.Epsilon)
                {
                    if (begin[i] < aabb.minPoint[i] || begin[i] > aabb.maxPoint[i])
                    {
                        return false;
                    }

                    continue;
                }

                float invDirection = 1.0f / direction[i];
                float t1 = (aabb.minPoint[i] - begin[i]) * invDirection;
                float t2 = (aabb.maxPoint[i] - begin[i]) * invDirection;

                if (invDirection < 0.0f)
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


        public void DrawGizmo(BattlePlane plane, Color color = default)
        {
            if (IsDegenerate() || HasNegativeVolume())
            {
                return;
            }

            Gizmos.color = color.a < 0.001f ? Color.white : color;
            
            if (plane == BattlePlane.XY)
            {
                DrawXY(minPoint, maxPoint);
            }
            else
            {
                DrawXZ(minPoint, maxPoint);
            }
        }


        /// <summary>
        /// 按四个角点绘制XY平面矩形线框。
        /// </summary>
        private void DrawXY(Vector2 min, Vector2 max)
        {
            var p0 = new Vector3(min.x, min.y, 0);
            var p1 = new Vector3(max.x, min.y, 0);
            var p2 = new Vector3(max.x, max.y, 0);
            var p3 = new Vector3(min.x, max.y, 0);
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);
        }

        /// <summary>
        /// 按四个角点绘制XZ平面矩形线框。
        /// </summary>
        private void DrawXZ(Vector2 min, Vector2 max)
        {
            var p0 = new Vector3(min.x, 0, min.y);
            var p1 = new Vector3(max.x, 0, min.y);
            var p2 = new Vector3(max.x, 0, max.y);
            var p3 = new Vector3(min.x, 0, max.y);
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);
        }
    }
}
