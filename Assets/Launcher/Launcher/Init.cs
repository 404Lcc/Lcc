using UnityEngine;
#if URP
using UnityEngine.Rendering.Universal;
#endif

namespace LccModel
{
    public class Init : MonoBehaviour
    {
        void Start()
        {
            var mainCamera = GameObject.Find("Global/MainCamera").GetComponent<Camera>();
            var uiCamera = GameObject.Find("Global/UIRoot/UICamera").GetComponent<Camera>();
            var adaptCamera = GameObject.Find("Global/AdaptCamera").GetComponent<Camera>();
            
            var uiCanvas = GameObject.Find("Global/UIRoot/Canvas").GetComponent<Canvas>();
            var adaptCanvas  = GameObject.Find("Global/AdaptCanvas").GetComponent<Canvas>();
            
            //uiCanvas需要在adaptCanvas前面
            uiCanvas.planeDistance = 10;
            adaptCanvas.planeDistance = 0;
#if URP
            //URP设置
            var mainData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
            var uiData = uiCamera.GetComponent<UniversalAdditionalCameraData>();
            var adaptData = adaptCamera.GetComponent<UniversalAdditionalCameraData>();

            mainData.renderType = CameraRenderType.Base;
            uiData.renderType = CameraRenderType.Overlay;
            adaptData.renderType = CameraRenderType.Base;

            mainData.cameraStack.Clear();
            //ui相机先叠在主相机上
            //game相机后叠在主相机上
            mainData.cameraStack.Add(uiCamera);
            adaptData.cameraStack.Clear();

            mainCamera.depth = 0;
            uiCamera.depth = 0;
            //adaptCamera的depth需要在最下面
            adaptCamera.depth = -10;

            //主相机不渲染
            mainCamera.cullingMask = 0;
            uiCamera.cullingMask = LayerMask.GetMask("UI");
            adaptCamera.cullingMask = LayerMask.GetMask("Adapt");
            //game相机显示除了UI和Adapt之外的
#else
            //builtin设置
            mainCamera.depth = 0;
            //game相机需要在主相机和ui相机之间
            //ui相机需要在主相机上面
            uiCamera.depth = 10;
            //adaptCamera的depth需要在最上面
            adaptCamera.depth = 20;

            //主相机需要排除UI和Adapt
            mainCamera.cullingMask = -1;
            mainCamera.cullingMask = ~((1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("Adapt")));
            uiCamera.cullingMask = LayerMask.GetMask("UI");
            adaptCamera.cullingMask = LayerMask.GetMask("Adapt");
#endif
            DontDestroyOnLoad(this.gameObject);
            Launcher.Instance.Init();
        }
    }
}