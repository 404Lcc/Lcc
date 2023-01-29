using System;

namespace LccModel
{
    public static class RandomUtil
    {
        private static Random _random = new Random();


        public static int RandomNumber(int lower, int upper)
        {
            int value = _random.Next(lower, upper);
            return value;
        }

        public static int RandomRate()
        {
            int value = _random.Next(1, 101);
            return value;
        }
    }
}