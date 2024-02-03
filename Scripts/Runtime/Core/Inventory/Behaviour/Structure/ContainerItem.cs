using UnityEngine;

namespace HJ.Runtime
{
    public sealed class ContainerItem
    {
        public string ItemGuid;
        public Item Item;
        public int Quantity;
        public Orientation Orientation;
        public ItemCustomData CustomData;
        public Vector2Int Coords;
    }
}
