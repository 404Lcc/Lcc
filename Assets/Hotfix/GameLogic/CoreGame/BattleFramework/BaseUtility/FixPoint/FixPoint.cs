using System;
using System.IO;

/*
 * FIXPOINT_IMPLICT_SUPPORTING_FLOAT：隐式支持浮点数
 * FIXPOINT_32BITS_FRACTIONAL：小数含32位精度
 * FIXPOINT_CHECK_OVERFLOW：检测溢出
 */
public partial struct FixPoint : IEquatable<FixPoint>, IComparable<FixPoint>
{
    public long m_raw_value;  //为了ProtoBuf，必须public

    public FixPoint(int value = 0)
    {
        m_raw_value = ((long)value) << FRACTIONAL_BITS;
    }

    public static FixPoint CreateFromFloat(float value)
    {
        return new FixPoint((long)(value * ONE));
    }

    public static FixPoint CreateFromFloat(double value)
    {
        return new FixPoint((long)(value * ONE));
    }

    public static FixPoint CreateFromFloat(decimal value)
    {
        return new FixPoint((long)(value * ONE));
    }

    public static FixPoint CreateFromRaw(long raw_value)
    {
        return new FixPoint(raw_value);
    }

    public long RawValue
    {
        get { return m_raw_value; }
    }

    public static FixPoint Parse(string str)
    {
        int sign = 0;
        bool fractional = false;
        int integer_part = 0;
        int fractional_part = 0;
        int fractional_base = 1;
        for (int i = 0; i < str.Length; ++i)
        {
            char ch = str[i];
            char code = GetCode(ch);
            if (code == Digit_____)
            {
                int num = ch - '0';
                if (fractional)
                {
                    fractional_part *= 10;
                    fractional_part += num;
                    fractional_base *= 10;
                }
                else
                {
                    integer_part *= 10;
                    integer_part += num;
                }
            }
            else if (code == Point_____)
            {
                if (fractional)
                    break;
                fractional = true;
            }
            else if (code == Sign______)
            {
                if (sign != 0 || integer_part != 0 || fractional)
                    break;
                if (ch == '-')
                    sign = -1;
                else
                    sign = 1;
            }
            else if (code == WhiteSpace)
                continue;
            else
                break;
        }
        long result = ((long)integer_part << FRACTIONAL_BITS);
        if (fractional_part != 0)
            result += (((long)fractional_part << (FRACTIONAL_BITS + 1)) / (long)fractional_base + 1) >> 1;
        if (sign < 0)
            result = -result;
        return new FixPoint(result);
    }

    public static bool TryParse(string str, out FixPoint result)
    {
        if (str == null)
        {
            result = FixPoint.Zero;
            return false;
        }
        result = Parse(str);
        return true;
    }

    public static readonly decimal Precision = (decimal)(new FixPoint(1L));
    public static readonly FixPoint MaxValue = new FixPoint(MAX_VALUE);
    public static readonly FixPoint MinValue = new FixPoint(MIN_VALUE);
    public static readonly FixPoint Zero = new FixPoint(0L);
    public static readonly FixPoint One = new FixPoint(ONE);
    public static readonly FixPoint MinusOne = new FixPoint(-ONE);
    public static readonly FixPoint Two = new FixPoint(ONE * 2L);
    public static readonly FixPoint Half = One / Two;
    public static readonly FixPoint Quarter = Half / Two;
    public static readonly FixPoint Ten = new FixPoint(ONE * 10L);
    public static readonly FixPoint Hundred = new FixPoint(ONE * 100L);
    public static readonly FixPoint Thousand = new FixPoint(ONE * 1000L);
    public static readonly FixPoint Million = new FixPoint(ONE * 1000000L);
    public static readonly FixPoint PrecisionFP = new FixPoint(1L);
    public static readonly FixPoint[] FixPointDigit = new[]{
        (FixPoint)0, (FixPoint)1, (FixPoint)2, (FixPoint)3, (FixPoint)4, (FixPoint)5, (FixPoint)6, (FixPoint)7, (FixPoint)8, (FixPoint)9
    };

    public static readonly FixPoint QuarterPi = new FixPoint(QUARTER_PI);
    public static readonly FixPoint HalfPi = new FixPoint(HALF_PI);
    public static readonly FixPoint Pi = new FixPoint(PI);
    public static readonly FixPoint OneAndHalfPi = new FixPoint(ONE_AND_HALF_PI);
    public static readonly FixPoint TwoPi = new FixPoint(TWO_PI);
    public static readonly FixPoint InvPi = new FixPoint(INV_PI);

    public static readonly FixPoint RadianPerDegree = new FixPoint(RADIAN_PER_DEGREE);
    public static readonly FixPoint DegreePerRadian = new FixPoint(DEGREE_PER_RADIAN);

    public static explicit operator FixPoint(int value)
    {
        return new FixPoint(((long)value) << FRACTIONAL_BITS);
    }
    public static explicit operator FixPoint(long value)
    {
        return new FixPoint(value << FRACTIONAL_BITS);
    }
    public static explicit operator FixPoint(bool value)
    {
        if (value)
            return FixPoint.One;
        else
            return FixPoint.Zero;
    }
    #region 正式的时候不开放这几个
#if FIXPOINT_IMPLICT_SUPPORTING_FLOAT
    public static explicit operator FixPoint(float value)
    {
        return new FixPoint((long)(value * ONE));
    }
    public static explicit operator FixPoint(double value)
    {
        return new FixPoint((long)(value * ONE));
    }
    public static explicit operator FixPoint(decimal value)
    {
        return new FixPoint((long)(value * ONE));
    }
#endif
    #endregion

    public static explicit operator int(FixPoint value)
    {
        return (int)(value.m_raw_value >> FRACTIONAL_BITS);
    }
    public static explicit operator long(FixPoint value)
    {
        return value.m_raw_value >> FRACTIONAL_BITS;
    }
    public static explicit operator bool(FixPoint value)
    {
        return value.m_raw_value != 0L;
    }
    public static explicit operator float(FixPoint value)
    {
        return (float)value.m_raw_value / ONE;
    }
    public static explicit operator double(FixPoint value)
    {
        return (double)value.m_raw_value / ONE;
    }
    public static explicit operator decimal(FixPoint value)
    {
        return (decimal)value.m_raw_value / ONE;
    }

    public override int GetHashCode()
    {
        return m_raw_value.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        return obj is FixPoint && ((FixPoint)obj).m_raw_value == m_raw_value;
    }
    public bool Equals(FixPoint other)
    {
        return m_raw_value == other.m_raw_value;
    }
    public int CompareTo(FixPoint other)
    {
        return m_raw_value.CompareTo(other.m_raw_value);
    }
    public override string ToString()
    {
        return ((decimal)this).ToString();
    }

    public static FixPoint operator -(FixPoint x)
    {
#if FIXPOINT_CHECK_OVERFLOW
        return x.m_raw_value == MIN_VALUE ? MaxValue : new FixPoint(-x.m_raw_value);
#else
        return new FixPoint(-x.m_raw_value);
#endif
    }

    public static FixPoint operator +(FixPoint x, FixPoint y)
    {
#if FIXPOINT_CHECK_OVERFLOW
        long x_raw = x.m_raw_value;
        long y_raw = y.m_raw_value;
        long z = x_raw + y_raw;
        if (((~(x_raw ^ y_raw) & (x_raw ^ z)) & MIN_VALUE) != 0)
            return x_raw > 0 ? MaxValue : MinValue;
        else
            return new FixPoint(z);
#else
        return new FixPoint(x.m_raw_value + y.m_raw_value);
#endif
    }

    public static FixPoint operator -(FixPoint x, FixPoint y)
    {
#if FIXPOINT_CHECK_OVERFLOW
        long x_raw = x.m_raw_value;
        long y_raw = y.m_raw_value;
        long z = x_raw - y_raw;
        if ((((x_raw ^ y_raw) & (x_raw ^ z)) & MIN_VALUE) != 0)
            return x_raw < 0 ? MinValue : MaxValue;
        else
            return new FixPoint(z);
#else
        return new FixPoint(x.m_raw_value - y.m_raw_value);
#endif
    }

    public static FixPoint operator *(FixPoint x, FixPoint y)
    {
#if FIXPOINT_32BITS_FRACTIONAL
        long x_raw = x.m_raw_value;
        long y_raw = y.m_raw_value;
        bool signs_equal = ((x_raw ^ y_raw) & MIN_VALUE) == 0;
        long mask = x_raw >> 63;
        x_raw = ((x_raw + mask) ^ mask);
        mask = y_raw >> 63;
        y_raw = ((y_raw + mask) ^ mask);
        ulong x_high = (ulong)x_raw >> FRACTIONAL_BITS;
        ulong x_low = (ulong)x_raw & FRACTIANAL_PART_MASK;
        ulong y_high = (ulong)y_raw >> FRACTIONAL_BITS;
        ulong y_low = (ulong)y_raw & FRACTIANAL_PART_MASK;
        ulong high_high = x_high * y_high;
#if FIXPOINT_CHECK_OVERFLOW
        if ((high_high >> INTEGER_BITS) != 0)
            return signs_equal ? MaxValue : MinValue;
#endif
        high_high <<= FRACTIONAL_BITS;
        ulong uz = high_high + x_high * y_low + x_low * y_high + ((x_low * y_low) >> FRACTIONAL_BITS);
#if FIXPOINT_CHECK_OVERFLOW
        if (uz < high_high || (uz >> 63) != 0)
            return signs_equal ? MaxValue : MinValue;
#endif
        long z = signs_equal ? (long)uz : -(long)uz;
        return new FixPoint(z);
#else
        //误差 < 0.02%
        return new FixPoint((x.m_raw_value * y.m_raw_value) >> FRACTIONAL_BITS);
#endif
    }

    public static FixPoint operator /(FixPoint x, FixPoint y)
    {
#if FIXPOINT_32BITS_FRACTIONAL
        long x_raw = x.m_raw_value;
        long y_raw = y.m_raw_value;
        if (y_raw == 0)
        {
            if (x_raw > 0)
                return MaxValue;
            else if (x_raw < 0)
                return MinValue;
            else
                return Zero;
        }
        ulong remainder = (ulong)(x_raw >= 0 ? x_raw : -x_raw);
        ulong divider = (ulong)(y_raw >= 0 ? y_raw : -y_raw);
        ulong quotient = 0UL;
        int magic_number = FRACTIONAL_BITS + 1;//[
        while ((divider & 0xF) == 0 && magic_number >= 4)
        {
            divider >>= 4;
            magic_number -= 4;
        }
        while (remainder != 0 && magic_number >= 0)
        {
            int shift = CountLeadingZeroes(remainder);
            if (shift > magic_number)
                shift = magic_number;
            remainder <<= shift;
            magic_number -= shift;
            ulong div = remainder / divider;
#if FIXPOINT_CHECK_OVERFLOW
            if ((div & ~(0xFFFFFFFFFFFFFFFF >> magic_number)) != 0)
                return ((x_raw ^ y_raw) & MIN_VALUE) == 0 ? MaxValue : MinValue;
#endif
            remainder = remainder % divider;
            quotient += div << magic_number;
            remainder <<= 1;
            --magic_number;
        }
        ++quotient; // rounding
        long z = (long)(quotient >> 1);//]
        if (((x_raw ^ y_raw) & MIN_VALUE) != 0)
            z = -z;
        return new FixPoint(z);
#else
        //误差 < 0.0000001%
        return new FixPoint((x.m_raw_value << FRACTIONAL_BITS) / y.m_raw_value);
#endif
    }

    public static FixPoint operator %(FixPoint x, FixPoint y)
    {
        return new FixPoint(x.m_raw_value % y.m_raw_value);
    }
    public static FixPoint operator &(FixPoint x, FixPoint y)
    {
        return new FixPoint(x.m_raw_value & y.m_raw_value);
    }
    public static FixPoint operator |(FixPoint x, FixPoint y)
    {
        return new FixPoint(x.m_raw_value | y.m_raw_value);
    }

    public static FixPoint operator <<(FixPoint fp, int bits)
    {
        return new FixPoint(fp.m_raw_value << bits);
    }
    public static FixPoint operator >>(FixPoint fp, int bits)
    {
        return new FixPoint(fp.m_raw_value >> bits);
    }

    public static bool operator ==(FixPoint x, FixPoint y)
    {
        return x.m_raw_value == y.m_raw_value;
    }
    public static bool operator !=(FixPoint x, FixPoint y)
    {
        return x.m_raw_value != y.m_raw_value;
    }
    public static bool operator >(FixPoint x, FixPoint y)
    {
        return x.m_raw_value > y.m_raw_value;
    }
    public static bool operator <(FixPoint x, FixPoint y)
    {
        return x.m_raw_value < y.m_raw_value;
    }
    public static bool operator >=(FixPoint x, FixPoint y)
    {
        return x.m_raw_value >= y.m_raw_value;
    }
    public static bool operator <=(FixPoint x, FixPoint y)
    {
        return x.m_raw_value <= y.m_raw_value;
    }

    #region int
    public static bool operator ==(FixPoint x, int y)
    {
        return x.m_raw_value == ((long)y) << FRACTIONAL_BITS;
    }
    public static bool operator !=(FixPoint x, int y)
    {
        return x.m_raw_value != ((long)y) << FRACTIONAL_BITS;
    }
    public static bool operator >(FixPoint x, int y)
    {
        return x.m_raw_value > ((long)y) << FRACTIONAL_BITS;
    }
    public static bool operator <(FixPoint x, int y)
    {
        return x.m_raw_value < ((long)y) << FRACTIONAL_BITS;
    }
    public static bool operator >=(FixPoint x, int y)
    {
        return x.m_raw_value >= ((long)y) << FRACTIONAL_BITS;
    }
    public static bool operator <=(FixPoint x, int y)
    {
        return x.m_raw_value <= ((long)y) << FRACTIONAL_BITS;
    }
    #endregion

    #region long
    public static bool operator ==(FixPoint x, long y)
    {
        return x.m_raw_value == y << FRACTIONAL_BITS;
    }
    public static bool operator !=(FixPoint x, long y)
    {
        return x.m_raw_value != y << FRACTIONAL_BITS;
    }
    public static bool operator >(FixPoint x, long y)
    {
        return x.m_raw_value > y << FRACTIONAL_BITS;
    }
    public static bool operator <(FixPoint x, long y)
    {
        return x.m_raw_value < y << FRACTIONAL_BITS;
    }
    public static bool operator >=(FixPoint x, long y)
    {
        return x.m_raw_value >= y << FRACTIONAL_BITS;
    }
    public static bool operator <=(FixPoint x, long y)
    {
        return x.m_raw_value <= y << FRACTIONAL_BITS;
    }
    #endregion

    #region float
#if FIXPOINT_IMPLICT_SUPPORTING_FLOAT
    public static bool operator ==(FixPoint x, float y)
    {
        return x.m_raw_value == (long)(y * ONE);
    }
    public static bool operator !=(FixPoint x, float y)
    {
        return x.m_raw_value != (long)(y * ONE);
    }
    public static bool operator >(FixPoint x, float y)
    {
        return x.m_raw_value > (long)(y * ONE);
    }
    public static bool operator <(FixPoint x, float y)
    {
        return x.m_raw_value < (long)(y * ONE);
    }
    public static bool operator >=(FixPoint x, float y)
    {
        return x.m_raw_value >= (long)(y * ONE);
    }
    public static bool operator <=(FixPoint x, float y)
    {
        return x.m_raw_value <= (long)(y * ONE);
    }
#endif
    #endregion

    #region double
#if FIXPOINT_IMPLICT_SUPPORTING_FLOAT
    public static bool operator ==(FixPoint x, double y)
    {
        return x.m_raw_value == (long)(y * ONE);
    }
    public static bool operator !=(FixPoint x, double y)
    {
        return x.m_raw_value != (long)(y * ONE);
    }
    public static bool operator >(FixPoint x, double y)
    {
        return x.m_raw_value > (long)(y * ONE);
    }
    public static bool operator <(FixPoint x, double y)
    {
        return x.m_raw_value < (long)(y * ONE);
    }
    public static bool operator >=(FixPoint x, double y)
    {
        return x.m_raw_value >= (long)(y * ONE);
    }
    public static bool operator <=(FixPoint x, double y)
    {
        return x.m_raw_value <= (long)(y * ONE);
    }
#endif
    #endregion

    #region decimal
#if FIXPOINT_IMPLICT_SUPPORTING_FLOAT
    public static bool operator ==(FixPoint x, decimal y)
    {
        return x.m_raw_value == (long)(y * ONE);
    }
    public static bool operator !=(FixPoint x, decimal y)
    {
        return x.m_raw_value != (long)(y * ONE);
    }
    public static bool operator >(FixPoint x, decimal y)
    {
        return x.m_raw_value > (long)(y * ONE);
    }
    public static bool operator <(FixPoint x, decimal y)
    {
        return x.m_raw_value < (long)(y * ONE);
    }
    public static bool operator >=(FixPoint x, decimal y)
    {
        return x.m_raw_value >= (long)(y * ONE);
    }
    public static bool operator <=(FixPoint x, decimal y)
    {
        return x.m_raw_value <= (long)(y * ONE);
    }
#endif
    #endregion

    public static int Sign(FixPoint value)
    {
        return -(int)((ulong)(value.m_raw_value) >> 63) | (int)((ulong)(-value.m_raw_value) >> 63);
    }

    public static FixPoint Abs(FixPoint value)
    {
#if FIXPOINT_CHECK_OVERFLOW
        if (value.m_raw_value == MIN_VALUE)
            return MaxValue;
#endif
        long mask = value.m_raw_value >> 63;
        return new FixPoint((value.m_raw_value + mask) ^ mask);
    }

    public static FixPoint Floor(FixPoint value)
    {
        return new FixPoint((long)((ulong)value.m_raw_value & INTEGER_PART_MASK));
    }

    public static FixPoint Ceiling(FixPoint value)
    {
        bool has_fractional_part = (value.m_raw_value & FRACTIANAL_PART_MASK) != 0;
        return has_fractional_part ? Floor(value) + One : value;
    }

    public static FixPoint Round(FixPoint value)
    {
        FixPoint integer_part = Floor(value);
        long fractional_part = value.m_raw_value & FRACTIANAL_PART_MASK;
        if (fractional_part > Half.m_raw_value)
            return integer_part + One;
        else if (fractional_part < Half.m_raw_value)
            return integer_part;
        else
            return (integer_part.m_raw_value & ONE) == 0 ? integer_part : integer_part + One;
    }

    public static FixPoint Sqrt(FixPoint value)
    {
        long x = value.m_raw_value << FRACTIONAL_BITS;
        if (x <= 1)
            return value;
        int s = 1;
        long x1 = x - 1;
        if (x1 > 4294967295) { s += 16; x1 >>= 32; }
        if (x1 > 65535) { s += 8; x1 >>= 16; }
        if (x1 > 255) { s += 4; x1 >>= 8; }
        if (x1 > 15) { s += 2; x1 >>= 4; }
        if (x1 > 3L) { s += 1; }
        long g0 = 1L << s;
        long g1 = (g0 + (x >> s)) >> 1;
        while (g1 < g0)
        {
            g0 = g1;
            g1 = (g0 + (x / g0)) >> 1;
        }
        return new FixPoint(g0);
    }

    public static FixPoint Distance(FixPoint x, FixPoint y)
    {
        return Sqrt(x * x + y * y);
    }

    public static FixPoint FastDistance(FixPoint x, FixPoint y)
    {
        //误差 < 8%
        long x1 = x.m_raw_value;
        long y1 = y.m_raw_value;
        if (x1 < 0) x1 = -x1;
        if (y1 < 0) y1 = -y1;
        long min_xy = x1;
        if (y1 < x1) min_xy = y1;
        long result = x1 + y1 - (min_xy >> 1) - (min_xy >> 2) + (min_xy >> 4);
        return new FixPoint(result);
    }

    public static FixPoint Sin(FixPoint radian)
    {
        //误差 < 0.02%
        long raw = radian.m_raw_value % TWO_PI;
        if (raw < 0)
            raw += TWO_PI;
        long p1 = raw % HALF_PI;
#if FIXPOINT_32BITS_FRACTIONAL
        p1 = p1 * SIN_TABLE_SIZE / HALF_PI;
#endif
        long p2 = raw / HALF_PI;
        if (p2 == 0)
            return new FixPoint(SinTable[p1]);
        else if (p2 == 1)
            return new FixPoint(SinTable[SIN_TABLE_SIZE - 1 - p1]);
        else if (p2 == 2)
            return new FixPoint(-SinTable[p1]);
        else
            return new FixPoint(-SinTable[SIN_TABLE_SIZE - 1 - p1]);
    }

    public static FixPoint Cos(FixPoint radian)
    {
        //误差 < 0.02%
        long raw = radian.m_raw_value % TWO_PI;
        if (raw < 0)
            raw += TWO_PI;
        long p1 = raw % HALF_PI;
#if FIXPOINT_32BITS_FRACTIONAL
        p1 = p1 * SIN_TABLE_SIZE / HALF_PI;
#endif
        long p2 = raw / HALF_PI;
        if (p2 == 0)
            return new FixPoint(SinTable[SIN_TABLE_SIZE - 1 - p1]);
        else if (p2 == 1)
            return new FixPoint(-SinTable[p1]);
        else if (p2 == 2)
            return new FixPoint(-SinTable[SIN_TABLE_SIZE - 1 - p1]);
        else
            return new FixPoint(SinTable[p1]);
    }

    public static FixPoint Tan(FixPoint radian)
    {
        FixPoint cos_value = Cos(radian);
        if (cos_value.m_raw_value == 0L)
            return FixPoint.MaxValue;
        FixPoint sin_value = Sin(radian);
        return sin_value / cos_value;
    }

    public static FixPoint Atan2(FixPoint y, FixPoint x)
    {
        //返回[0, FixPoint.TwoPi]
        //误差 < 0.005%
        long y1 = y.m_raw_value;
        long x1 = x.m_raw_value;
        if (x1 > 0)
        {
            if (y1 > 0)
            {
                if (y1 > x1)
#if FIXPOINT_32BITS_FRACTIONAL
                    if (x1 > ATAN2_HELPER_NUMBER)
                        return new FixPoint(HALF_PI - Atan2Table[x1 / (y1 >> 16)]);
                    else
                        return new FixPoint(HALF_PI - Atan2Table[(x1 << 16) / y1]);
#else
                    return new FixPoint(HALF_PI - Atan2Table[(x1 << FRACTIONAL_BITS) / y1]);
#endif
                else
#if FIXPOINT_32BITS_FRACTIONAL
                    if (y1 > ATAN2_HELPER_NUMBER)
                        return new FixPoint(Atan2Table[y1 / (x1 >> 16)]);
                    else
                        return new FixPoint(Atan2Table[(y1 << 16) / x1]);
#else
                    return new FixPoint(Atan2Table[(y1 << FRACTIONAL_BITS) / x1]);
#endif
            }
            else if (y1 < 0)
            {
                y1 = -y1;
                if (y1 > x1)
#if FIXPOINT_32BITS_FRACTIONAL
                    if (x1 > ATAN2_HELPER_NUMBER)
                        return new FixPoint(ONE_AND_HALF_PI + Atan2Table[x1 / (y1 >> 16)]);
                    else
                        return new FixPoint(ONE_AND_HALF_PI + Atan2Table[(x1 << 16) / y1]);
#else
                    return new FixPoint(ONE_AND_HALF_PI + Atan2Table[(x1 << FRACTIONAL_BITS) / y1]);
#endif
                else
#if FIXPOINT_32BITS_FRACTIONAL
                    if (y1 > ATAN2_HELPER_NUMBER)
                        return new FixPoint(TWO_PI - Atan2Table[y1 / (x1 >> 16)]);
                    else
                        return new FixPoint(TWO_PI - Atan2Table[(y1 << 16) / x1]);
#else
                    return new FixPoint(TWO_PI - Atan2Table[(y1 << FRACTIONAL_BITS) / x1]);
#endif
            }
            else
            {
                return Zero;
            }
        }
        else if (x1 < 0)
        {
            x1 = -x1;
            if (y1 > 0)
            {
                if (y1 > x1)
#if FIXPOINT_32BITS_FRACTIONAL
                    if (x1 > ATAN2_HELPER_NUMBER)
                        return new FixPoint(HALF_PI + Atan2Table[x1 / (y1 >> 16)]);
                    else
                        return new FixPoint(HALF_PI + Atan2Table[(x1 << 16) / y1]);
#else
                    return new FixPoint(HALF_PI + Atan2Table[(x1 << FRACTIONAL_BITS) / y1]);
#endif
                else
#if FIXPOINT_32BITS_FRACTIONAL
                    if (y1 > ATAN2_HELPER_NUMBER)
                        return new FixPoint(PI - Atan2Table[y1 / (x1 >> 16)]);
                    else
                        return new FixPoint(PI - Atan2Table[(y1 << 16) / x1]);
#else
                    return new FixPoint(PI - Atan2Table[(y1 << FRACTIONAL_BITS) / x1]);
#endif
            }
            else if (y1 < 0)
            {
                y1 = -y1;
                if (y1 < x1)
#if FIXPOINT_32BITS_FRACTIONAL
                    if (y1 > ATAN2_HELPER_NUMBER)
                        return new FixPoint(PI + Atan2Table[y1 / (x1 >> 16)]);
                    else
                        return new FixPoint(PI + Atan2Table[(y1 << 16) / x1]);
#else
                    return new FixPoint(PI + Atan2Table[(y1 << FRACTIONAL_BITS) / x1]);
#endif
                else
#if FIXPOINT_32BITS_FRACTIONAL
                    if (x1 > ATAN2_HELPER_NUMBER)
                        return new FixPoint(ONE_AND_HALF_PI - Atan2Table[x1 / (y1 >> 16)]);
                    else
                        return new FixPoint(ONE_AND_HALF_PI - Atan2Table[(x1 << 16) / y1]);
#else
                    return new FixPoint(ONE_AND_HALF_PI - Atan2Table[(x1 << FRACTIONAL_BITS) / y1]);
#endif
            }
            else
            {
                return Pi;
            }
        }
        else
        {
            if (y1 > 0)
            {
                return HalfPi;
            }
            else if (y1 < 0)
            {
                return OneAndHalfPi;
            }
            else
            {
                return Zero;
            }
        }
    }

    public static FixPoint Degree2Radian(FixPoint degree)
    {
        return degree * RadianPerDegree;
    }

    public static FixPoint Radian2Degree(FixPoint radian)
    {
        return radian * DegreePerRadian;
    }

    #region 方便函数
    public static FixPoint XZToUnityRotationRadian(FixPoint x, FixPoint z)
    {
        return FixPoint.Atan2(-z, x);
    }

    public static FixPoint XZToUnityRotationDegree(FixPoint x, FixPoint z)
    {
        return FixPoint.Radian2Degree(FixPoint.Atan2(-z, x));
    }

    public static FixPoint Min(FixPoint term1, FixPoint term2)
    {
        if (term2 < term1)
            return term2;
        return term1;
    }

    public static FixPoint Max(FixPoint term1, FixPoint term2)
    {
        if (term2 > term1)
            return term2;
        return term1;
    }

    public static FixPoint Clamp(FixPoint term, FixPoint min_value, FixPoint max_value)
    {
        if (term < min_value)
            return min_value;
        if (term > max_value)
            return max_value;
        return term;
    }
    #endregion

    #region 内部
    const int NUM_BITS = 64;
#if FIXPOINT_32BITS_FRACTIONAL
    const int FRACTIONAL_BITS = 32;
    const ulong INTEGER_PART_MASK = 0xFFFFFFFF00000000;
    const long FRACTIANAL_PART_MASK = 0x00000000FFFFFFFF;
    const long OVERFLOW_MASK = 0x7FFFFFFF00000000;
    //3.14159265358979323846264338327950288419716939937510  4294967296
    const long QUARTER_PI = 3373259426L;
    const long HALF_PI = 6746518852L;
    const long PI = 13493037704L;
    const long ONE_AND_HALF_PI = 20239556556L;
    const long TWO_PI = 26986075408L;
    const long INV_PI = 1367130551L;
    const long RADIAN_PER_DEGREE = 74961321L;
    const long DEGREE_PER_RADIAN = 246083499208L;
    const long ATAN2_HELPER_NUMBER = 0x00007FFFFFFFFFFF;
#else
    const int FRACTIONAL_BITS = 16;
    const ulong INTEGER_PART_MASK = 0xFFFFFFFFFFFF0000;
    const long FRACTIANAL_PART_MASK = 0x000000000000FFFF;
    const long OVERFLOW_MASK = 0x7FFFFFFFFFFF0000;
    //3.14159265358979323846264338327950288419716939937510
    const long QUARTER_PI = 51471L;
    const long HALF_PI = 102942L;
    const long PI = 205884L;
    const long ONE_AND_HALF_PI = 308826L;
    const long TWO_PI = 411768L;
    const long INV_PI = 20860L;
    const long RADIAN_PER_DEGREE = 1144L;
    const long DEGREE_PER_RADIAN = 3754936L;
#endif
    const int INTEGER_BITS = NUM_BITS - FRACTIONAL_BITS;
    const long ONE = 1L << FRACTIONAL_BITS;
    const long MAX_VALUE = long.MaxValue;
    const long MIN_VALUE = long.MinValue;
    const int SIN_TABLE_SIZE = 102942;
    const int ATAN2_TABLE_SIZE = (1 << 16) + 1;

    static long AddWithCheckingOverflow(long x, long y, ref bool overflow)
    {
        long z = x + y;
        if (((~(x ^ y) & (x ^ z)) & MIN_VALUE) != 0)
            overflow = true;
        return z;
    }

    static int CountLeadingZeroes(ulong x)
    {
        int count = 0;
        while ((x & 0xF000000000000000) == 0)
        {
            count += 4;
            x <<= 4;
        }
        while ((x & 0x8000000000000000) == 0)
        {
            count += 1;
            x <<= 1;
        }
        return count;
    }

    static readonly char WhiteSpace = (char)1;
    static readonly char Error_____ = (char)2;
    static readonly char Digit_____ = (char)3;
    static readonly char Sign______ = (char)4;
    static readonly char Point_____ = (char)5;

    static readonly char[] CodeMap = new[]{
        /*     0          1          2          3          4          5          6          7          8          9               */
        /*  0*/WhiteSpace,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,WhiteSpace,/*  0*/
        /* 10*/WhiteSpace,Error_____,Error_____,WhiteSpace,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 10*/
        /* 20*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 20*/
        /* 30*/Error_____,Error_____,WhiteSpace,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 30*/
        /* 40*/Error_____,Error_____,Error_____,Sign______,WhiteSpace,Sign______,Point_____,Error_____,Digit_____,Digit_____,/* 40*/
        /* 50*/Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Error_____,Error_____,/* 50*/
        /* 60*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 60*/
        /* 70*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 70*/
        /* 80*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 80*/
        /* 90*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 90*/
        /*100*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*100*/
        /*110*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*110*/
        /*120*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____/*127*/
    };

    static char GetCode(char ch)
    {
        if (ch > 127)
            return Error_____;
        else
            return CodeMap[ch];
    }

    private FixPoint(long raw_value)
    {
        m_raw_value = raw_value;
    }
    #endregion
}