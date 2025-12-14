using UnityEngine;

namespace LccHotfix
{
    public class HPView : HUDView
    {
        public HPBase HP => _headbar as HPBase;

        public override void Init(LogicEntity entity, HeadbarType type, float offsetY)
        {
            _entity = entity;
            _type = type;
            _offsetY = offsetY;
            _panel = Main.WindowService.GetElement<UIHeadbarPanel>(UIPanelDefine.UIHeadbarPanel);
        }



        public override void Update(float dt)
        {
            if (_headbar == null)
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
            _headbar.Update();
        }


        public void Show(bool show)
        {
            if (show)
            {
                if (_headbar == null)
                {
                    _headbar = _panel.GetHeadbar(_type, _entity, _offsetY);
                }

                if (_headbar == null)
                    return;

                HP.SetHp(_entity.comHP.HP, _entity.comProperty.maxHP);
            }
            else
            {
                if (_headbar == null)
                {
                    return;
                }

                _panel.RemoveHeadbar(_headbar);
                _headbar = null;
            }
        }
    }
}