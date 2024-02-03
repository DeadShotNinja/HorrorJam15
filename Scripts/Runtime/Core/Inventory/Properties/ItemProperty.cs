using System;

namespace HJ.Runtime 
{
    [Serializable]
    public class ItemProperty
    {
        public string GUID;

        public static implicit operator string(ItemProperty item)
        {
            return item.GUID;
        }

        public bool HasItem
        {
            get => !string.IsNullOrEmpty(GUID)
                   && Inventory.HasReference
                   && Inventory.Instance.Items.ContainsKey(GUID);
        }

        /// <summary>
        /// Get Item from Inventory (Runtime).
        /// </summary>
        public Item GetItem()
        {
            if (Inventory.HasReference)
            {
                if(Inventory.Instance.Items.TryGetValue(GUID, out Item item))
                    return item;
            }

            return new Item();
        }

        /// <summary>
        /// Get Item from Inventory Asset.
        /// </summary>
        public Item GetItemRaw()
        {
            if (Inventory.HasReference && Inventory.Instance.InventoryAsset != null)
            {
                foreach (var item in Inventory.Instance.InventoryAsset.Items)
                {
                    if (item.Guid == GUID) 
                        return item.Item;
                }
            }

            return new Item();
        }
    }
}