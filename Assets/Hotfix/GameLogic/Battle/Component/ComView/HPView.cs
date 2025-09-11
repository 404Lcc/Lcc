using UnityEngine;

namespace LccHotfix
{
    public class HPView : IViewWrapper, IViewUpdate
    {
        private UIHeadbarPanel _panel;

        private LogicEntity _entity;
        private HeadbarType _type;
        private float _offsetY;

        private UIHeadbarTemplate _hp;

        public HPBase HP => _hp as HPBase;

        public virtual void Init(LogicEntity entity, HeadbarType type, float offsetY)
        {
            _entity = entity;
            _type = type;
            _offsetY = offsetY;
            _panel = Main.UIService.GetPanel(UIPanelDefine.UIHeadbarPanel) as UIHeadbarPanel;
        }


        public void Show(bool show)
        {
            if (show)
            {
                if (_hp == null)
                {
                    _hp = _panel.GetHeadbar(_type, _entity, _offsetY);
                }

                if (_hp == null)
                    return;

                HP.SetHp(_entity.comHP.HP, _entity.comProperty.maxHP);
            }
            else
            {
                if (_hp == null)
                {
                    return;
                }

                _panel.RemoveHeadbar(_hp);
                _hp = null;
            }
        }


        public void Dispose()
        {
            Show(false);
        }

        public void SyncTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
        }


        public void Update(float dt)
        {
            if (_hp == null)
                return;

            if (!_entity.hasComProperty)
                return;

            if (!_entity.hasComHP)
                return;

            var comProperty = _entity.comProperty;
            var comHp = _entity.comHP;
            var maxHp = comProperty.maxHP;
            var hp = comHp.HP;

            HP.SetHp(hp, maxHp);
            _hp.Update();
        }
    }
}