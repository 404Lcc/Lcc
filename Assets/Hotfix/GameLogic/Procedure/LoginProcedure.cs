using LccModel;
using System.Collections;
using UnityEngine;

namespace LccHotfix
{
    [Procedure(ProcedureType.Login)]
    public class LoginProcedure : LoadProcedureHandler, ICoroutine
    {
        public LoginProcedure()
        {
            procedureType = ProcedureType.Login;
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

            Main.UIService.OpenPanel(UIPanelDefine.UILoginPanel);

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
            UI.ShowMaskBox((int)MaskType.CHANGE_PROCEDURE, true);
            yield return new WaitForSecondsRealtime(1f);
            UI.ShowMaskBox((int)MaskType.CHANGE_PROCEDURE, false);

            ProcedureLoadEndHandler();
        }

        public override void Tick()
        {
            base.Tick();
            Log.Debug("login update");
        }

        public override void ProcedureExitHandler()
        {
            base.ProcedureExitHandler();

            Log.Debug("退出login");
        }
    }
}