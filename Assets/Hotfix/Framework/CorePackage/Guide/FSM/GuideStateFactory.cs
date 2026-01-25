namespace LccHotfix
{
    public static class GuideStateFactory
    {
        public static IGuideStateNode CreateState(string stateName)
        {
            var type = Main.CodeTypesService.GetType(stateName);
            if (type == null)
            {
                UnityEngine.Debug.LogError("[新手引导] 引导状态未找到" + stateName + "引导状态类");
                return null;
            }

            var obj = System.Activator.CreateInstance(type);
            if (obj is not IGuideStateNode node)
            {
                return null;
            }

            return node;
        }
    }
}