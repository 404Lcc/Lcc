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
        //引导步骤
        public List<GuideStepConfig> stepList;
        //完成条件
        public string finishCond;
        //完成条件参数
        public List<string> finishArgs;
        //优先级
        public int priority;
    }
    
    /// <summary>
    /// 引导步骤
    /// </summary>
    public class GuideStepConfig
    {
        //状态机名称
        public string stateName;
        //状态参数
        public List<string> stateArgs;
        
        //超时时间
        public int timeout;
        //最大点击次数
        public int maxInvalidClickTimes;
        
        //完成条件
        public string finishCond;
        //完成条件参数
        public List<string> finishArgs;
    }

    /// <summary>
    /// 引导触发列表
    /// </summary>
    public class GuideTriggerConfigList
    {
        public List<GuideTriggerConfig> triggerList;
    }

    /// <summary>
    /// 一个完整引导的触发条件
    /// </summary>
    public class GuideTriggerConfig
    {
        public int guideId;
        public string guideTriggerName;
        public List<string> args;
    }
}