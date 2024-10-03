using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneType.Login)]
    public class LoginSceneState : SceneState
    {
        public override bool SceneLoadHandler()
        {
            return base.SceneLoadHandler();
        }
        public override void OnEnter(object[] args)
        {
            base.OnEnter(args);




            Debug.Log("Login" + "进入");

            SceneManager.Instance.OpenChangeScenePanel();

            SceneLoadEndHandler();
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Login" + "退出");
        }
    }
}