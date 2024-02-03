using System;
using System.Collections.Generic;
using UnityEngine;
using HJ.Runtime;

namespace HJ.Scriptable
{
    [CreateAssetMenu(fileName = "Inventory", menuName = "HJ/Game/Inventory Asset")]
    public class InventoryAsset : ScriptableObject
    {
        [Serializable]
        public struct ReferencedItem
        {
            public string Guid;
            public Item Item;
        }

        public List<ReferencedItem> Items = new List<ReferencedItem>();
    }
}