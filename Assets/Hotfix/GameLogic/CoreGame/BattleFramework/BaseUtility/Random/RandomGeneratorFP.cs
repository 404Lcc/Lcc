using System.Collections;
using System.Collections.Generic;
namespace CoreGame
{
    public class RandomGeneratorFP : IDestruct
    {
#if FIXPOINT_32BITS_FRACTIONAL
        public const ulong MASK = 0x7FFFFFFFFFFFFFFF;
#else
        public const ulong MASK = 0x00007FFFFFFFFFFF;
#endif
        MersenneTwister64 m_mt64;

        public RandomGeneratorFP(int seed = 0)
        {
            m_mt64 = new MersenneTwister64((ulong)seed);
        }

        public void Destruct()
        {
        }

        public void ResetSeed(int seed)
        {
            m_mt64.Reset((ulong)seed);
        }

        public FixPoint Rand()
        {
            long rand = (long)(m_mt64.ExtractNumber() & MASK);
            return FixPoint.CreateFromRaw(rand);
        }

        public FixPoint RandBetween(FixPoint min_value, FixPoint max_value)
        {
            if (min_value > max_value)
            {
                FixPoint temp = max_value;
                max_value = min_value;
                min_value = temp;
            }
            return min_value + Rand() % (max_value - min_value + FixPoint.PrecisionFP);
        }
    }
}