using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class ItemManager : Singleton<ItemManager>
    {
        public Hashtable items = new Hashtable();
        public ItemObjectBaseHandler handler;
        public void InitManager(ItemObjectBaseHandler handler)
        {
            this.handler = handler;
        }
        private bool ItemExist(ItemType type)
        {
            if (items.ContainsKey(type))
            {
                return true;
            }
            return false;
        }
        public ItemData CreateItem(ItemType type, object data, Transform parent)
        {
            ItemData itemData = handler.CreateItem(type, data, parent);
            if (ItemExist(type))
            {
                ((List<ItemData>)items[type]).Add(itemData);
                return itemData;
            }
            else
            {
                List<ItemData> itemDataList = new List<ItemData>();
                itemDataList.Add(itemData);
                items.Add(type, itemDataList);
                return itemData;
            }
        }
        public void ClearItems(ItemType type)
        {
            if (ItemExist(type))
            {
                ItemData[] itemDatas = GetItems(type);
                foreach (ItemData item in itemDatas)
                {
                    item.gameObject.SafeDestroy();
                }
                items.Remove(type);
            }
        }
        public void ClearAllItems()
        {
            foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
            {
                ClearItems(item);
            }
        }
        public ItemData[] GetItems(ItemType type)
        {
            if (ItemExist(type))
            {
                ItemData[] itemDatas = (ItemData[])items[type];
                return itemDatas;
            }
            return null;
        }
        public T[] GetItems<T>(ItemType type) where T : ObjectBase
        {
            if (ItemExist(type))
            {
                ItemData[] itemDatas = (ItemData[])items[type];
                List<T> objectBaseList = new List<T>();
                foreach (ItemData item in itemDatas)
                {
                    objectBaseList.Add((T)item.objectBase);
                }
                return objectBaseList.ToArray();
            }
            return null;
        }
    }
}