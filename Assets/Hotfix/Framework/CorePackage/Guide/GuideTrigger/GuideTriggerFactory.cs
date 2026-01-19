using UnityEngine;

namespace LccHotfix
{
    public class GuideTriggerFactory
    {
        public GuideTriggerBase CreateTriggerByType(GuideTriggerConfig cfg)
        {
            GuideTriggerBase baseTrigger = null;
            var triggerName = cfg.guideTriggerName;

            if (baseTrigger == null)
            {
                UnityEngine.Debug.LogError("[新手引导] GuideTriggerFactory 未找到" + triggerName + "触发类");
                return null;
            }

            return baseTrigger;
        }
    }
}