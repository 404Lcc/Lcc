using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class ItemManager : Singleton<ItemManager>
    {
        public Hashtable items = new Hashtable();
        public ItemObjectBaseHandler handler;
        public void InitManager(ItemObjectBaseHandler handler)
        {
            this.handler = handler;
        }
        public bool ItemExist(ItemType type)
        {
            if (items.ContainsKey(type))
            {
                return true;
            }
            return false;
        }
        public Item CreateItem(ItemType type, Transform parent, params object[] datas)
        {
            Item item = handler.CreateItem(type, parent, datas);
            if (ItemExist(type))
            {
                ((List<Item>)items[type]).Add(item);
                return item;
            }
            else
            {
                List<Item> itemList = new List<Item>();
                itemList.Add(item);
                items.Add(type, itemList);
                return item;
            }
        }
        public void ClearItems(ItemType type)
        {
            if (ItemExist(type))
            {
                Item[] items = GetItems(type);
                foreach (Item item in items)
                {
                    item.AObjectBase.SafeDestroy();
                }
                this.items.Remove(type);
            }
        }
        public void ClearAllItems()
        {
            foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
            {
                ClearItems(item);
            }
        }
        public Item[] GetItems(ItemType type)
        {
            if (ItemExist(type))
            {
                Item[] items = (Item[])this.items[type];
                return items;
            }
            return null;
        }
        public T[] GetItems<T>(ItemType type) where T : AObjectBase
        {
            if (ItemExist(type))
            {
                Item[] items = (Item[])this.items[type];
                List<T> aObjectBaseList = new List<T>();
                foreach (Item item in items)
                {
                    aObjectBaseList.Add((T)item.AObjectBase);
                }
                return aObjectBaseList.ToArray();
            }
            return null;
        }
    }
}