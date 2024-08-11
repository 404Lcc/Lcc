using System;

namespace LccModel
{
    public static class RandomHelper
    {
        public static Random random = new Random();

        public static int RandomNumber(int lower, int upper)
        {
            int value = random.Next(lower, upper);
            return value;
        }

        public static int RandomRate()
        {
            int value = random.Next(1, 101);
            return value;
        }
    }
}