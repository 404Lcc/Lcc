using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public enum UIType
    {
        Normal,//普通界面
        Fixed,//固定界面
        Popup,//弹出界面
    }
    public enum UIShowMode
    {
        Normal,//什么都不做
        HideOther,//打开当前界面关闭其他界面
    }
    public enum UINavigationMode
    {
        IgnoreNavigation,//忽略导航
        NormalNavigation,
    }
    public class ShowPanelData
    {
        //强制重置窗口
        public bool forceReset;

        //强制清除导航数据
        public bool forceClearBackSequenceData;

        public AObjectBase contextData;

        //执行导航逻辑
        public bool executeNavigationLogic = true;

        //检查导航
        public bool checkNavigation;

        //强制忽略添加导航数据
        public bool ignoreAddNavigationData;

        public ShowPanelData()
        {
        }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="forceReset">强制重置窗口</param>
        /// <param name="forceClearBackSequenceData">强制清除导航数据</param>
        /// <param name="contextData">上下文</param>
        /// <param name="executeNavigationLogic">执行导航逻辑，默认true</param>
        /// <param name="checkNavigation">检查导航</param>
        /// <param name="ignoreAddNavigationData">强制忽略添加导航数据</param>
        public ShowPanelData(bool forceReset, bool forceClearBackSequenceData, AObjectBase contextData, bool executeNavigationLogic, bool checkNavigation, bool ignoreAddNavigationData)
        {
            this.forceReset = forceReset;
            this.forceClearBackSequenceData = forceClearBackSequenceData;
            this.contextData = contextData;
            this.executeNavigationLogic = executeNavigationLogic;
            this.checkNavigation = checkNavigation;
            this.ignoreAddNavigationData = ignoreAddNavigationData;
        }
    }
    public class NavigationData
    {
        public Panel hideTarget;//当前界面
        public List<PanelType> backShowTargets;//当前界面所有显示的界面
    }
    public class PanelData : AObjectBase
    {
        public bool forceClearNavigation = false;
        public UIType type = UIType.Normal;
        public UIShowMode showMode = UIShowMode.Normal;
        public UINavigationMode navigationMode = UINavigationMode.IgnoreNavigation;
    }
    //ilr下有个bug
    //public class PanelCompare : IComparer<Panel>
    //{
    //    public int Compare(Panel left, Panel right)
    //    {
    //        //返回值1，则left > right
    //        //返回值0，则left = right
    //        //返回值-1，则left < right
    //        return left.Depth - right.Depth;
    //    }
    //}
    public class Panel : AObjectBase
    {
        private const int NormalDepth = 1000;
        private const int FixedDepth = 2000;
        private const int PopupDepth = 3000;

        private PanelType _type;
        private PanelType _preType;
        private bool _isLock;

        private int _depth;

        private AssetHandle _loadHandle;
        private GameObject _gameObject;
        private Canvas _canvas;

        public PanelData data;
        public IPanelHandler Logic
        {
            get
            {
                return PanelManager.Instance.GetPanelLogic(Type);
            }
        }
        public AssetHandle LoadHandle
        {
            get
            {
                return _loadHandle;
            }
            set
            {
                _loadHandle = value;
            }
        }
        public GameObject GameObject
        {
            get
            {
                return _gameObject;
            }
            set
            {
                _gameObject = value;
            }
        }
        public Transform Transform
        {
            get
            {
                if (GameObject != null)
                {
                    return GameObject.transform;
                }
                return null;
            }
        }

        public Canvas Canvas
        {
            get
            {
                return _canvas;
            }
            set
            {
                _canvas = value;
            }
        }
        public bool IsLoad => GameObject != null;
        public bool IsLock
        {
            get
            {
                return _isLock;
            }
            set
            {
                _isLock = value;
            }
        }
        public bool IsShown
        {
            get
            {
                return GameObject.activeSelf;
            }
            set
            {
                GameObject.SetActive(value);
            }
        }
        public int Depth
        {
            get
            {
                return _depth;
            }
            set
            {
                switch (data.type)
                {
                    case UIType.Normal:
                        _depth = NormalDepth + value;
                        break;
                    case UIType.Fixed:
                        _depth = FixedDepth + value;
                        break;
                    case UIType.Popup:
                        _depth = PopupDepth + value;
                        break;
                }
                Canvas.sortingOrder = _depth;
            }
        }
        public PanelType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }
        public PanelType PreType
        {
            get
            {
                return _preType;
            }
            private set
            {
                _preType = value;
            }
        }


        public override void InitData(object[] datas)
        {
            base.InitData(datas);

            data = AddChildren<PanelData>();

        }


        public void SetRoot(Transform root)
        {
            if (Transform == null)
            {
                return;
            }
            if (root == null)
            {
                return;
            }
            Transform.SetParent(root, false);
            Transform.transform.localScale = Vector3.one;
            Canvas.overrideSorting = true;
        }



        public override void OnDestroy()
        {
            base.OnDestroy();




            Type = PanelType.None;

            if (data != null && !data.IsDisposed)
            {
                data.Dispose();
            }
            if (GameObject != null)
            {
                Object.Destroy(GameObject);
                GameObject = null;
            }
        }
    }
}