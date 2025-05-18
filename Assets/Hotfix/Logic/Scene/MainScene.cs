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
            WorldManager.Instance.CreateWorld<BattleWorld>(state);
            
            WindowManager.Instance.OpenWindow<UIMainPanel>(UIWindowDefine.UIMainPanel);

            yield return new WaitForSeconds(1f);

            UIForeGroundPanel.Instance.FadeOut(0.5f);

            yield return null;

            UILoadingPanel.Instance.Hide();

            this.StartCoroutine(LevelStartWaiting());
        }
        
        // 开启场景后屏蔽操作，等待0.5秒钟弹出弹窗
        IEnumerator LevelStartWaiting()
        {
            WindowManager.Instance.ShowMaskBox((int)MaskType.CHANGE_SCENE, true);
            yield return new WaitForSecondsRealtime(1f);
            WindowManager.Instance.ShowMaskBox((int)MaskType.CHANGE_SCENE, false);

            SceneLoadEndHandler();
            WindowManager.Instance.TryPopupWindow();
        }
        
        public void SetBattleCamera()
        {
            currentCamera = map.transform.Find("Camera").GetComponent<Camera>();
            CameraManager.Instance.CurrentCamera = currentCamera;
            CameraManager.Instance.AddOverlayCamera(currentCamera);
        }
        
        public override void Tick()
        {
            base.Tick();
            
            if (IsLoading)
            {
                return;
            }

            WorldManager.Instance.GetWorld().Update();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            
            if (IsLoading)
            {
                return;
            }
            
            WorldManager.Instance.GetWorld().LateUpdate();
        }

        public override void SceneExitHandler()
        {
            base.SceneExitHandler();

            CameraManager.Instance.CurrentCamera = null;
            
            Log.Debug("退出main场景");
        }
    }
}