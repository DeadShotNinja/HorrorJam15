using UnityEngine;

namespace HJ.Runtime
{
    public struct ItemCreationData
    {
        public string ItemGuid;
        public ushort Quantity;
        public Orientation Orientation;
        public Vector2Int Coords;
        public ItemCustomData CustomData;
        public Transform Parent;
        public InventorySlot[,] SlotsSpace;
    }
}
