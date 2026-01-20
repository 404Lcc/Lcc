using System.Collections.Generic;

namespace LccHotfix
{
    public static class GuideFinishCondFactory
    {
        public static GuideFinishCondBase CreateCond(Guide guide, string condName, List<string> args)
        {
            var type = Main.CodeTypesService.GetType(condName);
            if (type == null)
            {
                UnityEngine.Debug.LogError("[新手引导] 完成条件未找到" + condName + "条件类");
                return null;
            }

            GuideFinishCondBase cond = System.Activator.CreateInstance(type, new object[] { guide, args }) as GuideFinishCondBase;
            return cond;
        }
    }
}