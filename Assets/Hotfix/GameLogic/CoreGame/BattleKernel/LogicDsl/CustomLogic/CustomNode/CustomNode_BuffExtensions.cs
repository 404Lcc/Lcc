using PBConfig;

namespace LccHotfix
{
    public static class CustomNodeBuffExtensions
    {
    #region Buff生成


        /// <summary>
        /// 给目标创建一个 BuffGenInfo，来源继承当前节点的单位来源。
        /// </summary>
        public static BuffGenInfo CreateBuffGenInfoFromUnit(this CustomNode self, LogicEntity target, int buffLogicID, int maxLvl = 1)
        {
            var genInfo = target.CreateBuffGenInfo(buffLogicID, maxLvl);
            if (genInfo == null)
            {
                return null;
            }

            var sumUnitSource = self.GetSumUnitSource(false);
            genInfo.SumUnitSource = sumUnitSource.FighterEnityId != 0
                ? sumUnitSource
                : self.CreateTempSumUnitSource();
            genInfo.InheritElementTypeFrom(self);
            if (self.GetSubobjectSource(out var subobjSource))
                genInfo.SubobjSource = subobjSource;
            return genInfo;
        }

        /// <summary>
        /// 把来源逻辑的元素类型写到 Buff，供 DOT / 跳伤走 MakeSkillFactorDmgEvt 时消费。
        /// </summary>
        public static void InheritElementTypeFrom(this BuffGenInfo genInfo, CustomNode source)
        {
            if (genInfo == null)
                return;
            genInfo.DamageType = source.ResolveElementType();
        }

        public static void InheritElementTypeFrom(this BuffGenInfo genInfo, TElementType elementType)
        {
            if (genInfo == null)
                return;
            genInfo.DamageType = elementType;
        }

    #endregion
    }
}
