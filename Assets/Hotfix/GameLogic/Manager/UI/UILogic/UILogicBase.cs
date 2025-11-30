using System;
using UnityEngine;
using LccModel;

namespace LccHotfix
{
    public abstract class UILogicBase : IUILogic
    {
        public GameObject gameObject
        {
            get
            {
                if (WNode is Window window)
                {
                    return window.gameObject;
                }

                return null;
            }
        }

        public RectTransform transform
        {
            get
            {
                if (WNode is Window window)
                {
                    return window.transform;
                }

                return null;
            }
        }

        public virtual void CloseWithAni()
        {
            Close();
        }

        private object Close()
        {
            return WNode.Close();
        }

        #region 接口函数

        public WNode WNode { get; set; }

        public virtual void OnStart()
        {
            if (WNode is Window)
            {
                AutoReferenceUtility.AutoReference(this, transform);
                ShowView(gameObject);
            }
        }

        public virtual void OnUpdate()
        {

        }

        public virtual bool EscCanClose()
        {
            return true;
        }

        public virtual void OnOpen(object[] paramsList)
        {

        }



        public virtual object OnClose()
        {
            return null;
        }

        public virtual void OnReset(object[] paramsList)
        {

        }

        public virtual void DoCovered(bool covered)
        {
        }

        public virtual void OnRemove()
        {

        }

        public virtual void OnSwitch(Action<bool> callback)
        {
            callback?.Invoke(true);
        }

        public virtual bool OnEscape(ref EscapeType escapeType)
        {
            escapeType = WNode.escapeType;
            if (EscapeType.SKIP_OVER == escapeType)
                return false;
            else
                return true;
        }

        public virtual void OnChildOpened(WNode child)
        {

        }

        public virtual void OnChildClosed(WNode child)
        {
        }

        public virtual bool OnChildRequireEscape(WNode child)
        {
            return true;
        }

        #endregion

        public void ShowView(GameObject gameObject, GameObject parent = null)
        {
            LccView view = gameObject.AddComponent<LccView>();
            view.className = GetType().Name;
            view.type = this;

            if (parent != null)
            {
                gameObject.transform.SetParent(parent.transform);
            }
        }
    }
}