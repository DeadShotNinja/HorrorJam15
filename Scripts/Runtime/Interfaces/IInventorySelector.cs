namespace HJ.Runtime
{
    public interface IInventorySelector
    {
        void OnInventoryItemSelect(Inventory inventory, InventoryItem selectedItem);
    }
}