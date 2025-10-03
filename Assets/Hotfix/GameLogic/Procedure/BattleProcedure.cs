using LccModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    [Procedure]
    public class BattleProcedure : LoadProcedureHandler, ICoroutine
    {
        public GameObject map;
        public Camera currentCamera;

        public BattleProcedure()
        {
            procedureType = ProcedureType.Battle.ToInt();
            loadType = LoadingType.Normal;
        }

        public override void ProcedureStartHandler()
        {
            base.ProcedureStartHandler();

            //进入

            Log.Debug("进入Battle");

            this.StartCoroutine(LoadProcedureCoroutine());

        }

        // 初始化显示
        public IEnumerator LoadProcedureCoroutine()
        {
            UILoadingPanel.Instance.UpdateLoadingPercent(10, 98, 2f);

            Main.AssetService.LoadGameObject("Map", true, out map);

            SetBattleCamera();

            Main.UIService.OpenPanel(UIPanelDefine.UIBattlePanel);
            Main.UIService.OpenPanel(UIPanelDefine.UIHeadbarPanel);

            var mod = GameUtility.GetModel<ModPlayer>();
            var data = new InGamePlayerData();
            data.InitData(mod.GetLocalPlayerSimpleData());

            // BattleModeState state = new BattleModeState();
            // state.Init(map, new List<InGamePlayerData>() { data });
            // Main.WorldService.CreateWorld<BattleWorld>(state);

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
            Main.UIService.TryPopupPanel();
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
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (IsLoading)
            {
                return;
            }
        }

        public override void ProcedureExitHandler()
        {
            base.ProcedureExitHandler();

            Main.CameraService.CurrentCamera = null;
            Main.WorldService.ExitWorld();

            Log.Debug("退出Battle");
        }
    }
}