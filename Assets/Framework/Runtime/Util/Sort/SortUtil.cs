using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public static class SortUtil
    {
        public static void BubbleSortUtil(List<int> list)
        {
            int num = list.Count - 1;
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < num - i; j++)
                {
                    if (list[j] > list[j + 1])
                    {
                        int temp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = temp;
                    }
                }
            }
        }
    }
}