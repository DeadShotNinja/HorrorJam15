using System;

namespace HJ.Runtime
{
    [Serializable]
    public struct StartingItem
    {
        public string GUID;
        public string Title;
        public ushort Quantity;
        public ItemCustomData Data;
    }
}
