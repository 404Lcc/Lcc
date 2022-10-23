using LccModel;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public enum UIType
    {
        Normal,//普通界面
        Fixed,//固定界面
        Popup,//弹出界面
        Other,//其他界面
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
        /// 销毁界面之前
        /// </summary>
        /// <param name="panel"></param>
        void BeforeUnload(Panel panel);
    }
    public class ShowPanelData : AObjectBase
    {
        public AObjectBase contextData;
    }
    public class PanelData : AObjectBase
    {
        public UIType type = UIType.Normal;
    }
    public class Panel : AObjectBase
    {
        private GameObject _gameObject;
        private PanelType _type;

        public PanelData data;
        public IPanelHandler logic;
        public GameObject GameObject
        {
            get
            {
                return _gameObject;
            }
            set
            {
                _gameObject = value;
                if (Type != PanelType.None)
                {
                    string name = Type.ToPanelString();
                    Type type = Manager.Instance.GetType(name);
                    if (type != null)
                    {
                        object ui = Activator.CreateInstance(type);
                        if (ui is IPanelHandler logic)
                        {
                            this.logic = logic;
                        }
                        else
                        {
                            LogUtil.LogError($"{name} 未继承IPanelHandler");
                        }
                    }
                    else
                    {
                        LogUtil.LogError($"UI逻辑未找到 {name}");
                    }
                }
            }
        }
        public bool IsLoad
        {
            get
            {
                return GameObject != null;
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
        public bool IsInStackQueue
        {
            get;
            set;
        }
        public override void InitData(object[] datas)
        {
            base.InitData(datas);

            data = AddChildren<PanelData>();
            IsInStackQueue = false;
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
            IsInStackQueue = false;
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