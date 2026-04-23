using System.Collections;
using System.Collections.Generic;
namespace CoreGame
{
    public class MersenneTwister64
    {
        public const int W = 64;
        public const ulong N = 312;
        public const ulong M = 156;
        public const ulong R = 31;
        public const ulong A = 0xB5026F5AA96619E9;
        public const int U = 29;
        public const ulong D = 0x5555555555555555;
        public const int S = 17;
        public const ulong B = 0x71D67FFFEDA60000;
        public const int T = 37;
        public const ulong C = 0xFFF7EEE000000000;
        public const int L = 43;
        public const ulong F = 6364136223846793005;

        public const ulong LOWER_MASK = 0x7FFFFFFF;
        public const ulong UPPER_MASK = ~LOWER_MASK;

        ulong[] MT = new ulong[N];
        ulong m_index = N;

        public MersenneTwister64(ulong seed)
        {
            Initialize(seed);
        }

        public void Reset(ulong seed)
        {
            Initialize(seed);
        }

        public ulong ExtractNumber()
        {
            if (m_index >= N)
                Twist();
            ulong y = MT[m_index];
            y = y ^ ((y >> U) & D);
            y = y ^ ((y << S) & B);
            y = y ^ ((y << T) & C);
            y = y ^ (y >> L);
            ++m_index;
            return y;
        }

        void Initialize(ulong seed)
        {
            m_index = N;
            MT[0] = seed;
            for (ulong i = 1; i < N; ++i)
            {
                MT[i] = (F * (MT[i - 1] ^ (MT[i - 1] >> (W - 2))) + i);
            }
            Twist();
        }

        void Twist()
        {
            for (ulong i = 0; i < N; ++i)
            {
                ulong x = (MT[i] & UPPER_MASK) + (MT[(i + 1) % N] & LOWER_MASK);
                ulong xa = x >> 1;
                if ((x & 0x1) != 0)
                    xa = xa ^ A;
                MT[i] = MT[(i + M) % N] ^ xa;
            }
            m_index = 0;
        }
    }

    public class MersenneTwister32
    {
        public const int W = 32;
        public const uint N = 624;
        public const uint M = 397;
        public const int R = 31;
        public const uint A = 0x9908B0DF;
        public const int U = 11;
        public const int S = 7;
        public const uint B = 0x9D2C5680;
        public const int T = 15;
        public const uint C = 0xEFC60000;
        public const int L = 18;
        public const uint F = 1812433253;
        public const uint LOWER_MASK = 0x7FFFFFFF;
        public const uint UPPER_MASK = ~LOWER_MASK;

        uint[] MT = new uint[N];
        uint m_index = N;

        public MersenneTwister32(uint seed)
        {
            Initialize(seed);
        }

        public void Reset(uint seed)
        {
            Initialize(seed);
        }

        public uint ExtractNumber()
        {
            if (m_index >= N)
                Twist();
            uint y = MT[m_index] >> U;
            y ^= (y << S) & B;
            y ^= (y << T) & C;
            y ^= (y >> L);
            ++m_index;
            return y;
        }

        void Initialize(uint seed)
        {
            m_index = N;
            MT[0] = seed;
            for (uint i = 1; i < N; ++i)
            {
                MT[i] = (F * (MT[i - 1] ^ (MT[i - 1] >> (W - 2))) + i);
            }
            Twist();
        }

        void Twist()
        {
            for (ulong i = 0; i < N; ++i)
            {
                uint x = (MT[i] & UPPER_MASK) + (MT[(i + 1) % N] & LOWER_MASK);
                uint xa = x >> 1;
                if ((x & 0x1) != 0)
                {
                    xa = xa ^ A;
                }
                MT[i] = MT[(i + M) % N] ^ xa;
            }
            m_index = 0;
        }
    }
}