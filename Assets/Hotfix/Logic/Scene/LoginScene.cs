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
            loadType = LoadingType.None;
        }
        public override void SceneStartHandler()
        {
            base.SceneStartHandler();
            //进入

            Log.Debug("进入login场景");

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
            WindowManager.Instance.OpenWindow<UILoginPanel>(UIWindowDefine.UILoginPanel);

            //GetLoginResult
            //如果不在提审状态
            if (!Launcher.Instance.IsAuditServer())
            {
                Launcher.Instance.OpenGameNoticeBoard(() => { /*WindowManager.Instance.OpenWindow<UINoticeBoardPanel>(UIWindowDefine.UINoticeBoardPanel);*/ });
            }
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