using System.Collections.Generic;

namespace LccModel
{
    public class ItemComponent : Component
    {
        public Combat Combat => GetParent<Combat>();

        public Dictionary<int, ItemAbility> itemDict = new Dictionary<int, ItemAbility>();

        public ItemAbility AttachItem(int itemId)
        {
            ItemConfigObject itemConfigObject = AssetManager.Instance.LoadRes<ItemConfigObject>(CombatContext.Instance.loader, $"Item_{itemId}");
            
            if (itemConfigObject == null)
            {
                return null;
            }

            var item = Combat.AttachAbility<ItemAbility>(itemConfigObject);
            if (!itemDict.ContainsKey(item.itemConfigObject.Id))
            {
                itemDict.Add(item.itemConfigObject.Id, item);
            }
            return item;
        }
        public ItemAbility GetItem(int itemId)
        {
            if (itemDict.ContainsKey(itemId))
            {
                return itemDict[itemId];
            }
            return null;
        }
    }
}