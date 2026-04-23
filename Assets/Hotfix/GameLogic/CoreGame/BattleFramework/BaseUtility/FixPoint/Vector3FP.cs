using System;
using System.Collections;
using System.Collections.Generic;

namespace CoreGame
{
    public struct Vector3FP : IEquatable<Vector3FP>
    {
        public FixPoint x;
        public FixPoint y;
        public FixPoint z;

        public Vector3FP(FixPoint term_x = default(FixPoint), FixPoint term_y = default(FixPoint), FixPoint term_z = default(FixPoint))
        {
            x = term_x;
            y = term_y;
            z = term_z;
        }

        public Vector3FP(Vector3FP rhs)
        {
            x = rhs.x;
            y = rhs.y;
            z = rhs.z;
        }

        public Vector3FP(Vector2FP v2)
        {
            x = v2.x;
            y = FixPoint.Zero;
            z = v2.z;
        }

        public static Vector3FP CreateXY(FixPoint term_x, FixPoint term_y)
        {
            return new Vector3FP(term_x, term_y, FixPoint.Zero);
        }

        public static Vector3FP CreateXZ(FixPoint term_x, FixPoint term_z)
        {
            return new Vector3FP(term_x, FixPoint.Zero, term_z);
        }

        public static readonly Vector3FP Zero = new Vector3FP(FixPoint.Zero, FixPoint.Zero, FixPoint.Zero);
        public static readonly Vector3FP One = new Vector3FP(FixPoint.One, FixPoint.One, FixPoint.One);

        public static readonly Vector3FP Forward = new Vector3FP(FixPoint.Zero, FixPoint.Zero, FixPoint.One);
        public static readonly Vector3FP Back = new Vector3FP(FixPoint.Zero, FixPoint.Zero, -FixPoint.One);

        public static readonly Vector3FP Up = new Vector3FP(FixPoint.Zero, FixPoint.One, FixPoint.Zero);
        public static readonly Vector3FP Down = new Vector3FP(FixPoint.Zero, -FixPoint.One, FixPoint.Zero);

        public static readonly Vector3FP Left = new Vector3FP(-FixPoint.One, FixPoint.Zero, FixPoint.Zero);
        public static readonly Vector3FP Right = new Vector3FP(FixPoint.One, FixPoint.Zero, FixPoint.Zero);

        public override int GetHashCode()
        {
            return (int)GetCRC();
        }
        public override bool Equals(object rhs)
        {
            return (rhs is Vector3FP) && Equals((Vector3FP)rhs);
        }
        public bool Equals(Vector3FP rhs)
        {
            return x == rhs.x && y == rhs.y && z == rhs.z;
        }
        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }

        public uint GetCRC(uint old_crc = 0)
        {
            old_crc = CRC.Calculate(x.RawValue, old_crc);
            old_crc = CRC.Calculate(y.RawValue, old_crc);
            old_crc = CRC.Calculate(z.RawValue, old_crc);
            return old_crc;
        }

        public static Vector3FP operator -(Vector3FP v3fp)
        {
            return new Vector3FP(-v3fp.x, -v3fp.y, -v3fp.z);
        }

        public static Vector3FP operator +(Vector3FP lhs, Vector3FP rhs)
        {
            return new Vector3FP(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        }
        public static Vector3FP operator -(Vector3FP lhs, Vector3FP rhs)
        {
            return new Vector3FP(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
        }
        public static Vector3FP operator *(FixPoint lhs, Vector3FP rhs)
        {
            return new Vector3FP(rhs.x * lhs, rhs.y * lhs, rhs.z * lhs);
        }
        public static Vector3FP operator *(Vector3FP lhs, FixPoint rhs)
        {
            return new Vector3FP(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
        }
        public static Vector3FP operator /(Vector3FP lhs, FixPoint rhs)
        {
            if (rhs == FixPoint.Zero)
                return new Vector3FP(FixPoint.MaxValue, FixPoint.MaxValue, FixPoint.MaxValue);
            else
                return new Vector3FP(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
        }

        public static bool operator ==(Vector3FP lhs, Vector3FP rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }
        public static bool operator !=(Vector3FP lhs, Vector3FP rhs)
        {
            return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
        }

        public FixPoint Normalize()
        {
            FixPoint length = Length();
            if (length == FixPoint.Zero)
                return FixPoint.Zero;
            x /= length;
            y /= length;
            z /= length;
            return length;
        }

        public FixPoint FastNormalize()
        {
            FixPoint length = FastLength();
            if (length == FixPoint.Zero)
                return FixPoint.Zero;
            x /= length;
            y /= length;
            z /= length;
            return length;
        }

        public FixPoint Dot(ref Vector3FP v3fp)
        {
            return x * v3fp.x + y * v3fp.y + z * v3fp.z;
        }

        public Vector3FP Cross(ref Vector3FP v3fp)
        {
            return new Vector3FP(
                y * v3fp.z - z * v3fp.y,
                z * v3fp.x - x * v3fp.z,
                x * v3fp.y - y * v3fp.x
            );
        }

        public FixPoint LengthSquare()
        {
            return x * x + y * y + z * z;
        }

        public FixPoint Length()
        {
            return FixPoint.Sqrt(x * x + y * y + z * z);
        }

        public FixPoint FastLength()
        {
            return FixPoint.FastDistance(FixPoint.FastDistance(x, y), z);
        }

        public FixPoint DistanceSquare(ref Vector3FP v3fp)
        {
            FixPoint dx = v3fp.x - x;
            FixPoint dy = v3fp.y - y;
            FixPoint dz = v3fp.z - z;
            return dx * dx + dy * dy + dz * dz;
        }

        public FixPoint Distance(ref Vector3FP v3fp)
        {
            FixPoint dx = v3fp.x - x;
            FixPoint dy = v3fp.y - y;
            FixPoint dz = v3fp.z - z;
            return FixPoint.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public FixPoint FastDistance(ref Vector3FP v3fp)
        {
            return FixPoint.FastDistance(FixPoint.FastDistance(v3fp.x - x, v3fp.y - y), v3fp.z - z);
        }

        public FixPoint FastDistance(Vector3FP v3fp)
        {
            return FixPoint.FastDistance(FixPoint.FastDistance(v3fp.x - x, v3fp.y - y), v3fp.z - z);
        }

        public void MakeZero()
        {
            x = y = z = FixPoint.Zero;
        }
        public void MakeOne()
        {
            x = y = z = FixPoint.One;
        }
        public void MakeUnitX()
        {
            x = FixPoint.One;
            y = z = FixPoint.Zero;
        }
        public void MakeUnitY()
        {
            y = FixPoint.One;
            x = z = FixPoint.Zero;
        }
        public void MakeUnitZ()
        {
            z = FixPoint.One;
            x = y = FixPoint.Zero;
        }

        public bool IsAllZero()
        {
            return x == FixPoint.Zero && y == FixPoint.Zero && z == FixPoint.Zero;
        }

        public Vector3FP YRotate(FixPoint radian)
        {
            FixPoint sin_v = FixPoint.Sin(radian);
            FixPoint cos_v = FixPoint.Cos(radian);
            return new Vector3FP(cos_v * x - sin_v * z, y, sin_v * x + cos_v * z);
        }

        public static Vector3FP Reflect(ref Vector3FP I, ref Vector3FP N)
        {
            return I - I.Dot(ref N) * N * FixPoint.Two;
        }

        public static bool InsideRegion(ref Vector3FP p, ref Vector3FP min_xyz/*right up forward*/, ref Vector3FP max_xyz/*left down back*/)
        {
            return !(p.x < min_xyz.x || p.y < min_xyz.y || p.z < min_xyz.z || p.x > max_xyz.x || p.y > max_xyz.y || p.z > max_xyz.z);
        }

        public static bool InsideRegionXZ(ref Vector3FP p, ref Vector3FP min_xz, ref Vector3FP max_xz)
        {
            return !(p.x < min_xz.x || p.z < min_xz.z || p.x > max_xz.x || p.z > max_xz.z);
        }

        public static bool InsideRegionXY(ref Vector3FP p, ref Vector3FP min_xy, ref Vector3FP max_xy)
        {
            return !(p.x < min_xy.x || p.y < min_xy.y || p.x > max_xy.x || p.y > max_xy.y);
        }

        public static bool InsideFovXZ(ref Vector3FP source, ref Vector3FP facing, FixPoint fov_degree, ref Vector3FP target)
        {
            return InsideFov2D(source.x, source.z, facing.x, facing.z, fov_degree, target.x, target.z);
        }

        public static bool FastInsideFovXZ(ref Vector3FP source, ref Vector3FP facing, FixPoint fov_degree, ref Vector3FP target)
        {
            return FastInsideFov2D(source.x, source.z, facing.x, facing.z, fov_degree, target.x, target.z);
        }

        public static bool InsideFovXY(ref Vector3FP source, ref Vector3FP facing, FixPoint fov_degree, ref Vector3FP target)
        {
            return InsideFov2D(source.x, source.y, facing.x, facing.y, fov_degree, target.x, target.y);
        }

        public static bool FastInsideFovXY(ref Vector3FP source, ref Vector3FP facing, FixPoint fov_degree, ref Vector3FP target)
        {
            return FastInsideFov2D(source.x, source.y, facing.x, facing.y, fov_degree, target.x, target.y);
        }

        public static bool InsideFov2D(FixPoint source_d1, FixPoint source_d2, FixPoint facing_d1, FixPoint facing_d2, FixPoint fov_degree, FixPoint target_d1, FixPoint target_d2)
        {
            FixPoint to_target_d1 = target_d1 - source_d1;
            FixPoint to_target_d2 = target_d2 - source_d2;
            FixPoint distance = FixPoint.Distance(to_target_d1, to_target_d2);
            to_target_d1 /= distance;
            to_target_d2 /= distance;
            return to_target_d1 * facing_d1 + to_target_d2 * facing_d2 >= FixPoint.Cos(FixPoint.Degree2Radian(fov_degree / FixPoint.Two));
        }

        public static bool FastInsideFov2D(FixPoint source_d1, FixPoint source_d2, FixPoint facing_d1, FixPoint facing_d2, FixPoint fov_degree, FixPoint target_d1, FixPoint target_d2)
        {
            FixPoint to_target_d1 = target_d1 - source_d1;
            FixPoint to_target_d2 = target_d2 - source_d2;
            FixPoint distance = FixPoint.FastDistance(to_target_d1, to_target_d2);
            to_target_d1 /= distance;
            to_target_d2 /= distance;
            return to_target_d1 * facing_d1 + to_target_d2 * facing_d2 >= FixPoint.Cos(FixPoint.Degree2Radian(fov_degree / FixPoint.Two));
        }

        public static void Lerp(ref Vector3FP from, ref Vector3FP to, FixPoint cur_t, FixPoint total_t, ref Vector3FP result)
        {
            if (cur_t >= total_t)
            {
                result = to;
                return;
            }
            FixPoint percent = cur_t / total_t;
            result.x = from.x + (to.x - from.x) * percent;
            result.y = from.y + (to.y - from.y) * percent;
            result.z = from.z + (to.z - from.z) * percent;
        }

        public static Vector3FP Lerp(ref Vector3FP from, ref Vector3FP to, FixPoint cur_t, FixPoint total_t)
        {
            if (cur_t >= total_t)
                return to;
            FixPoint percent = cur_t / total_t;
            return from + (to - from) * percent;
        }
    };
}