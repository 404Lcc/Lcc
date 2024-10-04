using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneType.Main)]
    public class MainScene : LoadSceneHandler
    {
        public override void SceneStartHandler()
        {
            base.SceneStartHandler();
            //进入

            Log.Debug("进入main场景");
            WindowManager.Instance.OpenWindow<UIMainPanel>(UIWindowDefine.UIMainPanel);
        }

        public override void SceneExitHandler()
        {
            base.SceneExitHandler();

            Log.Debug("退出main场景");
        }
    }
}