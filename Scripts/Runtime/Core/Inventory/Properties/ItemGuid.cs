using System;

namespace HJ.Runtime
{
    [Serializable]
    public class ItemGuid
    {
        public string GUID;

        /// <summary>
        /// Returns the quantity of the inventory item.
        /// </summary>
        public int Quantity
        {
            get => !string.IsNullOrEmpty(GUID) ? Inventory.Instance.GetItemQuantity(GUID) : 0;
        }

        public static implicit operator string(ItemGuid item)
        {
            return item.GUID;
        }

        public bool HasItem
        {
            get => !string.IsNullOrEmpty(GUID)
                   && Inventory.HasReference
                   && Inventory.Instance.ContainsItem(GUID);
        }

        /// <summary>
        /// Get Item from Inventory (Runtime).
        /// </summary>
        public Item GetItem()
        {
            if (!string.IsNullOrEmpty(GUID) && Inventory.HasReference)
            {
                if (Inventory.Instance.Items.TryGetValue(GUID, out Item item))
                    return item;
            }

            return null;
        }

        /// <summary>
        /// Get Item from Inventory Asset.
        /// </summary>
        public Item GetItemRaw()
        {
            if (!string.IsNullOrEmpty(GUID) && Inventory.HasReference && Inventory.Instance.InventoryAsset != null)
            {
                foreach (var item in Inventory.Instance.InventoryAsset.Items)
                {
                    if (item.Guid == GUID)
                        return item.Item;
                }
            }

            return null;
        }
    }
}