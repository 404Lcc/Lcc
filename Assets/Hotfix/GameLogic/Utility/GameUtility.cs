using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using cfg;
using UnityEngine;
using Random = System.Random;

namespace LccHotfix
{
    public static class GameUtility
    {
        public const long CSHARP_1970_TIME = 621355968000000000; //C#中1970年的时间，用于处理java时间戳

        public static void Dispatch<T>(T value) where T : struct, IValueEvent
        {
            Main.ValueEventService.Dispatch(value);
        }

        public static void AddHandle<T>(Action<T> handle) where T : struct, IValueEvent
        {
            Main.ValueEventService.AddHandle(handle);
        }

        public static void RemoveHandle<T>(Action<T> handle) where T : struct, IValueEvent
        {
            Main.ValueEventService.RemoveHandle(handle);
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

        public static string GetLanguageText(int id, params object[] args)
        {
            return Main.LanguageService.GetValue(id, args);
        }

        public static string GetLanguageText(uint id, params object[] args)
        {
            return Main.LanguageService.GetValue(id, args);
        }

        public static string GetLanguageKey(int id)
        {
            return Main.LanguageService.GetKey(id);
        }

        public static string GetLanguageKey(uint id)
        {
            return Main.LanguageService.GetKey(id);
        }

        public static T GetModel<T>() where T : ModelTemplate
        {
            return Main.ModelService.GetModel<T>();
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

        public static int GetWeightIndex(List<int> weight)
        {
            int weightAll = 0;
            int addWeight = 0;
            int random = 0;
            foreach (int _weight in weight)
            {
                if (_weight < 0)
                {
                    Debug.LogError("随机出错");
                    return 0;
                }

                weightAll += _weight;
            }

            random = UnityEngine.Random.Range(1, weightAll + 1);
            for (int i = 0; i < weight.Count; i++)
            {
                addWeight += weight[i];
                if (random <= addWeight)
                {
                    return i;
                }
            }

            return 0;
        }

        public static GameObjectPoolObject GetObj(string poolKey)
        {
            var obj = Main.GameObjectPoolService.GetObject(poolKey);
            return obj;
        }

        public static void PutObj(ref GameObjectPoolObject poolObject)
        {
            if (poolObject == null)
                return;

            Main.GameObjectPoolService.ReleaseObject(poolObject);
            poolObject = null;
        }

        #region 动画工具

        /// <summary>
        /// 得到动画长度 匹配部分字符串 比如Hero_Hit 查找Hit返回第一个匹配的动画
        /// </summary>
        public static float GetAnimationClipLengthByNameMatched(Animator animator, string matchString)
        {
            if (animator == null)
            {
                return 0;
            }

            if (animator.runtimeAnimatorController == null)
            {
                return 0;
            }

            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            if (clips == null)
            {
                return 0;
            }

            foreach (AnimationClip clip in clips)
            {
                if (clip.name.Contains(matchString))
                {
                    return clip.length;
                }
            }

            Debug.LogError($"找不到包含字符串 {matchString} 的动画片段，animator挂载的物体：{animator.name}");

            return 0;
        }

        #endregion

        /// <summary>
        /// 获取本地IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "127.0.0.1";
        }
    }
}