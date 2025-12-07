using LccModel;
using System.Collections;
using UnityEngine;

namespace LccHotfix
{
    [Procedure]
    public class LoginProcedure : LoadProcedureHandler, ICoroutine
    {
        public LoginProcedure()
        {
            procedureType = ProcedureType.Login.ToInt();
            loadType = LoadingType.Fast;
        }

        public override void ProcedureStartHandler()
        {
            base.ProcedureStartHandler();
            //进入

            Log.Debug("进入login");

            //设备id
            Log.Debug("设备id = " + UnityEngine.SystemInfo.deviceUniqueIdentifier);

            //进入游戏
            UILoadingPanel.Instance.SetStartLoadingBg();
            Main.UIService.OpenPanel(UIPanelDefine.UILoginPanel, UIRootDefine.UIRootLogin);

            this.StartCoroutine(LoadProcedureCoroutine());
        }

        // 初始化显示
        private IEnumerator LoadProcedureCoroutine()
        {
            yield return LevelStartWaiting();
        }

        // 开启流程后屏蔽操作，等待0.5秒钟弹出弹窗
        IEnumerator LevelStartWaiting()
        {
            yield return new WaitForSecondsRealtime(1f);

            ProcedureLoadEndHandler();
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override void ProcedureExitHandler()
        {
            base.ProcedureExitHandler();

            Log.Debug("退出login");
        }
    }
}