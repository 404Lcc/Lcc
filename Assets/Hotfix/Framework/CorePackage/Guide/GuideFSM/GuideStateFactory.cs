using UnityEngine;

namespace LccHotfix
{
    public class GuideStateFactory
    {
        public string CreateState(GuideFSM fsm, string stateName)
        {
            GuideState state = null;

            switch (stateName)
            {

            }

            if (stateName == "")
            {
                UnityEngine.Debug.LogError("[新手引导] GuideStateFactory 未找到" + stateName + "引导状态类");
                return "";
            }

            return stateName;
        }
    }
}