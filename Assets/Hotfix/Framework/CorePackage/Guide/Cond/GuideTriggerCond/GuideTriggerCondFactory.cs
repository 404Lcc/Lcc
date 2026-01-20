namespace LccHotfix
{
    public static class GuideTriggerCondFactory
    {
        public static GuideTriggerCondBase CreateCond(GuideTriggerConfig config)
        {
            var condName = config.guideTriggerName;
            var type = Main.CodeTypesService.GetType(condName);
            if (type == null)
            {
                UnityEngine.Debug.LogError("[新手引导] 触发条件未找到" + condName + "触发类");
                return null;
            }

            GuideTriggerCondBase cond = System.Activator.CreateInstance(type, new object[] { config }) as GuideTriggerCondBase;
            return cond;
        }
    }
}