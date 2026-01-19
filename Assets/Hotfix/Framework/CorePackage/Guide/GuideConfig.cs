using System.Collections.Generic;

namespace LccHotfix
{
    /// <summary>
    /// 引导列表
    /// </summary>
    public class GuideConfigList
    {
        public List<GuideConfig> configList;
    }

    /// <summary>
    /// 一个完整引导要做的事
    /// </summary>
    public class GuideConfig
    {
        //逻辑id
        public int id;

        //0=强制引导 1=非强制引导
        public int type;

        //包含的状态机
        public List<GuideStateNode> stateList;

        public string finishCond;
        public List<string> finishArgs;
        public int priority;
    }

    /// <summary>
    /// 触发顺序列表
    /// </summary>
    public class GuideSeqConfig
    {
        public List<GuideTriggerConfig> triggerList;
    }

    /// <summary>
    /// 一个完整引导的触发
    /// </summary>
    public class GuideTriggerConfig
    {
        public int guideId;
        public string guideTriggerName;
        public List<string> args;
    }

    /// <summary>
    /// 状态机
    /// </summary>
    public class GuideStateNode
    {
        //状态机名称
        public string stateName;

        //超时时间
        public int timeout;

        //最大点击次数
        public int maxInvalidClickTimes;

        //状态参数
        public List<string> args;

        public string overCond;
        public List<string> overArgs;
    }
}