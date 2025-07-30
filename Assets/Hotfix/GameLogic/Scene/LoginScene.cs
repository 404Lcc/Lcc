using LccModel;
using System.Collections;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneType.Login)]
    public class LoginScene : LoadSceneHandler, ICoroutine
    {
        public LoginScene()
        {
            sceneType = SceneType.Login;
            loadType = LoadingType.Fast;
        }

        public override void SceneStartHandler()
        {
            base.SceneStartHandler();
            //进入

            Log.Debug("进入login场景");

            //设备id
            Log.Debug("设备id = " + UnityEngine.SystemInfo.deviceUniqueIdentifier);

            //进入游戏
            UILoadingPanel.Instance.SetStartLoadingBg();

            WindowManager.Instance.OpenWindow<UILoginPanel>(UIWindowDefine.UILoginPanel);

            this.StartCoroutine(LoadSceneCoroutine());
        }

        // 初始化显示
        private IEnumerator LoadSceneCoroutine()
        {
            yield return LevelStartWaiting();
        }

        // 开启场景后屏蔽操作，等待0.5秒钟弹出弹窗
        IEnumerator LevelStartWaiting()
        {
            WindowManager.Instance.ShowMaskBox((int)MaskType.CHANGE_SCENE, true);
            yield return new WaitForSecondsRealtime(1f);
            WindowManager.Instance.ShowMaskBox((int)MaskType.CHANGE_SCENE, false);

            SceneLoadEndHandler();
        }

        public override void Tick()
        {
            base.Tick();
            Log.Debug("login update");
        }

        public override void SceneExitHandler()
        {
            base.SceneExitHandler();

            Log.Debug("退出login场景");
        }
    }
}