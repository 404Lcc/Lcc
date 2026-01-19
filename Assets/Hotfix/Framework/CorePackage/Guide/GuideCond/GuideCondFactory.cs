using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class GuideCondFactory
    {
        public GuideCondBase CreateCond(Guide guide, string condName, List<string> args)
        {
            GuideCondBase condBase = null;

            if (condBase == null)
            {
                UnityEngine.Debug.LogError("[新手引导] GuideCondFactory 未找到" + condName + "条件类");
                return null;
            }

            return condBase;
        }
    }
}