using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateName.Login)]
    public class LoginSceneState : SceneState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("Login" + "����");
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Login" + "�˳�");
        }
    }
    [SceneState(SceneStateName.Main)]
    public class MainSceneState : SceneState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("Main" + "����");
        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Main" + "�˳�");
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