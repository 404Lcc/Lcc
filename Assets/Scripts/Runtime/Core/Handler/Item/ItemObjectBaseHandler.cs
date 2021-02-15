using System;
using UnityEngine;

namespace LccModel
{
    public class ItemObjectBaseHandler : AObjectBaseHandler
    {
        public ItemObjectBaseHandler(bool isKeep, bool isAssetBundle, params string[] types) : base(isKeep, isAssetBundle, types)
        {
            this.isKeep = isKeep;
            this.isAssetBundle = isAssetBundle;
            this.types = types;
        }
        public override GameObject CreateGameObject(string name, Transform parent)
        {
            GameObject gameObject = base.CreateGameObject(name, parent);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            return gameObject;
        }
        public Item CreateItem(ItemType type, object data, Transform parent)
        {
            Item item = new Item();
            GameObject gameObject = CreateGameObject(type.ToItemString(), parent);
            if (gameObject == null) return null;
            item.Type = type;
            Type classType = Manager.Instance.GetType(type.ToItemString());
            if (classType != null)
            {
                item.AObjectBase = LccViewFactory.CreateView(classType, gameObject, data);
            }
            return item;
        }
    }
}