using ET;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Login)]
    public class LoginSceneState : SceneState
    {
        public override bool SceneLoadHandler()
        {
            return base.SceneLoadHandler();
        }
        public override void OnEnter(object[] args)
        {
            base.OnEnter(args);




            Debug.Log("Login" + "½øÈë");

            SceneStateManager.Instance.OpenChangeScenePanel();

            SceneLoadEndHandler();
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Login" + "ÍË³ö");
        }
    }
}