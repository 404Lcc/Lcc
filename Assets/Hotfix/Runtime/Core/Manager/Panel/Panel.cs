using System.Collections.Generic;
using UnityEngine;
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
    public interface IPanelHandler
    {
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="panel"></param>
        void OnInitData(Panel panel);

        /// <summary>
        /// 初始化业务逻辑
        /// </summary>
        /// <param name="panel"></param>
        void OnInitComponent(Panel panel);

        /// <summary>
        /// 注册UI业务逻辑事件
        /// </summary>
        /// <param name="panel"></param>
        void OnRegisterUIEvent(Panel panel);

        /// <summary>
        /// 显示UI界面
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="contextData"></param>
        void OnShow(Panel panel, AObjectBase contextData = null);

        /// <summary>
        /// 隐藏UI界面
        /// </summary>
        /// <param name="panel"></param>
        void OnHide(Panel panel);

        /// <summary>
        /// 重置界面
        /// </summary>
        /// <param name="panel"></param>
        void OnReset(Panel panel);

        /// <summary>
        /// 销毁界面之前
        /// </summary>
        /// <param name="panel"></param>
        void OnBeforeUnload(Panel panel);

        /// <summary>
        /// 判断是否返回
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        bool IsReturn(Panel panel);
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
    public class PanelCompare : IComparer<Panel>
    {
        public int Compare(Panel left, Panel right)
        {
            //返回值1，则left > right
            //返回值0，则left = right
            //返回值-1，则left < right
            return left.Depth - right.Depth;
        }
    }
    public class Panel : AObjectBase
    {
        private const int NormalDepth = 1000;
        private const int FixedDepth = 2000;
        private const int PopupDepth = 3000;

        private PanelType _type;
        private PanelType _preType;
        private bool _isLock;

        private GameObject _gameObject;

        public PanelData data;
        public IPanelHandler Logic
        {
            get
            {
                return PanelManager.Instance.GetPanelLogic(Type);
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
                switch (data.type)
                {
                    case UIType.Normal:
                        return NormalDepth + Transform.GetSiblingIndex();
                    case UIType.Fixed:
                        return FixedDepth + Transform.GetSiblingIndex();
                    case UIType.Popup:
                        return PopupDepth + Transform.GetSiblingIndex();
                }
                return -1;
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