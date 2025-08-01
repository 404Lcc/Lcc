using LccModel;
using System.Collections;
using UnityEngine;

namespace LccHotfix
{
    [Procedure(ProcedureType.Main)]
    public class MainProcedure : LoadProcedureHandler, ICoroutine
    {
        public GameObject map;
        public Camera currentCamera;
        
        public MainProcedure()
        {
            procedureType = ProcedureType.Main;
            loadType = LoadingType.Normal;
        }
        public override void ProcedureStartHandler()
        {
            base.ProcedureStartHandler();
            //进入

            Log.Debug("进入main");

            this.StartCoroutine(LoadProcedureCoroutine());

        }

        // 初始化显示
        public IEnumerator LoadProcedureCoroutine()
        {
            UILoadingPanel.Instance.UpdateLoadingPercent(10, 98, 2f);

            map = ResGameObject.LoadGameObject("Map", true).ResGO;
            SetBattleCamera();
            BattleGameModeState state = new BattleGameModeState();
            state.Init(map);
            Main.WorldService.CreateWorld<BattleWorld>(state);
            
            Main.IUIService.OpenPanel(UIPanelDefine.UIMainPanel);

            yield return new WaitForSeconds(1f);

            UIForeGroundPanel.Instance.FadeOut(0.5f);

            yield return null;

            UILoadingPanel.Instance.Hide();

            this.StartCoroutine(LevelStartWaiting());
        }
        
        // 开启流程后屏蔽操作，等待0.5秒钟弹出弹窗
        IEnumerator LevelStartWaiting()
        {
            UI.ShowMaskBox((int)MaskType.CHANGE_PROCEDURE, true);
            yield return new WaitForSecondsRealtime(1f);
            UI.ShowMaskBox((int)MaskType.CHANGE_PROCEDURE, false);

            ProcedureLoadEndHandler();
            Main.IUIService.TryPopupPanel();
        }
        
        public void SetBattleCamera()
        {
            currentCamera = map.transform.Find("Camera").GetComponent<Camera>();
            Main.CameraService.CurrentCamera = currentCamera;
            Main.CameraService.AddOverlayCamera(currentCamera);
        }
        
        public override void Tick()
        {
            base.Tick();
            
            if (IsLoading)
            {
                return;
            }

            Main.WorldService.GetWorld().Update();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            
            if (IsLoading)
            {
                return;
            }
            
            Main.WorldService.GetWorld().LateUpdate();
        }

        public override void ProcedureExitHandler()
        {
            base.ProcedureExitHandler();

            Main.CameraService.CurrentCamera = null;
            
            Log.Debug("退出main");
        }
    }
}