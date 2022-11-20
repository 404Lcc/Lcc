using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateName.Login)]
    public class LoginSceneState : SceneState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("Login" + "进入");
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Login" + "退出");
        }
    }
    [SceneState(SceneStateName.Main)]
    public class MainSceneState : SceneState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("Main" + "进入");
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Main" + "退出");
        }
    }

    [StatePipeline(SceneStateName.Login, SceneStateName.Main)]
    public class EnterMain : StatePipeline
    {
        public EnterMain(string sceneName, string target) : base(sceneName, target)
        {
        }

        public override bool CheckState()
        {
            return false;
        }
    }
    [StatePipeline(SceneStateName.Main, SceneStateName.Login)]
    public class EnterLogin : StatePipeline
    {
        public EnterLogin(string sceneName, string target) : base(sceneName, target)
        {
        }

        public override bool CheckState()
        {
            return false;
        }
    }
}