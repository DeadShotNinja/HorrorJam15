using UnityEngine;

namespace HJ.Runtime
{
    public struct ItemData
    {
        public string Guid;
        public Item Item;
        public int Quantity;
        public Orientation Orientation;
        public ItemCustomData CustomData;
        public Vector2Int SlotSpace;
    }
}
