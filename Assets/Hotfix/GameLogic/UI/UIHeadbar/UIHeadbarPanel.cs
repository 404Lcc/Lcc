using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public enum HeadbarType
    {
        NormalHP, //普通血条
    }

    public class UIHeadbarPanel : UILogicBase, ICoroutine
    {
        public List<NormalHPBase> normalList = new List<NormalHPBase>();

        public int maxNormalHPCount = 20;

        public GameObject normalTemplate;

        public override void OnStart()
        {
            base.OnStart();

            ClientTools.SetCachedItemHide(normalList);

            for (int i = 0; i < maxNormalHPCount; i++)
            {
                ClientTools.GetOneCached(normalList, transform.gameObject, normalTemplate);
            }

            ClientTools.SetCachedItemHide(normalList);
        }

        public UIHeadbarTemplate GetHeadbar(HeadbarType type, LogicEntity entity, float offsetY)
        {
            switch (type)
            {
                case HeadbarType.NormalHP:
                    var item = ClientTools.GetOneCached(normalList, transform.gameObject, normalTemplate);
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

        public override void OnPause()
        {
            base.OnPause();


            ClientTools.SetCachedItemHide(normalList);
        }
    }
}