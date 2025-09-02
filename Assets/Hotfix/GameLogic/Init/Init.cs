using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public partial class Init
    {
        public static void Start()
        {
            Log.SetLogHelper(new DefaultLogHelper());
            Main.SetMain(new GameMain());
            
            try
            {
                Launcher.Instance.GameAction.OnFixedUpdate += FixedUpdate;
                Launcher.Instance.GameAction.OnUpdate += Update;
                Launcher.Instance.GameAction.OnLateUpdate += LateUpdate;
                Launcher.Instance.GameAction.OnClose += Close;
                Launcher.Instance.GameAction.OnDrawGizmos += DrawGizmos;

                Launcher.Instance.LauncherFinish();
                
                Main.ProcedureService.ChangeProcedure(ProcedureType.Login);
            }
            catch (System.Exception e)
            {
                Log.Error(e);
            }
        }
        private static void FixedUpdate()
        {
        }
        private static void Update()
        {
            Main.Current.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }
        private static void LateUpdate()
        {
            Main.Current.LateUpdate();
        }
        private static void DrawGizmos()
        {
            Main.GizmoService.OnDrawGizmos();
        }
        private static void Close()
        {
            Launcher.Instance.GameAction.OnFixedUpdate -= FixedUpdate;
            Launcher.Instance.GameAction.OnUpdate -= Update;
            Launcher.Instance.GameAction.OnLateUpdate -= LateUpdate;
            Launcher.Instance.GameAction.OnClose -= Close;
            Launcher.Instance.GameAction.OnDrawGizmos -= DrawGizmos;
            Main.Current.Shutdown();
        }
    }
}