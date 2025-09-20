using System.Collections.Generic;
using cfg;

namespace LccHotfix
{
    public class ItemSaveData
    {
        public int itemId;
        public int count;

        public void InitData(int itemId, int count)
        {
            this.itemId = itemId;
            this.count = count;
        }
    }

    public class BagSaveData : ISave
    {
        public string TypeName => GetType().FullName;
        public List<ItemSaveData> ItemList { get; set; }

        public void Init()
        {
            ItemList = new List<ItemSaveData>();
        }
    }

    public class BagData : ISaveConverter<BagSaveData>
    {
        public BagSaveData Save { get; set; }
        public List<ItemData> ItemList { get; set; }

        /// <summary>
        /// 保存存档时调用 把运行时数据转换过去
        /// </summary>
        /// <returns></returns>
        public ISave Flush()
        {
            Save.ItemList = new List<ItemSaveData>();
            foreach (var item in ItemList)
            {
                ItemSaveData data = new ItemSaveData();
                data.InitData(item.ItemId, item.Count);
                Save.ItemList.Add(data);
            }

            return Save;
        }

        /// <summary>
        /// 创建运行时数据时调用 把存档的数据转换过来
        /// </summary>
        public void Init()
        {
            ItemList = new List<ItemData>();
            foreach (var item in Save.ItemList)
            {
                ItemData data = new ItemData();
                data.InitData(item.itemId, item.count);
                ItemList.Add(data);
            }
        }

        /// <summary>
        /// 增加玩家道具
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="count"></param>
        public void AddItem(int itemId, int count)
        {
            foreach (var item in ItemList)
            {
                if (item.ItemId == itemId)
                {
                    item.AddCount(count);
                    return;
                }
            }

            ItemData data = new ItemData();
            data.InitData(itemId, count);
            ItemList.Add(data);
        }

        /// <summary>
        /// 获取玩家道具数量
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public int GetItemCount(int itemId)
        {
            foreach (var item in ItemList)
            {
                if (item.ItemId == itemId)
                {
                    return item.Count;
                }
            }

            return 0;
        }

        /// <summary>
        /// 移除玩家道具数量
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="count"></param>
        public bool RemoveItemCount(int itemId, int count)
        {
            var haveCount = GetItemCount(itemId);
            if (haveCount >= count)
            {
                var newCount = haveCount - count;
                GetItem(itemId).SetCount(newCount);
                if (GetItemCount(itemId) <= 0)
                {
                    ItemList.RemoveAt(itemId);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取玩家道具
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ItemData GetItem(int itemId)
        {
            foreach (var item in ItemList)
            {
                if (item.ItemId == itemId)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取玩家道具列表
        /// </summary>
        /// <param name="smallType"></param>
        /// <returns></returns>
        public List<ItemData> GetItemList(ItemSmallType smallType)
        {
            var list = new List<ItemData>();
            foreach (var item in ItemList)
            {
                if (item.GetItemSmallType() == smallType)
                {
                    list.Add(item);
                }
            }

            return list;
        }
    }

    [Model]
    public class ModBag : ModelTemplate, ISavePipeline
    {
        public BagData BagData { get; private set; }

        public void InitData(GameSaveData gameSaveData)
        {
            BagData = gameSaveData.GetSaveConverterData<BagData, BagSaveData>();
        }

        public void AddItem(int itemId, int count)
        {
            BagData.AddItem(itemId, count);
        }

        public List<ItemData> GetItemList(ItemSmallType smallType)
        {
            return BagData.GetItemList(smallType);
        }
    }
}