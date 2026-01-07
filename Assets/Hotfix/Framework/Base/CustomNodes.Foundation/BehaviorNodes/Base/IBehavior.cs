using System.Collections;
using System.Collections.Generic;

namespace LccHotfix
{
    //////////////////////////////////////////////////////////////////////////
    // 自定义行为逻辑运行接口
    //////////////////////////////////////////////////////////////////////////

    /* 什么是 Behavior（Bhv）, 这其实是一个相当模糊的概念... CustomLogic对于这个概念的存在与否并不强求
       下面当前对 Behavior 的定义归纳： 
         1、可驱动，暂定都可以通过Update进行驱动（哪怕瞬发的行为也是如此）
         2、可重复，可以通过Reset重新开始
         3、有实效，Bhv运行之后，一般来说它对游戏数据、状态做了什么改变，而是不是相当于什么都没发生（不似 Condition）
    */
    // public interface IBehavior : INeedUpdate, ICanReset
    // {
    //
    // }
}
