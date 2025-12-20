using UnityEngine;
#if URP
using UnityEngine.Rendering.Universal;
#endif

namespace LccModel
{
    public class UIAdapt : MonoBehaviour
    {
        //最小适配比
#if Horizontal
        public const float MinAdaptScal = 16 / 9f;
        public const float MaxAdaptScal = 2400 / 1080f;
#elif Vertical
        public const float MinAdaptScal = 9 / 16f;
        public const float MaxAdaptScal = 1080 / 2400f;
#endif

        public float leftOffset;
        public float rightOffset;
        
        private int _scalerChangeFrameCount = -1;
        private RectTransform _adaptRoot;
        private RectTransform _uiUpCanvasRoot;
        private RectTransform _uiDownCanvasRoot;
        private RectTransform _uiLeftCanvasRoot;
        private RectTransform _uiRightCanvasRoot;
        private Camera _mainCamera;
        
        public float ScreenRatio
        {
            get
            {
                return (float)Screen.width / Screen.height;
            }
        }
        private void Start()
        {
            _adaptRoot = (RectTransform)GameObject.Find("Global/AdaptCanvas").transform;
            _uiUpCanvasRoot = (RectTransform)GameObject.Find("Global/AdaptCanvas/UpCanvas").transform;
            _uiDownCanvasRoot = (RectTransform)GameObject.Find("Global/AdaptCanvas/DownCanvas").transform;
            _uiLeftCanvasRoot = (RectTransform)GameObject.Find("Global/AdaptCanvas/LeftCanvas").transform;
            _uiRightCanvasRoot = (RectTransform)GameObject.Find("Global/AdaptCanvas/RightCanvas").transform;
            //urp下设置主相机 buildin下设置ui相机
#if URP
            _mainCamera = GameObject.Find("Global/MainCamera").GetComponent<Camera>();
#else
            _mainCamera = GameObject.Find("Global/UIRoot/UICamera").GetComponent<Camera>();
#endif
            AdaptUIRoot(true, true, leftOffset, rightOffset);
        }
        public void Update()
        {
            if (_scalerChangeFrameCount != -1 && _scalerChangeFrameCount < Time.frameCount)
            {
                AdaptUIRoot(true, true, leftOffset, rightOffset);
            }

        }

        public void UIOffset(float offect)
        {
            this.leftOffset = offect;
            this.rightOffset = offect;
            AdaptUIRoot(true, true, leftOffset, rightOffset);
        }

        /// <summary>
        /// 如果通过屏幕分辨率进行适配，则会自动适配到安全区域
        /// </summary>
        /// <param name="adaptScreenRatio"></param>
        /// <param name="leftOffset"></param>
        /// <param name="rightOffset"></param>
        public void AdaptUIRoot(bool adaptScreenRatio, bool adaptSafeArea, float leftOffset = 0, float rightOffset = 0)
        {
            float tempUP = 0;
            float tempDown = 0;
            float tempLeft = 0;
            float tempRight = 0;

            float maxSafe = Mathf.Max(Screen.safeArea.xMin, Screen.width - Screen.safeArea.xMax);

            //1.首先适配安全区
            if (adaptSafeArea)
            {
                float ratioWidth = maxSafe / Screen.width;
                tempLeft = _adaptRoot.rect.width * ratioWidth;
                tempRight = tempLeft;

                tempLeft = leftOffset;
                tempRight = rightOffset;

            }

            if (adaptScreenRatio)
            {
                if (ScreenRatio < MinAdaptScal)
                {
                    var delta = (_adaptRoot.rect.size.y - (_adaptRoot.rect.size.x - tempLeft - tempRight) / MinAdaptScal) / 2f;
                    tempUP = tempDown = delta;
                }
                else if (ScreenRatio > MaxAdaptScal)//超过了最大适配，填充边
                {
                    float width = (_adaptRoot.rect.size.x - _adaptRoot.rect.size.y * MaxAdaptScal) / 2;
                    tempLeft = tempLeft > width ? tempLeft : width;
                    tempRight = tempLeft;
                }
            }

            Rect realRect = new Rect(tempLeft / _adaptRoot.rect.size.x, tempDown / _adaptRoot.rect.size.y, 
                1 - (tempLeft + tempRight) / _adaptRoot.rect.size.x, 1 - (tempUP + tempDown) / _adaptRoot.rect.size.y);


            if (!_mainCamera.rect.Equals(realRect))
            {
                _scalerChangeFrameCount = Time.frameCount;
                _mainCamera.rect = realRect;
                //urp下需要处理subCamera的rect
#if URP
                var cameraData = _mainCamera.GetComponent<UniversalAdditionalCameraData>();
                if (cameraData.renderType == CameraRenderType.Base && cameraData.cameraStack.Count > 0)
                {
                    cameraData.cameraStack.ForEach(subCamera => subCamera.rect = _mainCamera.rect);
                }
#endif
                return;
            }
            _scalerChangeFrameCount = -1;
            SetAdaptUIRoot(tempUP, tempDown, tempLeft, tempRight);
        }

        private void SetAdaptUIRoot(float up, float down, float left, float right)
        {
            //-UP
            SetTransOffset(_uiUpCanvasRoot, 0, _adaptRoot.rect.height - up, 0, 0);
            //-Down
            SetTransOffset(_uiDownCanvasRoot, _adaptRoot.rect.height - down, 0, 0, 0);
            //-Left
            SetTransOffset(_uiLeftCanvasRoot, 0, 0, 0, _adaptRoot.rect.width - left);
            //-Down
            SetTransOffset(_uiRightCanvasRoot, 0, 0, _adaptRoot.rect.width - right, 0);
        }

        private void SetTransOffset(RectTransform rectTrans, float up, float down, float left, float right)
        {
            var max = rectTrans.offsetMax;
            var min = rectTrans.offsetMin;
            max.y = -up;
            min.y = down;
            max.x = -right;
            min.x = left;
            rectTrans.offsetMax = max;
            rectTrans.offsetMin = min;
        }
    }
}