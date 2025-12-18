using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{

    public enum TipsAutoDirType
    {
        UpDown,
        LeftRight,
        Auto,
    }

    public enum TipsDirType
    {
        Up,
        Down,
        Left,
        Right,
    }

    public class UITipsBase : UIElementBase
    {
        private Camera _uiCamera;
        private RectTransform _maxRect;
        private float[] _maxRectLocalPos = new float[4]; //边界（上下左右）

        private RectTransform _tipsRect;
        private float[] _tipsRectLocalPos = new float[4]; //tips区域（上下左右）

        private RectTransform _arrowRect;

        private TipsAutoDirType _autoDirType = TipsAutoDirType.UpDown; //方向类型
        private TipsDirType _dirType = TipsDirType.Up;
        private float _offset;

        private bool _isInit;
        
        public override void OnConstruct()
        {
            base.OnConstruct();
            
            LayerID = UILayerID.Popup;
            IsFullScreen = false;
            EscapeType = EscapeType.Skip;
        }

        public override void OnShow(object[] paramsList)
        {
            base.OnShow(paramsList);

            _uiCamera = Main.CameraService.UICamera;
            _maxRect = GameObject.transform as RectTransform;

            Init(TipsAutoDirType.Auto, GameObject.transform.Find("MaxRect/TipsRect").transform as RectTransform, null);

        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!_isInit)
                return;
            Main.CameraService.CurrentCamera = Main.CameraService.MainCamera;
            var pos = Main.CameraService.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            SetTipsRectBy3DWorld(pos);

            UpdateMaxRectLocalPos();
            UpdateTipsRectLocalPos();
            UpdateDir();
            Adjust();
            Clamp();
        }

        /// <summary>
        /// 更新最大范围
        /// </summary>
        public void UpdateMaxRectLocalPos()
        {
            Vector3[] tempArray = new Vector3[4];
            _maxRect.GetWorldCorners(tempArray); //左下、左上、右上、右下

            _maxRectLocalPos[0] = ClientTools.Screen2UILocal(ClientTools.UIWorld2Screen( /*左上*/tempArray[1], _uiCamera), _maxRect, _uiCamera).y;
            _maxRectLocalPos[1] = ClientTools.Screen2UILocal(ClientTools.UIWorld2Screen( /*左下*/tempArray[0], _uiCamera), _maxRect, _uiCamera).y;
            _maxRectLocalPos[2] = ClientTools.Screen2UILocal(ClientTools.UIWorld2Screen( /*左下*/tempArray[0], _uiCamera), _maxRect, _uiCamera).x;
            _maxRectLocalPos[3] = ClientTools.Screen2UILocal(ClientTools.UIWorld2Screen( /*右下*/tempArray[3], _uiCamera), _maxRect, _uiCamera).x;
        }

        /// <summary>
        /// 更新Tips范围
        /// </summary>
        public void UpdateTipsRectLocalPos()
        {
            Vector3[] tempArray = new Vector3[4];
            _tipsRect.GetWorldCorners(tempArray); //左下、左上、右上、右下

            _tipsRectLocalPos[0] = ClientTools.Screen2UILocal(ClientTools.UIWorld2Screen( /*左上*/tempArray[1], _uiCamera), _maxRect, _uiCamera).y;
            _tipsRectLocalPos[1] = ClientTools.Screen2UILocal(ClientTools.UIWorld2Screen( /*左下*/tempArray[0], _uiCamera), _maxRect, _uiCamera).y;
            _tipsRectLocalPos[2] = ClientTools.Screen2UILocal(ClientTools.UIWorld2Screen( /*左下*/tempArray[0], _uiCamera), _maxRect, _uiCamera).x;
            _tipsRectLocalPos[3] = ClientTools.Screen2UILocal(ClientTools.UIWorld2Screen( /*右下*/tempArray[3], _uiCamera), _maxRect, _uiCamera).x;
        }

        /// <summary>
        /// 计算最佳显示方向
        /// </summary>
        private void UpdateDir()
        {
            Vector2 localPos = _tipsRect.localPosition;
            float spaceUP = GetMaxUP() - (localPos.y + _tipsRect.rect.height / 2);
            float spaceDown = (localPos.y - _tipsRect.rect.height / 2) - GetMaxDown();
            float spaceLeft = (localPos.x - _tipsRect.rect.width / 2) - GetMaxLeft();
            float spaceRight = GetMaxRight() - (localPos.x + _tipsRect.rect.width / 2);

            // 找出最大可用空间的方向
            float maxSpace = Mathf.Max(spaceUP, spaceDown, spaceLeft, spaceRight);

            switch (_autoDirType)
            {
                case TipsAutoDirType.UpDown:

                    if (maxSpace == spaceUP)
                    {
                        _dirType = TipsDirType.Down;
                    }

                    if (maxSpace == spaceDown)
                    {
                        _dirType = TipsDirType.Up;
                    }

                    break;
                case TipsAutoDirType.LeftRight:

                    if (maxSpace == spaceLeft)
                    {
                        _dirType = TipsDirType.Right;
                    }

                    if (maxSpace == spaceRight)
                    {
                        _dirType = TipsDirType.Left;
                    }


                    break;
                case TipsAutoDirType.Auto:

                    if (maxSpace == spaceUP)
                    {
                        _dirType = TipsDirType.Down;
                    }

                    if (maxSpace == spaceDown)
                    {
                        _dirType = TipsDirType.Up;
                    }

                    if (maxSpace == spaceLeft)
                    {
                        _dirType = TipsDirType.Right;
                    }

                    if (maxSpace == spaceRight)
                    {
                        _dirType = TipsDirType.Left;
                    }

                    break;
            }

            Debug.LogError(_dirType);
        }



        private void Adjust()
        {
            float tooltipRectOffset = _offset; //整体偏移
            float arrowOffset = 0; //箭头偏移
            bool showArrow = _arrowRect != null;


            switch (_dirType)
            {
                case TipsDirType.Up:
                    tooltipRectOffset += _tipsRect.rect.height / 2;

                    if (showArrow)
                    {
                        tooltipRectOffset += _arrowRect.rect.height / 2;
                        arrowOffset += _tipsRect.rect.height / 2;
                        arrowOffset += _arrowRect.rect.height / 2;


                        _arrowRect.localPosition += Vector3.down * arrowOffset;
                        _arrowRect.eulerAngles = new Vector3(0, 0, 0);
                    }

                    _tipsRect.localPosition += Vector3.up * tooltipRectOffset;
                    break;
                case TipsDirType.Down:
                    tooltipRectOffset += _tipsRect.rect.height / 2;

                    if (showArrow)
                    {
                        tooltipRectOffset += _arrowRect.rect.height / 2;
                        arrowOffset += _tipsRect.rect.height / 2;
                        arrowOffset += _arrowRect.rect.height / 2;

                        _arrowRect.localPosition += Vector3.up * arrowOffset;
                        _arrowRect.eulerAngles = new Vector3(0, 0, 180);
                    }

                    _tipsRect.localPosition += Vector3.down * tooltipRectOffset;
                    break;
                case TipsDirType.Left:
                    tooltipRectOffset += _tipsRect.rect.width / 2;

                    if (showArrow)
                    {
                        tooltipRectOffset += _arrowRect.rect.width / 2;
                        arrowOffset += _tipsRect.rect.width / 2;
                        arrowOffset += _arrowRect.rect.width / 2;

                        _arrowRect.localPosition += Vector3.right * arrowOffset;
                        _arrowRect.eulerAngles = new Vector3(0, 0, -90);
                    }

                    _tipsRect.localPosition += Vector3.left * tooltipRectOffset;
                    break;
                case TipsDirType.Right:
                    tooltipRectOffset += _tipsRect.rect.width / 2;
                    if (showArrow)
                    {
                        tooltipRectOffset += _arrowRect.rect.width / 2;
                        arrowOffset += _tipsRect.rect.width / 2;
                        arrowOffset += _arrowRect.rect.width / 2;

                        _arrowRect.localPosition += Vector3.left * arrowOffset;
                        _arrowRect.eulerAngles = new Vector3(0, 0, 90);
                    }

                    _tipsRect.localPosition += Vector3.right * tooltipRectOffset;
                    break;
                default:
                    break;
            }
        }

        private void Clamp()
        {
            var pos = _tipsRect.localPosition;

            float offsetX = 0;
            float offsetY = 0;

            switch (_dirType)
            {
                case TipsDirType.Up:
                case TipsDirType.Down:
                    if (GetTipsRight() > GetMaxRight())
                    {
                        offsetX = GetTipsRight() - GetMaxRight();

                        pos.x -= offsetX;
                    }
                    else if (GetTipsLeft() < GetMaxLeft())
                    {
                        offsetX = GetMaxLeft() - GetTipsLeft();

                        pos.x += offsetX;
                    }

                    break;
                case TipsDirType.Left:
                case TipsDirType.Right:
                    if (GetTipsUP() > GetMaxUP())
                    {
                        offsetY = GetTipsUP() - GetMaxUP();

                        pos.y -= offsetY;
                    }
                    else if (GetTipsDown() < GetMaxDown())
                    {
                        offsetY = GetMaxDown() - GetTipsDown();

                        pos.y += offsetY;
                    }

                    break;
            }

            _tipsRect.localPosition = pos;
        }

        #region 接口

        public void Init(TipsAutoDirType autoDirType, RectTransform tipsRect, RectTransform arrowRect)
        {
            _autoDirType = autoDirType;
            _tipsRect = tipsRect;
            _arrowRect = arrowRect;

            _isInit = true;
        }

        public void SetTipsRectByUIWorld(Vector3 pos)
        {
            Vector2 screenPoint = ClientTools.UIWorld2Screen(pos, _uiCamera);
            var localPos = ClientTools.Screen2UILocal(screenPoint, _maxRect, _uiCamera);
            _tipsRect.localPosition = localPos;
        }

        public void SetTipsRectBy3DWorld(Vector3 pos)
        {
            Vector2 screenPoint = Main.CameraService.CurrentCamera.WorldToScreenPoint(pos);
            var localPos = ClientTools.Screen2UILocal(screenPoint, _maxRect, _uiCamera);
            _tipsRect.localPosition = localPos;
        }

        private float GetMaxUP()
        {
            return _maxRectLocalPos[0];
        }

        private float GetMaxDown()
        {
            return _maxRectLocalPos[1];
        }

        private float GetMaxLeft()
        {
            return _maxRectLocalPos[2];
        }

        private float GetMaxRight()
        {
            return _maxRectLocalPos[3];
        }

        private float GetTipsUP()
        {
            return _tipsRectLocalPos[0];
        }

        private float GetTipsDown()
        {
            return _tipsRectLocalPos[1];
        }

        private float GetTipsLeft()
        {
            return _tipsRectLocalPos[2];
        }

        private float GetTipsRight()
        {
            return _tipsRectLocalPos[3];
        }

        #endregion
    }
}