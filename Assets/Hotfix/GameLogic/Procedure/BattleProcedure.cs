using LccModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    [Procedure]
    public class BattleProcedure : LoadProcedureHandler, ICoroutine
    {
        public GameObjectPoolAsyncOperation operation;

        public Camera currentCamera;
        public GameObject Map => operation.GameObject;

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

            operation = Main.GameObjectPoolService.GetObjectAsync("Map", (x) =>
            {
                operation = x;

                SetBattleCamera();

                Main.UIService.ShowElement(UIPanelDefine.UIBattlePanel);
                Main.UIService.ShowElement(UIPanelDefine.UIHeadbarPanel);

                var mod = GameUtility.GetModel<ModPlayer>();
                var data = new InGamePlayerData();
                data.InitData(mod.GetLocalPlayerSimpleData());

                BattleWorldData worldData = new BattleWorldData();
                worldData.Init(new List<InGamePlayerData>() { data });
                Main.WorldService.CreateWorld<BattleWorld>(worldData);

                this.StartCoroutine(LoadProcedureCoroutine());

            });

        }

        // 初始化显示
        public IEnumerator LoadProcedureCoroutine()
        {
            yield return new WaitForSeconds(1f);

            yield return null;

            this.StartCoroutine(LevelStartWaiting());
        }

        // 开启流程后屏蔽操作，等待0.5秒钟弹出弹窗
        IEnumerator LevelStartWaiting()
        {
            yield return new WaitForSecondsRealtime(1f);

            ProcedureLoadEndHandler();
        }

        public void SetBattleCamera()
        {
            currentCamera = Map.transform.Find("Camera").GetComponent<Camera>();
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

            operation.Release(ref operation);

            Main.CameraService.CurrentCamera = null;
            Main.WorldService.ExitWorld();

            Log.Debug("退出Battle");
        }
    }
}