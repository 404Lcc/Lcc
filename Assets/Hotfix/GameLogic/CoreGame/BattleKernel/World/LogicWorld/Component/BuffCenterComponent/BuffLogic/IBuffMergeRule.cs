
namespace LccHotfix
{
    public interface IBuffMergeRule
    {
        /// <summary>
        /// 合并规则，返回true表示保留当前buff
        /// </summary>
        /// <param name="buffCenter">所在的buffCenter</param>
        /// <param name="genInfo">新buff的生成信息</param>
        /// <returns></returns>
        public bool Merge(BuffCenterComponent buffCenter, BuffGenInfo genInfo);
    }
}
