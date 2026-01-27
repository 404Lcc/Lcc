using LccModel;
using UnityEngine;
using UnityEngine.UI;
using Time = UnityEngine.Time;

namespace LccHotfix
{
    /// <summary>
    /// 新手引导裁剪类型
    /// </summary>
    public enum GuideMaskType
    {
        None = 0,
        Circle = 1,
        Rectangle = 2,
    }

    public enum GuideStateType
    {
        StartGuideMask = 1, //开始引导遮罩
        GuideMaskFinish = 2, //引导遮罩完成
        GuideFinish = 3, //引导完成
    }

    public class UIForceGuidePanel : UIElementBase, ICoroutine
    {
        public GameObject hand;
        public Image mask;

        //===参数
        public string guidePath;
        public GuideMaskType guideMaskType = 0;
        public Vector2 handOffsetPos;
        //===

        //遮罩穿透
        public PanerateMask panerateMask;

        //引导的obj
        public GameObject guideObj;

        //引导状态
        public GuideStateType guideStateType;

        //遮罩初始化参数
        public Vector4 maskParams = new Vector4(-1000, -1000, 1000, 1000);

        //结果
        public Vector4 maskResultParams = new Vector4(0, 0, 0, 0);

        //点击计数
        public int clickMaskNum;

        //是否异常
        public bool isException;

        public override void OnConstruct()
        {
            base.OnConstruct();

            LayerID = UILayerID.Guide;
            IsFullScreen = false;
        }

        public override void OnShow(object[] paramsList)
        {
            base.OnShow(paramsList);

            GameUtility.AddHandle<EvtShowForceGuide>(OnEvtShowForceGuide);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (guideObj == null)
                return;

            if (guideStateType == GuideStateType.StartGuideMask)
            {
                UpdateMask();
            }

            if (guideStateType == GuideStateType.GuideMaskFinish)
            {
                CheckGuideException();
            }
        }

        public override object OnHide()
        {
            GameUtility.RemoveHandle<EvtShowForceGuide>(OnEvtShowForceGuide);

            return base.OnHide();
        }

        /// <summary>
        /// 比较两个 Vector4 是否近似相等
        /// </summary>
        private bool Approximately(Vector4 a, Vector4 b, float epsilon = 0.01f)
        {
            return Mathf.Abs(a.x - b.x) < epsilon &&
                   Mathf.Abs(a.y - b.y) < epsilon &&
                   Mathf.Abs(a.z - b.z) < epsilon &&
                   Mathf.Abs(a.w - b.w) < epsilon;
        }

        /// <summary>
        /// 更新遮罩
        /// </summary>
        private void UpdateMask()
        {
            if (!Approximately(maskParams, maskResultParams))
            {
                maskParams = Vector4.Lerp(maskParams, maskResultParams, 0.1f);

                //如果非常接近目标值，直接设置为目标值
                if (Approximately(maskParams, maskResultParams, 0.001f))
                {
                    maskParams = maskResultParams;
                }

                SetMaskParam(guideMaskType, maskParams);
            }
            else
            {
                SetMaskParam(guideMaskType, maskResultParams);
                guideStateType = GuideStateType.GuideMaskFinish;
            }
        }

        public void CheckGuideException()
        {
            if (clickMaskNum >= 20 && !isException)
            {
                isException = true;
                //todo
            }
        }

        // 第1个参数：裁剪类型
        // 第2个参数：UIPath
        // 第3个参数：手位置
        private void OnEvtShowForceGuide(IEventMessage obj)
        {
            var e = obj as EvtShowForceGuide;
            guidePath = e.guidePath;
            guideMaskType = e.type;
            handOffsetPos = e.handOffsetPos;

            if (guideMaskType == GuideMaskType.None)
            {
                return;
            }

            StartGuide();
        }

        /// <summary>
        /// 开始引导
        /// </summary>
        private void StartGuide()
        {
            HideGuide();

            guideObj = GameObject.Find(guidePath);

            if (guideObj == null)
            {
                Debug.LogError("路径未找到" + guidePath);
                return;
            }

            switch (guideMaskType)
            {
                case GuideMaskType.Circle:
                    maskParams = new Vector4(0, 0, 1000, 0);
                    break;
                case GuideMaskType.Rectangle:
                    maskParams = new Vector4(-1000, -1000, 1000, 1000);
                    break;
            }

            maskResultParams = GetMaskResultParams(guideMaskType, 0);

            clickMaskNum = 0;
            isException = false;
            mask.gameObject.SetActive(true);
            panerateMask.SetClickMask(() => { clickMaskNum++; });
            panerateMask.AddTarget(guideObj, OnClickGuide);
            SetMaskParam(guideMaskType, new Vector4(-1000, -1000, 1000, 1000));
            SetHand(new Vector3(handOffsetPos.x, handOffsetPos.y, 0), Quaternion.identity);
            guideStateType = GuideStateType.StartGuideMask;
        }

        /// <summary>
        /// 获取遮罩结果
        /// </summary>
        /// <param name="type"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private Vector4 GetMaskResultParams(GuideMaskType type, float radius)
        {
            Vector4 res = new Vector4(0, 0, 0, 0);

            RectTransform rectTrans = guideObj.GetComponent<RectTransform>();
            if (rectTrans != null)
            {
                var cam = Main.CameraService.UICamera;
                var screenPoint = RectTransformUtility.WorldToScreenPoint(cam, guideObj.transform.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(this.GameObject.transform as RectTransform, screenPoint, cam, out Vector2 pos);
                float pivotX = rectTrans.pivot.x;
                float pivotY = rectTrans.pivot.y;
                float posX = -(pivotX - 0.5f) * rectTrans.rect.size.x + pos.x;
                float posY = -(pivotY - 0.5f) * rectTrans.rect.size.y + pos.y;
                // posY -= Screen.safeArea.position.y / 2;

                if (type == GuideMaskType.Rectangle)
                {
                    res.x = posX - rectTrans.rect.size.x / 2;
                    res.y = posY - rectTrans.rect.size.y / 2;
                    res.z = posX + rectTrans.rect.size.x / 2;
                    res.w = posY + rectTrans.rect.size.y / 2;
                }
                else if (type == GuideMaskType.Circle)
                {
                    res.x = posX;
                    res.y = posY;
                    res.z = radius;
                }
            }

            return res;
        }

        /// <summary>
        /// 设置遮罩参数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="param"></param>
        private void SetMaskParam(GuideMaskType type, Vector4 param)
        {
            mask.material.SetInt("_MaskType", (int)type - 1);
            mask.material.SetVector("_Origin", param);
        }

        /// <summary>
        /// 设置引导手位置
        /// </summary>
        /// <param name="handPos"></param>
        /// <param name="handRot"></param>
        private void SetHand(Vector3 handPos, Quaternion handRot)
        {
            RectTransform rectTrans = guideObj.GetComponent<RectTransform>();
            if (rectTrans != null)
            {
                var cam = Main.CameraService.UICamera;
                var screenPoint = RectTransformUtility.WorldToScreenPoint(cam, guideObj.transform.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(this.GameObject.transform as RectTransform, screenPoint, cam, out Vector2 pos);
                float pivotX = rectTrans.pivot.x;
                float pivotY = rectTrans.pivot.y;
                float posX = -(pivotX - 0.5f) * rectTrans.rect.size.x + pos.x + handPos.x;
                float posY = -(pivotY - 0.5f) * rectTrans.rect.size.y + pos.y + handPos.y;
                hand.transform.localPosition = new Vector3(posX, posY, 0);
            }

            hand.transform.localRotation = handRot;
        }

        /// <summary>
        /// 隐藏引导
        /// </summary>
        private void HideGuide()
        {
            clickMaskNum = 0;
            isException = false;
            mask.gameObject.SetActive(false);
            panerateMask.SetClickMask(null);
            panerateMask.ClearTarget(OnClickGuide);
            guideObj = null;
            guideStateType = GuideStateType.GuideFinish;

            EvtClickForceGuideFinish.Broadcast();
        }

        public void OnClickGuide(GameObject obj)
        {
            HideGuide();
        }
    }
}