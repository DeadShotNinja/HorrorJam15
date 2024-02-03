using System;
using UnityEngine;

namespace HJ.Runtime
{
    public enum ImageOrientation { Normal, Flipped };
    public enum FlipDirection { Left, Right };
    public enum UsableType { PlayerItem, HealthItem }
    
    [Serializable]
    public sealed class Item
    {
        public string Title;
        public string Description;
        public ushort Width;
        public ushort Height;
        public ImageOrientation Orientation;
        public FlipDirection FlipDirection;
        public Sprite Icon;

        public ObjectReference ItemObject;

        [Serializable]
        public struct ItemSettings 
        {
            public bool IsUsable;
            public bool IsStackable;
            public bool IsExaminable;
            public bool IsCombinable;
            public bool IsDroppable;
            public bool IsDiscardable;
            public bool CanBindShortcut;
            public bool AlwaysShowQuantity;
        }
        public ItemSettings Settings;

        [Serializable]
        public struct ItemUsableSettings
        {
            public UsableType UsableType;
            public int PlayerItemIndex;
            public uint HealthPoints;
        }
        public ItemUsableSettings UsableSettings;

        [Serializable]
        public struct ItemProperties
        {
            public ushort MaxStack;
        }
        public ItemProperties Properties;

        [Serializable]
        public struct ItemCombineSettings
        {
            public string CombineWithID;
            public string ResultCombineID;
            public int PlayerItemIndex;

            [Tooltip("After combining, do not remove the item from inventory.")]
            public bool KeepAfterCombine;
            [Tooltip("After combining, call the combine event if the second inventory item is a player item. The combine event will be called only on the second item.")]
            public bool EventAfterCombine;
            [Tooltip("After combining, select the player item instead of adding the result item to the inventory.")]
            public bool SelectAfterCombine;
        }
        public ItemCombineSettings[] CombineSettings;

        [Serializable]
        public struct Localization
        {
            public string TitleKey;
            public string DescriptionKey;
        }
        public Localization LocalizationSettings;

        /// <summary>
        /// Creates a new instance of a class with the same value as an existing instance.
        /// </summary>
        public Item DeepCopy()
        {
            return new Item()
            {
                Title = Title,
                Description = Description,
                Width = Width,
                Height = Height,
                Orientation = Orientation,
                FlipDirection = FlipDirection,
                Icon = Icon,
                ItemObject = ItemObject,
                Settings = Settings,
                UsableSettings = UsableSettings,
                Properties = Properties,
                CombineSettings = CombineSettings,
                LocalizationSettings = LocalizationSettings
            };
        }
    }
}
