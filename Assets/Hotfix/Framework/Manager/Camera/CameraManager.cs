using UnityEngine;

namespace LccHotfix
{
    internal class CameraManager : Module
    {
        public static CameraManager Instance => Entry.GetModule<CameraManager>();
        private Camera _mainCamera = null;
        public Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = GameObject.Find("Global/MainCamera").GetComponent<Camera>();
                }
                return _mainCamera;
            }
        }

        private Camera _uiCamera = null;
        public Camera UICamera
        {
            get
            {
                if (_uiCamera == null)
                {
                    _uiCamera = GameObject.Find("Global/UI Root/UICamera").GetComponent<Camera>();
                }
                return _uiCamera;
            }
        }

        private Camera _currentCamera = null;
        public Camera CurrentCamera
        {
            get
            {
                if (_currentCamera == null)
                {
                    return null;
                }
                return _currentCamera;
            }
            set
            {
                _currentCamera = value;
            }
        }
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }
        internal override void Shutdown()
        {
            //var cameraData = MainCamera.GetComponent<UniversalAdditionalCameraData>();
            //cameraData.cameraStack.Clear();
            //cameraData.cameraStack.Add(UICamera);
            //if (cameraData.renderType == CameraRenderType.Base && cameraData.cameraStack.Count > 0)
            //{
            //    cameraData.cameraStack.ForEach(subCamera => subCamera.rect = MainCamera.rect);
            //}
        }
        public void AddOverlayCamera(Camera camera)
        {
            //var cameraData = MainCamera.GetComponent<UniversalAdditionalCameraData>();
            //cameraData.cameraStack.Clear();
            //cameraData.cameraStack.Add(camera);
            //cameraData.cameraStack.Add(UICamera);
            //if (cameraData.renderType == CameraRenderType.Base && cameraData.cameraStack.Count > 0)
            //{
            //    cameraData.cameraStack.ForEach(subCamera => subCamera.rect = MainCamera.rect);
            //}
        }
        public void RemoveOverlayCamera(Camera camera)
        {
            //var cameraData = MainCamera.GetComponent<UniversalAdditionalCameraData>();
            //cameraData.cameraStack.Remove(camera);
        }
    }
}