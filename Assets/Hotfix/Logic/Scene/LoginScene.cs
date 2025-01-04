using LccModel;
using System.Collections;

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


            this.StartCoroutine(LoadReadyForShow());
        }
        // 初始化显示
        private IEnumerator LoadReadyForShow()
        {
            yield return null;
            StartLevel();

        }
        public void StartLevel()
        {
            SceneLoadEndHandler();

            //GetLoginResult
            WindowManager.Instance.OpenWindow<UILoginPanel>(UIWindowDefine.UILoginPanel);


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