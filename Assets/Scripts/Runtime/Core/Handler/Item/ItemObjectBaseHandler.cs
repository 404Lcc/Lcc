using System;
using System.Reflection;
using UnityEngine;

namespace Model
{
    public class ItemObjectBaseHandler : ObjectBaseHandler
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
        public ItemData CreateItem(ItemType type, object data, Transform parent)
        {
            ItemData itemData = new ItemData();
            itemData.gameObject = CreateGameObject(type.ToItemString(), parent);
            if (itemData.gameObject == null) return null;
            itemData.type = type;
            Assembly assembly = type.GetType().Assembly;
            Type classType = assembly.GetType(type.GetType().Namespace + "." + type.ToItemString());
            if (classType != null)
            {
                itemData.objectBase = LccViewFactory.CreateView(classType, itemData.gameObject, data);
            }
            return itemData;
        }
    }
}