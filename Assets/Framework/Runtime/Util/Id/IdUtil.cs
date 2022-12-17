using System.Collections.Generic;

namespace LccModel
{
    public static class IdUtil
    {
        public static List<int> countList = new List<int>();
        public static int id;
        public static long GenerateId()
        {
            if (countList.Count == 0)
            {
                id++;
                return id;
            }
            for (int i = 1; i <= countList.Count; i++)
            {
                if (!countList.Contains(i))
                {
                    return i;
                }
            }
            id++;
            return id;
        }
        public static long GenerateInstanceId()
        {
            if (countList.Count == 0)
            {
                id++;
                return id;
            }
            for (int i = 1; i <= countList.Count; i++)
            {
                if (!countList.Contains(i))
                {
                    return i;
                }
            }
            id++;
            return id;
        }
    }
}