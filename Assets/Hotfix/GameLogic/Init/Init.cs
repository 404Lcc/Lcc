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
                Launcher.Instance.actionFixedUpdate += FixedUpdate;
                Launcher.Instance.actionUpdate += Update;
                Launcher.Instance.actionLateUpdate += LateUpdate;
                Launcher.Instance.actionClose += Close;
                Launcher.Instance.actionOnDrawGizmos += DrawGizmos;

                Launcher.Instance.LoadFinish();
                
                Main.ProcedureService.ChangeProcedure(ProcedureType.Login);
            }
            catch (System.Exception e)
            {
                Log.Error(e);
            }
        }
        private static void FixedUpdate()
        {
            if (!Launcher.Instance.GameStarted)
                return;
        }
        private static void Update()
        {
            if (!Launcher.Instance.GameStarted)
                return;
            Main.Current.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }
        private static void LateUpdate()
        {
            if (!Launcher.Instance.GameStarted)
                return;
            Main.Current.LateUpdate();
        }
        private static void DrawGizmos()
        {
            if (!Launcher.Instance.GameStarted)
                return;
            Main.GizmoService.OnDrawGizmos();
        }
        private static void Close()
        {
            Launcher.Instance.actionFixedUpdate -= FixedUpdate;
            Launcher.Instance.actionUpdate -= Update;
            Launcher.Instance.actionLateUpdate -= LateUpdate;
            Launcher.Instance.actionClose -= Close;
            Launcher.Instance.actionOnDrawGizmos -= DrawGizmos;
            Main.Current.Shutdown();
        }
    }
}