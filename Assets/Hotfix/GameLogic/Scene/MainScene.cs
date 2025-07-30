using LccModel;
using System.Collections;
using UnityEngine;

namespace LccHotfix
{
    [SceneState(SceneType.Main)]
    public class MainScene : LoadSceneHandler, ICoroutine
    {
        public GameObject map;
        public Camera currentCamera;
        
        public MainScene()
        {
            sceneType = SceneType.Main;
            loadType = LoadingType.Normal;
        }
        public override void SceneStartHandler()
        {
            base.SceneStartHandler();
            //进入

            Log.Debug("进入main场景");

            this.StartCoroutine(LoadSceneCoroutine());

        }

        // 初始化显示
        public IEnumerator LoadSceneCoroutine()
        {
            UILoadingPanel.Instance.UpdateLoadingPercent(10, 98, 2f);

            map = ResGameObject.LoadGameObject("Map", true).ResGO;
            SetBattleCamera();
            BattleGameModeState state = new BattleGameModeState();
            state.Init(map);
            Main.WorldService.CreateWorld<BattleWorld>(state);
            
            Main.WindowService.OpenWindow<UIMainPanel>(UIWindowDefine.UIMainPanel);

            yield return new WaitForSeconds(1f);

            UIForeGroundPanel.Instance.FadeOut(0.5f);

            yield return null;

            UILoadingPanel.Instance.Hide();

            this.StartCoroutine(LevelStartWaiting());
        }
        
        // 开启场景后屏蔽操作，等待0.5秒钟弹出弹窗
        IEnumerator LevelStartWaiting()
        {
            Main.WindowService.ShowMaskBox((int)MaskType.CHANGE_SCENE, true);
            yield return new WaitForSecondsRealtime(1f);
            Main.WindowService.ShowMaskBox((int)MaskType.CHANGE_SCENE, false);

            SceneLoadEndHandler();
            Main.WindowService.TryPopupWindow();
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

        public override void SceneExitHandler()
        {
            base.SceneExitHandler();

            Main.CameraService.CurrentCamera = null;
            
            Log.Debug("退出main场景");
        }
    }
}