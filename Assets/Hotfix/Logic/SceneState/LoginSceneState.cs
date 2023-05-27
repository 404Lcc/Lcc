using ET;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Login)]
    public class LoginSceneState : SceneState
    {
        public override async ETTask OnEnter()
        {
            await base.OnEnter();


            UIEventManager.Instance.Publish(UIEventType.Login);

            Debug.Log("Login" + "½øÈë");
        }
        public override async ETTask OnExit()
        {
            await base.OnExit();
            Debug.Log("Login" + "ÍË³ö");
        }
    }
}