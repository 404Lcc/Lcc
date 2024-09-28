using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Main)]
    public class MainSceneState : SceneState
    {
        public override void OnEnter(object[] args)
        {
            base.OnEnter(args);
            Debug.Log("Main" + "进入");


        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Main" + "退出");
        }
    }
}