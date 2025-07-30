using System;
using System.Collections.Generic;
using cfg;
using UnityEngine;
using Random = System.Random;

namespace LccHotfix
{
    public static class GameUtility
    {
        public static void FireNow(object sender, GameEventArgs e)
        {
            Main.EventService.FireNow(sender, e);
        }

        public static void Subscribe(GameEventType type, EventHandler<GameEventArgs> handler)
        {
            Main.EventService.Subscribe((int)type, handler);
        }

        public static void Unsubscribe(GameEventType type, EventHandler<GameEventArgs> handler)
        {
            Main.EventService.Unsubscribe((int)type, handler);
        }

        public static string GetLanguageText(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
            {
                Log.Error("key不能等于空");
                return "";
            }

            return Main.LanguageService.GetValue(key, args);
        }

        /// <summary>
        /// 打乱一个list
        /// </summary>
        public static void ShuffleList<T>(IList<T> list)
        {
            Random rand = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int i = rand.Next(n + 1);
                T value = list[i];
                list[i] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// 四舍五入
        /// </summary>
        /// digits:保留几位小数
        public static float Round(float value, int digits = 1)
        {
            if (value == 0)
            {
                return 0;
            }

            float multiple = Mathf.Pow(10, digits);
            float tempValue = value > 0 ? value * multiple + 0.5f : value * multiple - 0.5f;
            tempValue = Mathf.FloorToInt(tempValue);
            return tempValue / multiple;
        }

        public static string FormatCurrency(float num)
        {
            string[] units = new string[] { "", "K", "M", "B", "T" };
            string str = "";

            // 4位数之后才管理
            if (num < 1000)
            {
                str = num.ToString();
                return str;
            }

            float tempNum = num;
            long step = 1000;
            int unitIndex = 0;

            while (tempNum >= step)
            {
                unitIndex++;
                tempNum /= step;
            }

            if (unitIndex >= units.Length)
            {
                Debug.LogError("数字太大了，不知道后面要用什么了");
                str = num.ToString();
            }
            else
            {
                //整数部分的位数
                var digit = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log10(tempNum) + 1), 0, 3);
                tempNum = Round(tempNum, 3 - digit);
                str = $"{tempNum}{units[unitIndex]}";
            }

            return str;
        }
    }
}