using LccModel;
using System;
using LccModelInit = LccModel.Init;

namespace LccHotfix
{
    public class Init
    {
        public static void InitHotfix()
        {
            try
            {
                LccModelInit.OnEventSystemFixedUpdate += ObjectBaseEventSystem.Instance.EventSystemFixedUpdate;
                LccModelInit.OnEventSystemUpdate += ObjectBaseEventSystem.Instance.EventSystemUpdate;
                LccModelInit.OnEventSystemLateUpdate += ObjectBaseEventSystem.Instance.EventSystemLateUpdate;

                Manager.Instance.InitManager();
                EventManager.Instance.InitManager();
                UIEventManager.Instance.InitManager();
                ConfigManager.Instance.InitManager();

                EventManager.Instance.Publish(new Start()).Coroutine();
            }
            catch (Exception e)
            {
                LogUtil.LogError(e.ToString());
            }
        }
    }
}