using System;

namespace LccHotfix
{
    public abstract class UIDomainBase : IUIDomainLogic
    {
        public UINode Node { get; set; }
        public EscapeType EscapeType { get; protected set; }
        public ReleaseType ReleaseType { get; protected set; }
        
        public virtual void OnConstruct()
        {
            
        }

        public virtual void OnCreate()
        {
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

        public virtual void OnAddChildNode(ElementNode child)
        {
        }

        public virtual void OnRemoveChildNode(ElementNode child)
        {
        }

        public virtual bool OnRequireEscape(ElementNode child)
        {
            return true;
        }

        protected object Hide()
        {
            return Node.Hide();
        }
    }
}