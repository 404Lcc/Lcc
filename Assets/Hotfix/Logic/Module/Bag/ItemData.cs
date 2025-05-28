using System;
using System.Collections;
using System.Collections.Generic;
using cfg;
using LccHotfix;
using UnityEngine;

[Serializable]
public class ItemData
{
    private Item _item;

    public int ItemId { get; private set; }
    public int Count { get; private set; }

        


    public void InitData(int itemId, int count)
    {
        this.ItemId = itemId;
        this.Count = count;
        _item = ConfigManager.Instance.Tables.TBItem.Get(itemId);
        if (_item == null)
        {
            Debug.LogError("item表错了 id" + itemId);
            return;
        }
    }
    
    
    public void SetCount(int count)
    {
        this.Count = count;
    }


    public void AddCount(int count)
    {
        this.Count = this.Count + count;
    }

    public void RemoveCount(int count)
    {
        if (this.Count < count)
            return;
        this.Count = this.Count - count;
    }

    public string GetName()
    {
        return GameUtility.GetLanguageText(_item.Name);
    }
    
    public string GetTips()
    {
        return GameUtility.GetLanguageText(_item.Tips);
    }
    
    public QualityType GetQuality()
    {
        return _item.Quality;
    }
    
    public int GetItemFrameIcon()
    {
        return GameUtility.GetItemFrameIcon(GetQuality());
    }
    
    public ItemType GetItemType()
    {
        return _item.Type;
    }

    public ItemSmallType GetItemSmallType()
    {
        return _item.SmallType;
    }

    public int GetSmallId()
    {
        return _item.SmallSubId;
    }
    
    //是否不支持显示
    public bool GetNotDisplay()
    {
        return _item.NotDisplay;
    }
}
