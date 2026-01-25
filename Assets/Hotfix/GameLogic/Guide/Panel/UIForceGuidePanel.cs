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
        StartGuide = 0,
        Guiding = 1,
        GuideFinish = 2,
        GuideClose = 3,
    }

    public class UIForceGuidePanel : UIElementBase, ICoroutine
    {
        public GameObject hand;
        public Image maskImg;
        public Button breakBtn;

        //===参数
        public string guidePath;
        public GuideMaskType guideMaskType = 0;
        public Vector2 handOffsetPos;
        //===

        //遮罩
        public PanerateMask panerateMask;

        //引导的obj
        public GameObject guideObj;

        //引导状态
        public GuideStateType guideState;

        //初始化参数
        public Vector4 maskParams = new Vector4(-1000, -1000, 1000, 1000);

        //结果
        public Vector4 maskResultParams = new Vector4(0, 0, 0, 0);

        public float maskProcess;

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

            if (guideState == GuideStateType.Guiding)
            {
                UpdateMask();
            }

            if (guideState == GuideStateType.GuideFinish)
            {
                HideGuide();
            }
        }

        public override object OnHide()
        {
            GameUtility.RemoveHandle<EvtShowForceGuide>(OnEvtShowForceGuide);

            return base.OnHide();
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

        public void StartGuide()
        {
            HideGuide();

            var obj = GameObject.Find(guidePath);

            if (obj == null)
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

            maskResultParams = new Vector4(0, 0, 0, 0);
            maskProcess = 0;

            ShowGuide(obj);

            maskResultParams = GetMaskResultParams(guideMaskType, 0);
            SetMaskParam(guideMaskType, new Vector4(-1000, -1000, 1000, 1000));
            SetHand(new Vector3(handOffsetPos.x, handOffsetPos.y, 0), Quaternion.identity);
            guideState = GuideStateType.Guiding;
        }

        public void ShowGuide(GameObject obj)
        {
            guideObj = obj;
            maskImg.gameObject.SetActive(true);
            panerateMask.AddTarget(guideObj, OnClickGuide);
            guideState = GuideStateType.StartGuide;
        }

        public void HideGuide()
        {
            panerateMask.ClearTarget(OnClickGuide);
            maskImg.gameObject.SetActive(false);
            guideObj = null;
            guideState = GuideStateType.GuideClose;

            EvtClickForceGuideFinish.Broadcast();
        }

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

        private void UpdateMask()
        {
            if (maskParams != maskResultParams)
            {
                maskParams = Vector4.Lerp(maskParams, maskResultParams, maskProcess);
                maskProcess += 0.5f * Time.unscaledDeltaTime;

                if (maskProcess >= 1)
                {
                    maskProcess = 1;
                }

                SetMaskParam(guideMaskType, maskParams);
            }
            else
            {
                SetMaskParam(guideMaskType, maskResultParams);
                guideState = GuideStateType.GuideFinish;
            }
        }

        private void SetMaskParam(GuideMaskType type, Vector4 param)
        {
            maskImg.material.SetInt("_MaskType", (int)type - 1);
            maskImg.material.SetVector("_Origin", param);
        }

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

        public void OnClickGuide(GameObject obj)
        {
            HideGuide();
        }
    }
}