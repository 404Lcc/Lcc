using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneType.Login)]
    public class LoginScene : LoadSceneHandler
    {
        public override void SceneStartHandler()
        {
            base.SceneStartHandler();
            //进入

            Log.Debug("进入login场景");
            WindowManager.Instance.OpenWindow<UILoginPanel>(UIWindowDefine.UILoginPanel);
        }

        public override void SceneExitHandler()
        {
            base.SceneExitHandler();

            Log.Debug("退出login场景");
        }
    }
}