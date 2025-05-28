using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cfg;
using LccHotfix;
using UnityEngine;


/// <summary>
/// 普通item奖励显示
/// </summary>
public class RewardItemData : ItemData
{
    public bool IsNew { get; private set; }

    public void SetIsNew(bool isNew)
    {
        this.IsNew = isNew;
    }

    //是否显示数量
    public bool IsShowCount()
    {
        return true;
    }

    //功能开启显示名字
    public bool IsShowName()
    {
        return true;
    }


    public RewardItemData Clone()
    {
        var newData = new RewardItemData();
        newData.InitData(ItemId, Count);

        return newData;
    }
}


/// <summary>
/// 奖励集合
/// </summary>
public class RewardData
{
    private int _id; //奖励id
    private List<RewardItemData> _rewardList = new List<RewardItemData>();
    private List<RewardItemData> _noDisplayRewardList = new List<RewardItemData>();

    public List<RewardItemData> AllRewardList => _rewardList.Union(_noDisplayRewardList).ToList();


    public void InitData(int rewardTemplateId)
    {
        var rewardTemplate = ConfigManager.Instance.Tables.TBRewardTemplate.Get(rewardTemplateId);
        if (rewardTemplate == null)
        {
            return;
        }

        InitData(rewardTemplate);
    }

    public void InitData(RewardTemplate rewardTemplate)
    {
        if (rewardTemplate == null)
        {
            return;
        }

        foreach (var item in rewardTemplate.Rewards)
        {
            AddRewardItem(item);
        }
    }


    private void AddRewardItem(TBReward reward)
    {
        if (reward == null)
            return;
        var itemCfg = ConfigManager.Instance.Tables.TBItem.Get(reward.ItemId);
        if (itemCfg == null)
            return;
        AddItem(reward.ItemId, reward.Count, false);
    }

    public void AddItem(int itemId, int count, bool isNew)
    {
        var itemCfg = ConfigManager.Instance.Tables.TBItem.Get(itemId);
        if (itemCfg == null)
        {
            Debug.LogError($" ItemId = {itemId}, item Cfg is null");
            return;
        }

        if (itemCfg.NotDisplay)
        {
            AddNoDisplayItem(itemId, count, isNew);
            return;
        }

        //todo功能解锁

        foreach (var item in _rewardList)
        {
            if (item == null)
            {
                Debug.LogError("Item is null ??????");
                continue;
            }

            if (item.ItemId == itemId)
            {
                item.AddCount(count);
                return;
            }
        }

        AddNewItem(itemId, count, isNew);
    }



    public void AddRewardItem(RewardItemData rewardItemData)
    {
        if (rewardItemData == null)
            return;
        AddItem(rewardItemData.ItemId, rewardItemData.Count, rewardItemData.IsNew);
    }

    private void AddNewItem(int itemId, int count, bool isNew)
    {
        RewardItemData data = new RewardItemData();
        data.InitData(itemId, count);
        data.SetIsNew(isNew);
        _rewardList.Add(data);
    }

    private void AddNoDisplayItem(int itemId, int count, bool isNew)
    {
        foreach (var item in _noDisplayRewardList)
        {
            if (item.ItemId == itemId)
            {
                item.AddCount(count);
                return;
            }
        }

        AddNewNoDisplayItem(itemId, count, isNew);
    }

    private void AddNewNoDisplayItem(int itemId, int count, bool isNew)
    {
        RewardItemData data = new RewardItemData();
        data.InitData(itemId, count);
        data.SetIsNew(isNew);
        _noDisplayRewardList.Add(data);
    }


    public List<RewardItemData> GetRewardItemDataList()
    {
        return _rewardList;
    }

    public RewardItemData GetRewardItemByIndex(int index)
    {
        if (_rewardList.Count > index)
        {
            return _rewardList[index];
        }

        return null;
    }

    public bool IsHaveReward()
    {
        return _rewardList.Count > 0;
    }

    public void RemoveReward(int index)
    {
        _rewardList.RemoveAt(index);
    }

    public void ClearAllReward()
    {
        _rewardList.Clear();
    }
}