using UnityEngine;

namespace LccHotfix
{
    /// <summary>
    /// 战斗平面上的定向包围盒（2D OBB）：中心 + 两条正交单位轴 + 沿轴半长。
    /// 供阵营空气墙等可旋转 Box 墙在 Locomotion 层做精确求交与溜边法线，替代世界轴 AABB 外包络。
    /// 协作：<see cref="MapFactionAirWall"/> 提供 BoxCollider 数据，<see cref="FactionAirWallRegistryComponent"/> 做位移截断。
    /// 边界：假定墙为竖直薄盒、旋转主要绕战斗平面法线（如 XZ 战斗绕 Y）；大角度 3D 倾斜使平面投影非矩形的墙不在本次覆盖范围。
    /// </summary>
    public struct BattlePlaneObb
    {
        // 平面坐标系中的 OBB 中心；XZ 战斗时分量为 (world.x, world.z)
        public Vector2 center;

        // 平面内第一条局部轴，单位向量；XZ 时通常对应 Box local X 在平面上的投影
        public Vector2 axisU;

        // 平面内第二条局部轴，单位向量，与 axisU 正交；XZ 时通常对应 Box local Z 的投影
        public Vector2 axisV;

        // 沿 axisU 的半长（世界单位，恒正）
        public float halfU;

        // 沿 axisV 的半长（世界单位，恒正）
        public float halfV;

        private const float DegenerateAxisSqr = 1e-8f;
        private const float DegenerateHalf = 1e-6f;

        /// <summary>
        /// 由 BoxCollider 构造战斗平面 OBB。使用 Transform 局部轴而非 bounds 外包络，以保留旋转朝向。
        /// </summary>
        /// <param name="collider">空气墙 BoxCollider，size 为局部尺寸。</param>
        /// <param name="plane">战斗平面；决定平面坐标分量与取哪两条局部轴。</param>
        /// <param name="obb">成功时输出 OBB；失败时为 default。</param>
        /// <returns>轴有效且半长非退化时 true。</returns>
        public static bool TryFromBoxCollider(BoxCollider collider, BattlePlane plane, out BattlePlaneObb obb)
        {
            obb = default;
            if (collider == null || !collider.enabled)
            {
                return false;
            }

            var transform = collider.transform;
            var localHalfSize = collider.size * 0.5f;

            // 用 TransformVector 把局部半长变到世界，再投影到战斗平面，保留旋转后的真实占地
            Vector3 worldAxisU;
            Vector3 worldAxisV;
            if (plane == BattlePlane.XY)
            {
                worldAxisU = transform.TransformVector(new Vector3(localHalfSize.x, 0f, 0f));
                worldAxisV = transform.TransformVector(new Vector3(0f, localHalfSize.y, 0f));
            }
            else
            {
                worldAxisU = transform.TransformVector(new Vector3(localHalfSize.x, 0f, 0f));
                worldAxisV = transform.TransformVector(new Vector3(0f, 0f, localHalfSize.z));
            }

            var extentU = AABB.ToPlanePoint(worldAxisU, plane);
            var extentV = AABB.ToPlanePoint(worldAxisV, plane);
            var lenU = extentU.magnitude;
            var lenV = extentV.magnitude;
            if (lenU < DegenerateHalf || lenV < DegenerateHalf)
            {
                return false;
            }

            var axisU = extentU / lenU;
            var axisV = extentV / lenV;
            // 两轴在平面上近共线时无法构成有效矩形（极端 3D 倾斜）
            if (Mathf.Abs(Vector2.Dot(axisU, axisV)) > 0.999f)
            {
                return false;
            }

            var bounds = collider.bounds;
            obb = new BattlePlaneObb
            {
                center = AABB.ToPlanePoint(bounds.center, plane),
                axisU = axisU,
                axisV = axisV,
                halfU = lenU,
                halfV = lenV,
            };
            return true;
        }

        /// <summary>
        /// 判断平面坐标点是否落在 OBB 闭区域内（含边界）。用于 Registry「已穿入墙内则零位移」判定。
        /// </summary>
        /// <param name="planePoint">战斗平面坐标点。</param>
        /// <returns>在 OBB 内或边上时 true。</returns>
        public bool ContainsPoint(Vector2 planePoint)
        {
            ToLocal(planePoint, out var localU, out var localV);
            // 局部坐标下 OBB 为 [-halfU, halfU] x [-halfV, halfV] 的轴对齐矩形
            return Mathf.Abs(localU) <= halfU + 1e-5f && Mathf.Abs(localV) <= halfV + 1e-5f;
        }

        /// <summary>
        /// 线段（世界坐标）与 OBB 的首次入口求交；输出世界命中点与平面入口外法线（指向墙外、背向盒体内部）。
        /// </summary>
        /// <param name="begin">线段起点（世界）。</param>
        /// <param name="end">线段终点（世界）。</param>
        /// <param name="plane">战斗平面。</param>
        /// <param name="hitWorld">入口交点（世界）；失败时为 zero。</param>
        /// <param name="entryPlaneNormal">入口面外法线（平面 Vector2）；失败时为 zero。</param>
        /// <returns>从 begin 外侧进入 OBB 时 true；不相交或 begin 已在内时 false。</returns>
        public bool TrySegmentIntersect(Vector3 begin, Vector3 end, BattlePlane plane, out Vector3 hitWorld, out Vector2 entryPlaneNormal)
        {
            hitWorld = Vector3.zero;
            entryPlaneNormal = Vector2.zero;

            var beginPlane = AABB.ToPlanePoint(begin, plane);
            var endPlane = AABB.ToPlanePoint(end, plane);

            // 退化线段：仅当起点在盒外且落在边界上时视为命中
            var segPlane = endPlane - beginPlane;
            if (segPlane.sqrMagnitude < DegenerateAxisSqr)
            {
                if (ContainsPoint(beginPlane))
                {
                    return false;
                }

                return false;
            }

            ToLocal(beginPlane, out var localBeginU, out var localBeginV);
            ToLocal(endPlane, out var localEndU, out var localEndV);
            var localDirU = localEndU - localBeginU;
            var localDirV = localEndV - localBeginV;

            // 在 OBB 局部空间做 slab 求交，等价于对轴对齐矩形 [-halfU,halfU]×[-halfV,halfV] 的 Liang-Barsky
            var tMin = 0f;
            var tMax = 1f;
            var entryAxis = -1;
            var entrySign = 0;

            if (!ClipSlab(localBeginU, localDirU, -halfU, halfU, ref tMin, ref tMax, ref entryAxis, ref entrySign, 0))
            {
                return false;
            }

            if (!ClipSlab(localBeginV, localDirV, -halfV, halfV, ref tMin, ref tMax, ref entryAxis, ref entrySign, 1))
            {
                return false;
            }

            // tMin 为从 begin 出发沿线段进入盒体的最早参数；须在 [0,1] 内才是本段位移上的阻挡
            if (tMin < 0f || tMin > 1f || tMin > tMax)
            {
                return false;
            }

            // 入口命中点：先在平面坐标中求，再还原世界坐标（固定轴取 end 的 z 或 y）
            var hitPlane = beginPlane + segPlane * tMin;
            var fixedAxis = plane == BattlePlane.XY ? end.z : end.y;
            hitWorld = AABB.ToWorldPoint(hitPlane, fixedAxis, plane);

            // 入口外法线：在局部空间指向被进入面的外侧，再线性组合回平面轴
            var localNormal = Vector2.zero;
            if (entryAxis == 0)
            {
                localNormal.x = entrySign;
            }
            else if (entryAxis == 1)
            {
                localNormal.y = entrySign;
            }

            entryPlaneNormal = localNormal.x * axisU + localNormal.y * axisV;
            if (entryPlaneNormal.sqrMagnitude > DegenerateAxisSqr)
            {
                entryPlaneNormal.Normalize();
            }

            return true;
        }

        /// <summary>
        /// 将平面外法线转换为世界 Vector3，供 ProjectOnPlane 溜边使用；竖直墙时垂直分量置零。
        /// </summary>
        public static Vector3 PlaneNormalToWorld(Vector2 planeNormal, BattlePlane plane)
        {
            if (plane == BattlePlane.XY)
            {
                return new Vector3(planeNormal.x, planeNormal.y, 0f);
            }

            return new Vector3(planeNormal.x, 0f, planeNormal.y);
        }

        // 平面点 → OBB 局部坐标 (u, v)，即相对 center 在 axisU/V 上的投影系数
        private void ToLocal(Vector2 planePoint, out float localU, out float localV)
        {
            var offset = planePoint - center;
            localU = Vector2.Dot(offset, axisU);
            localV = Vector2.Dot(offset, axisV);
        }

        // 单轴 slab 裁剪；更新线段参数区间 [tMin,tMax]，并在 tMin 被更新时记录入口轴与面符号（±1 为外法线局部分量）
        private static bool ClipSlab(
            float begin, float dir, float min, float max,
            ref float tMin, ref float tMax, ref int entryAxis, ref int entrySign, int axisIndex)
        {
            if (Mathf.Abs(dir) <= DegenerateAxisSqr)
            {
                // 运动方向与该轴平行：起点须在开口区间内，否则整条线段在盒外
                return begin >= min && begin <= max;
            }

            var invDir = 1f / dir;
            var t1 = (min - begin) * invDir;
            var t2 = (max - begin) * invDir;
            var sign1 = -1;
            var sign2 = 1;
            if (invDir < 0f)
            {
                var temp = t1;
                t1 = t2;
                t2 = temp;
                sign1 = 1;
                sign2 = -1;
            }

            if (t1 > tMin)
            {
                tMin = t1;
                entryAxis = axisIndex;
                entrySign = sign1;
            }

            if (t2 < tMax)
            {
                tMax = t2;
            }

            return tMin <= tMax;
        }
    }
}
