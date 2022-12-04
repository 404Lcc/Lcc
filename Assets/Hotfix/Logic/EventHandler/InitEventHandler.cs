using ET;
using LccModel;
using System;

namespace LccHotfix
{
    [EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override async ETTask Publish(Start data)
        {

            //这里可以初始化


            await UpdatePanel.Instance.UpdateLoadingPercent(51, 100);

            //进入第一个状态
            SceneStateManager.Instance.SetDefaultState(SceneStateName.Login);
        }
    }
}