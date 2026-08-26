namespace LccHotfix
{
    /// <summary>
    /// Buff合并规则定义
    /// </summary>
    public static class BuffMergeRule
    {
        public static IBuffMergeRule NoMerge = new NoMergeRule(); // 不处理合并，直接创建
        public static IBuffMergeRule Replace = new ReplaceMergeRule(); // 保留新的
        public static IBuffMergeRule Preserve = new PreserveMergeRule(); // 保留旧的
        public static IBuffMergeRule PreserveMax = new PreserveMaxMergeRule(); // 保留Level更高的
        public static IBuffMergeRule SumLevel = new SumLevelMergeRule(); // 层数相加，最大为MaxLevel，不刷新持续时间
        public static IBuffMergeRule SumLevelAndResetDuration = new SumLevelMergeRule() { ResetDurationOnMerge = true }; // 层数相加，最大为MaxLevel，刷新持续时间
        public static IBuffMergeRule SumLevelAndResetDurationOnLevelUp = new SumLevelMergeRule() { ResetDurationOnMerge = true, ResetDurationOnlyWhenLevelIncreases = true }; // 层数相加；仅层数实际上涨时刷新持续时间（满层再叠不续时）
        public static IBuffMergeRule Stack = new StackMergeRule(); // 分别叠层计时，但Level总和受MaxLevel限制

        /// <summary>
        /// 不合并，直接添加新的Buff
        /// 其实配置的时候留空就行了
        /// </summary>
        private class NoMergeRule : IBuffMergeRule
        {
            public bool Merge(BuffCenterComponent buffCenter, BuffGenInfo genInfo)
            {
                return true;
            }
        }

        /// <summary>
        /// 替换：移除旧的Buff，添加新的Buff，同时只留一个
        /// </summary>
        private class ReplaceMergeRule : IBuffMergeRule
        {
            public bool Merge(BuffCenterComponent buffCenter, BuffGenInfo genInfo)
            {
                buffCenter.RemoveBuffByID(genInfo.LogicConfigID);
                return true;
            }
        }

        /// <summary>
        /// 保留原有buff，丢弃新buff
        /// </summary>
        private class PreserveMergeRule : IBuffMergeRule
        {
            public bool Merge(BuffCenterComponent buffCenter, BuffGenInfo genInfo)
            {
                return !buffCenter.HasBuff(genInfo.LogicConfigID);
            }
        }

        /// <summary>
        /// 保留Level最大的buff：如果新buff层数更高，则替换旧buff，否则丢弃新buff
        /// </summary>
        private class PreserveMaxMergeRule : IBuffMergeRule
        {
            public bool Merge(BuffCenterComponent buffCenter, BuffGenInfo genInfo)
            {
                var oldBuff = buffCenter.GetBuff(genInfo.LogicConfigID);
                if (oldBuff == null)
                {
                    return true;
                }
                if (oldBuff.BuffGenInfo.BuffLevel > genInfo.BuffLevel)
                {
                    return false;
                }
                buffCenter.RemoveBuffByID(genInfo.LogicConfigID);
                return true;
            }
        }

        /// <summary>
        /// 层数相加：如果新buff和旧buff的层数都大于0，则合并后的buff层数为两者之和，最大为MaxLevel
        /// </summary>
        private class SumLevelMergeRule : IBuffMergeRule
        {
            public bool ResetDurationOnMerge; // 是否在合并时刷新持续时间
            public bool ResetDurationOnlyWhenLevelIncreases; // 为 true 时，仅层数实际上涨才刷新持续时间
            public bool Merge(BuffCenterComponent buffCenter, BuffGenInfo genInfo)
            {
                var oldBuff = buffCenter.GetBuff(genInfo.LogicConfigID);
                if (oldBuff == null)
                {
                    return true;
                }

                var oldLevel = oldBuff.Level;
                oldBuff.MaxLevel = genInfo.BuffMaxLevel;
                oldBuff.Level = System.Math.Min(oldBuff.Level + genInfo.BuffLevel, genInfo.BuffMaxLevel);
                oldBuff.BuffGenInfo.BuffLevel = oldBuff.Level;
                oldBuff.BuffGenInfo.BuffMaxLevel = oldBuff.MaxLevel;
                var levelIncreased = oldBuff.Level > oldLevel;
                if (ResetDurationOnMerge && (!ResetDurationOnlyWhenLevelIncreases || levelIncreased))
                {
                    oldBuff.ResetTime();
                }
                oldBuff.Upgrade();
                oldBuff.OnBuffStateChanged(buffCenter.Owner);
                return false;
            }
        }

        /// <summary>
        /// 多层buff叠加规则
        /// 允许buff叠加，每次叠加都会添加一个新的buff实例
        /// 受到MaxLevel限制，如果超过了MaxLevel就尝试丢掉最早的buff实例
        /// </summary>
        private class StackMergeRule : IBuffMergeRule
        {
            public bool Merge(BuffCenterComponent buffCenter, BuffGenInfo genInfo)
            {
                var sumLevel = genInfo.BuffLevel;
                for (int i = buffCenter.BuffList.Count - 1; i >= 0; --i)
                {
                    var buff = buffCenter.BuffList[i] as BuffLogic;
                    if (buff.BuffGenInfo.LogicConfigID != genInfo.LogicConfigID)
                    {
                        continue;   
                    }
                    sumLevel += buff.BuffGenInfo.BuffLevel;
                    if (sumLevel > genInfo.BuffMaxLevel)
                    {
                        buffCenter.RemoveBuffByIndex(i);
                    }
                }
                return true;
            }
        }
    }
}
