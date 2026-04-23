using System;
using System.Collections;
using System.Collections.Generic;
namespace CoreGame
{
    /*
     * 实现基于Unity：左手系，Y轴指向天空
     */
    public struct Vector2FP : IEquatable<Vector2FP>
    {
        public FixPoint x;
        public FixPoint z;

        public Vector2FP(FixPoint term_x = default(FixPoint), FixPoint term_y = default(FixPoint))
        {
            x = term_x;
            z = term_y;
        }

        public Vector2FP(Vector2FP rhs)
        {
            x = rhs.x;
            z = rhs.z;
        }

        public Vector2FP(Vector3FP v3)
        {
            x = v3.x;
            z = v3.z;
        }

        public static readonly Vector2FP Zero = new Vector2FP(FixPoint.Zero, FixPoint.Zero);
        public static readonly Vector2FP One = new Vector2FP(FixPoint.One, FixPoint.One);

        public override int GetHashCode()
        {
            return (int)GetCRC();
        }
        public override bool Equals(object rhs)
        {
            return (rhs is Vector2FP) && Equals((Vector2FP)rhs);
        }
        public bool Equals(Vector2FP rhs)
        {
            return x == rhs.x && z == rhs.z;
        }
        public override string ToString()
        {
            return "(" + x + ", " + z + ")";
        }

        public uint GetCRC(uint old_crc = 0)
        {
            old_crc = CRC.Calculate(x.RawValue, old_crc);
            old_crc = CRC.Calculate(z.RawValue, old_crc);
            return old_crc;
        }

        public static Vector2FP operator -(Vector2FP v2fp)
        {
            return new Vector2FP(-v2fp.x, -v2fp.z);
        }

        public static Vector2FP operator +(Vector2FP lhs, Vector2FP rhs)
        {
            return new Vector2FP(lhs.x + rhs.x, lhs.z + rhs.z);
        }
        public static Vector2FP operator -(Vector2FP lhs, Vector2FP rhs)
        {
            return new Vector2FP(lhs.x - rhs.x, lhs.z - rhs.z);
        }
        public static Vector2FP operator *(Vector2FP lhs, FixPoint rhs)
        {
            return new Vector2FP(lhs.x * rhs, lhs.z * rhs);
        }
        public static Vector2FP operator *(FixPoint lhs, Vector2FP rhs)
        {
            return new Vector2FP(rhs.x * lhs, rhs.z * lhs);
        }
        public static Vector2FP operator /(Vector2FP lhs, FixPoint rhs)
        {
            if (rhs == FixPoint.Zero)
                return new Vector2FP(FixPoint.MaxValue, FixPoint.MaxValue);
            else
                return new Vector2FP(lhs.x / rhs, lhs.z / rhs);
        }
        public static bool operator ==(Vector2FP lhs, Vector2FP rhs)
        {
            return lhs.x == rhs.x && lhs.z == rhs.z;
        }
        public static bool operator !=(Vector2FP lhs, Vector2FP rhs)
        {
            return lhs.x != rhs.x || lhs.z != rhs.z;
        }

        public FixPoint Normalize()
        {
            FixPoint length = Length();
            if (length == FixPoint.Zero)
                return FixPoint.Zero;
            x /= length;
            z /= length;
            return length;
        }

        public FixPoint FastNormalize()
        {
            FixPoint length = FastLength();
            if (length == FixPoint.Zero)
                return FixPoint.Zero;
            x /= length;
            z /= length;
            return length;
        }

        public FixPoint Dot(ref Vector2FP v2fp)
        {
            return x * v2fp.x + z * v2fp.z;
        }

        public FixPoint LengthSquare()
        {
            return x * x + z * z;
        }

        public FixPoint Length()
        {
            return FixPoint.Distance(x, z);
        }

        public FixPoint FastLength()
        {
            return FixPoint.FastDistance(x, z);
        }

        public FixPoint DistanceSquare(ref Vector2FP v2fp)
        {
            FixPoint dx = v2fp.x - x;
            FixPoint dz = v2fp.z - z;
            return dx * dx + dz * dz;
        }

        public FixPoint Distance(ref Vector2FP v2fp)
        {
            return FixPoint.Distance(v2fp.x - x, v2fp.z - z);
        }

        public FixPoint FastDistance(ref Vector2FP v2fp)
        {
            return FixPoint.FastDistance(v2fp.x - x, v2fp.z - z);
        }

        public void MakeZero()
        {
            x = FixPoint.Zero;
            z = FixPoint.Zero;
        }

        public void Reverse()
        {
            x = -x;
            z = -z;
        }

        public enum VectorSign { CLOCKWISE = 1, ANTICLOCKWISE = -1 };
        public VectorSign Sign(ref Vector2FP v2fp)
        {
            //返回沿-Y的方向看，v2fp在当前向量的哪个方向
            if (z * v2fp.x > x * v2fp.z)
                return VectorSign.CLOCKWISE;
            else
                return VectorSign.ANTICLOCKWISE;
        }

        public Vector2FP Perpendicular()
        {
            return new Vector2FP(z, -x);
        }
        public void Perpendicular(ref Vector2FP v2fp)
        {
            v2fp.x = -z;
            v2fp.z = x;
        }

        public Vector2FP PerpendicularClockwise()
        {
            return new Vector2FP(z, -x);
        }
        public void PerpendicularClockwise(ref Vector2FP v2fp)
        {
            v2fp.x = z;
            v2fp.z = -x;
        }

        public Vector2FP PerpendicularAnticlockwise()
        {
            return new Vector2FP(-z, x);
        }
        public void PerpendicularAnticlockwise(ref Vector2FP v2fp)
        {
            v2fp.x = -z;
            v2fp.z = x;
        }

        public Vector2FP ParallelComponent(ref Vector2FP normalized_basis)
        {
            FixPoint projection = this.Dot(ref normalized_basis);
            return normalized_basis * projection;
        }

        public Vector2FP PerpendicularComponent(ref Vector2FP normalized_basis)
        {
            return this - ParallelComponent(ref normalized_basis);
        }

        public Vector2FP GetReflect(ref Vector2FP norm)
        {
            //入射是this，法向是norm，求反射
            return this - this.Dot(ref norm) * norm * FixPoint.Two;
        }

        public static Vector2FP Reflect(ref Vector2FP I, ref Vector2FP N)
        {
            return I - I.Dot(ref N) * N * FixPoint.Two;
        }

        public Vector2FP YRotate(FixPoint radian)
        {
            FixPoint sin_v = FixPoint.Sin(radian);
            FixPoint cos_v = FixPoint.Cos(radian);
            return new Vector2FP(cos_v * x - sin_v * z, sin_v * x + cos_v * z);
        }

        public FixPoint ToDegree()
        {
            //返回绕Y轴的旋转角度
            return FixPoint.XZToUnityRotationDegree(x, z);
        }

        public void FromDegree(FixPoint degree)
        {
            FixPoint radian = FixPoint.Degree2Radian(-degree);
            x = FixPoint.Cos(radian);
            z = FixPoint.Sin(radian);
        }

        public static bool InsideRegion(ref Vector2FP p, ref Vector2FP min_xz, ref Vector2FP max_xz)
        {
            return !(p.x < min_xz.x || p.z < min_xz.z || p.x > max_xz.x || p.z > max_xz.z);
        }

        public static bool InsideFov(ref Vector2FP source, ref Vector2FP facing, FixPoint fov_degree, ref Vector2FP target)
        {
            // 判断target是否在以source为中心的朝向为facing的角度范围fov_deg里面
            Vector2FP to_target = target - source;
            to_target.Normalize();
            return to_target.Dot(ref facing) >= FixPoint.Cos(FixPoint.Degree2Radian(fov_degree / FixPoint.Two));
        }

        public static bool FsstInsideFov(ref Vector2FP source, ref Vector2FP facing, FixPoint fov_degree, ref Vector2FP target)
        {
            Vector2FP to_target = target - source;
            to_target.FastNormalize();
            return to_target.Dot(ref facing) >= FixPoint.Cos(FixPoint.Degree2Radian(fov_degree / FixPoint.Two));
        }

        static public void Lerp(ref Vector2FP from, ref Vector2FP to, FixPoint cur_t, FixPoint total_t, ref Vector2FP result)
        {
            if (cur_t >= total_t)
            {
                result = to;
                return;
            }
            FixPoint percent = cur_t / total_t;
            result.x = from.x + (to.x - from.x) * percent;
            result.z = from.z + (to.z - from.z) * percent;
        }

        static public Vector2FP Lerp(ref Vector2FP from, ref Vector2FP to, FixPoint cur_t, FixPoint total_t)
        {
            if (cur_t >= total_t)
                return to;
            FixPoint percent = cur_t / total_t;
            return from + (to - from) * percent;
        }
    }
}