using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneStateType.Game)]
    public class GameSceneState : SceneState
    {
        public override void OnEnter(object[] args)
        {
            base.OnEnter(args);
            Debug.Log("Game" + "进入");





         

        }
        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("Game" + "退出");
        }
    }
}