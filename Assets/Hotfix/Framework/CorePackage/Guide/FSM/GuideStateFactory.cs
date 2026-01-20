namespace LccHotfix
{
    public static class GuideStateFactory
    {
        public static bool CreateState(GuideFSM fsm, string stateName)
        {
            var type = Main.CodeTypesService.GetType(stateName);
            if (type == null)
            {
                UnityEngine.Debug.LogError("[新手引导] 引导状态未找到" + stateName + "引导状态类");
                return false;
            }

            var obj = System.Activator.CreateInstance(type);
            if (obj is not IGuideStateNode node)
            {
                return false;
            }

            fsm.AddNode(node);
            fsm.SetDefaultState(stateName);
            return true;
        }
    }
}