using UnityEngine;

namespace LccHotfix
{
    public class HUDView : IViewWrapper, IViewUpdate
    {
        protected UIHeadbarPanel _panel;

        protected LogicEntity _entity;
        protected HeadbarType _type;
        protected float _offsetY;

        protected UIHeadbarTemplate _headbar;

        public virtual void Init(LogicEntity entity, HeadbarType type, float offsetY)
        {
            _entity = entity;
            _type = type;
            _offsetY = offsetY;
            _panel = Main.WindowService.GetElement<UIHeadbarPanel>(UIPanelDefine.UIHeadbarPanel);
            GetHeadbar();
        }


        public virtual void Dispose()
        {
            if (_headbar == null)
            {
                return;
            }

            _panel.RemoveHeadbar(_headbar);
            _headbar = null;
        }

        public virtual void SyncTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {

        }


        public virtual void Update(float dt)
        {
            if (_headbar == null)
                return;

            _headbar.Update();
        }

        public virtual void GetHeadbar()
        {
            _headbar = _panel.GetHeadbar(_type, _entity, _offsetY);
        }
    }
}