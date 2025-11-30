using LccModel;

namespace LccHotfix
{
    public partial class Init
    {
        /// <summary>
        /// 重新启动游戏
        /// </summary>
        public static void ReturnToStart()
        {
            //关闭所有协程，如果patchOperation的状态机在运行，这里会杀掉
            Main.CoroutineService.StopAllTypeCoroutines();
            //todo清理菊花界面
            //清理加载界面
            UILoadingPanel.Instance.Hide();

            //重置速度
            Launcher.Instance.GameControl.SetGameSlow(false);
            Launcher.Instance.GameControl.SetGameSpeed(1);
            Launcher.Instance.GameControl.Resume();

            //清理上个玩家数据
            ClearLastUserData();

            //清理流程
            Main.ProcedureService.CleanProcedure();

            //重启
            Launcher.Instance.GameAction.ExecuteOnClose();
            Launcher.Instance.StartLauncher();
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        public static void ReturnToLogin()
        {
            if (Main.ProcedureService.CurState == ProcedureType.None.ToInt() || Main.ProcedureService.CurState == ProcedureType.Login.ToInt())
            {
                ReturnToStart();
                return;
            }

            //关闭所有协程
            Main.CoroutineService.StopAllTypeCoroutines();
            //todo清理菊花界面
            //清理加载界面
            UILoadingPanel.Instance.Hide();

            //重置速度
            Launcher.Instance.GameControl.SetGameSlow(false);
            Launcher.Instance.GameControl.SetGameSpeed(1);
            Launcher.Instance.GameControl.Resume();

            //清理上个玩家数据
            ClearLastUserData();

            Main.ProcedureService.ChangeProcedure(ProcedureType.Login.ToInt());
        }

        private static void ClearLastUserData()
        {
        }
    }
}