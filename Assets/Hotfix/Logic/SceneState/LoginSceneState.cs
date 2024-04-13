using ET;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Login)]
    public class LoginSceneState : SceneState
    {
        public override void OnEnter(object[] args)
        {
            base.OnEnter(args);


            UIEventManager.Instance.Publish(UIEventType.Login);

            Debug.Log("Login" + "½øÈë");
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Login" + "ÍË³ö");
        }
    }
}