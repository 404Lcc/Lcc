namespace LccHotfix
{
    //////////////////////////////////////////////////////////////////////////
    // 自定义条件
    //////////////////////////////////////////////////////////////////////////

    /* CustomLogic对于Condition这个概念存在与否不做强求， 下面是对 Condition 节点的定义：
         1 有判断是否达成的接口
         2 无副作用，Cnd运行之后，一般来说不对数据造成改变
    */


    //自定义条件逻辑运行接口
    public interface ICondition
    {
        /// <summary>
        /// 条件是否达成
        /// </summary>
        /// <returns></returns>
        bool IsConditionReached();
    }
}