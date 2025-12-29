using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public enum HeadbarType
    {
        NormalHP, //普通血条
    }

    public class UIHeadbarPanel : UIElementBase, ICoroutine
    {
        public int maxCount = 20;

        public List<NormalHPBase> normalList = new List<NormalHPBase>();

        public GameObject normalTemplate;

        public override void OnConstruct()
        {
            base.OnConstruct();
            
            IsFullScreen = true;
            EscapeType = EscapeType.Skip;
        }

        public override void OnShow(object[] paramsList)
        {
            base.OnShow(paramsList);

            ClientTools.SetCachedItemHide(normalList);

            for (int i = 0; i < maxCount; i++)
            {
                ClientTools.GetOneCached(normalList, RectTransform.gameObject, normalTemplate);
            }

            ClientTools.SetCachedItemHide(normalList);

        }



        public UIHeadbarTemplate GetHeadbar(HeadbarType type, LogicEntity entity, float offsetY)
        {
            switch (type)
            {
                case HeadbarType.NormalHP:
                    var item = ClientTools.GetOneCached(normalList, RectTransform.gameObject, normalTemplate);
                    item.Init(entity, offsetY);
                    return item;
            }

            return null;
        }

        public void RemoveHeadbar(ItemBase hud)
        {
            hud.GameObject.SetActive(false);
            hud.Hide();
        }

        public override object OnHide()
        {
            return base.OnHide();

            ClientTools.SetCachedItemHide(normalList);
        }


    }
}