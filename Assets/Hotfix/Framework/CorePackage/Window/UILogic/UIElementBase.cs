using System;
using UnityEngine;
using LccModel;

namespace LccHotfix
{
    public abstract class UIElementBase : IUILogic
    {
        public UINode Node { get; set; }

        public GameObject GameObject
        {
            get
            {
                if (Node is ElementNode elementNode)
                {
                    return elementNode.GameObject;
                }

                return null;
            }
        }

        public RectTransform RectTransform
        {
            get
            {
                if (Node is ElementNode elementNode)
                {
                    return elementNode.RectTransform;
                }

                return null;
            }
        }

        public virtual void OnConstruct()
        {
            
        }

        public virtual void OnCreate()
        {
            if (Node is ElementNode)
            {
                AutoReferenceUtility.AutoReference(this, RectTransform);
                ShowView(GameObject);
            }
        }

        public virtual void OnSwitch(Action<bool> callback)
        {
            callback?.Invoke(true);
        }

        public virtual void OnCovered(bool covered)
        {
        }

        public virtual void OnShow(object[] paramsList)
        {
        }

        public virtual void OnReShow(object[] paramsList)
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual object OnHide()
        {
            return null;
        }

        public virtual void OnDestroy()
        {
        }

        public virtual bool OnEscape(ref EscapeType escapeType)
        {
            escapeType = Node.EscapeType;
            if (escapeType == EscapeType.Skip)
                return false;
            else
                return true;
        }

        protected object Hide()
        {
            return Node.Hide();
        }

        public void ShowView(GameObject gameObject)
        {
            LccView view = gameObject.AddComponent<LccView>();
            view.className = GetType().Name;
            view.type = this;
        }
    }
}