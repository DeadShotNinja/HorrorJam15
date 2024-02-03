using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HJ.Tools;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public abstract class InventoryContainer : MonoBehaviour, ISaveable
    {
        [Header("Setup")]
        [SerializeField] private GString _containerTitle;
        [SerializeField, Range(2, 10)] private ushort _rows = 5;
        [SerializeField, Range(2, 10)] private ushort _columns = 5;

        private Dictionary<string, ContainerItem> _containerItems = new();
        protected Inventory _inventory;

        public GString ContainerTitle => _containerTitle;
        public ushort Rows => _rows;
        public ushort Columns => _columns;
        public Dictionary<string, ContainerItem> ContainerItems => _containerItems;

        public virtual void Awake()
        {
            _inventory = Inventory.Instance;
        }

        private void Start()
        {
            _containerTitle.SubscribeGloc();
        }

        public virtual void Store(InventoryItem inventoryItem, Vector2Int coords)
        {
            string containerGuid = GameTools.GetGuid();
            _containerItems.Add(containerGuid, new ContainerItem()
            {
                ItemGuid = inventoryItem.ItemGuid,
                Item = inventoryItem.Item,
                Quantity = inventoryItem.Quantity,
                Orientation = inventoryItem.Orientation,
                CustomData = inventoryItem.CustomData,
                Coords = coords,
            });

            inventoryItem.ContainerGuid = containerGuid;
        }

        public virtual void Remove(InventoryItem inventoryItem)
        {
            if (_containerItems.ContainsKey(inventoryItem.ContainerGuid))
            {
                _containerItems.Remove(inventoryItem.ContainerGuid);
            }
        }

        public virtual void Move(InventoryItem inventoryItem, FreeSpace freeSpace)
        {
            if (_containerItems.TryGetValue(inventoryItem.ContainerGuid, out ContainerItem item))
            {
                item.Coords = new Vector2Int(freeSpace.X, freeSpace.Y);
                item.Orientation = freeSpace.Orientation;
            }
        }

        public virtual void OnStorageClose() { }

        public bool Contains(string itemGuid) => _containerItems.Values.Any(x => x.ItemGuid == itemGuid);

        public int Count() => _containerItems.Count;

        protected bool CheckSpace(ushort width, ushort height, out FreeSpace slotSpace)
        {
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    if (width == height)
                    {
                        if (CheckSpaceFromPosition(x, y, width, height))
                        {
                            slotSpace = new FreeSpace(x, y, Orientation.Horizontal);
                            return true;
                        }
                    }
                    else
                    {
                        if (CheckSpaceFromPosition(x, y, width, height))
                        {
                            slotSpace = new FreeSpace(x, y, Orientation.Horizontal);
                            return true;
                        }
                        else if (CheckSpaceFromPosition(x, y, height, width))
                        {
                            slotSpace = new FreeSpace(x, y, Orientation.Vertical);
                            return true;
                        }
                    }
                }
            }

            slotSpace = new FreeSpace();
            return false;
        }

        protected bool CheckSpaceFromPosition(int x, int y, int width, int height)
        {
            // check if the item fits within the storage boundary.
            if (x + width > _columns || y + height > _rows)
                return false;

            // check if the item overlaps with any other stored item.
            for (int yy = y; yy < y + height; yy++)
            {
                for (int xx = x; xx < x + width; xx++)
                {
                    foreach (var kvp in _containerItems)
                    {
                        var storedItem = kvp.Value;

                        // check if current xx,yy coordinate is inside any stored item's space.
                        if (xx >= storedItem.Coords.x && xx < storedItem.Coords.x + storedItem.Item.Width &&
                            yy >= storedItem.Coords.y && yy < storedItem.Coords.y + storedItem.Item.Height)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public virtual StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new();

            foreach (var item in _containerItems)
            {
                saveableBuffer.Add(item.Key, new StorableCollection()
                {
                    { "item", item.Value.ItemGuid },
                    { "quantity", item.Value.Quantity },
                    { "orientation", item.Value.Orientation },
                    { "position", item.Value.Coords.ToSaveable() },
                    { "customData", item.Value.CustomData.GetJson() },
                });
            }

            return saveableBuffer;
        }

        public virtual void OnLoad(JToken data)
        {
            JObject savedItems = (JObject)data;
            if (savedItems == null) return;

            foreach (var itemProp in savedItems.Properties())
            {
                JToken token = itemProp.Value;

                string containerGuid = itemProp.Name;
                string itemGuid = token["item"].ToString();
                Item item = _inventory.Items[itemGuid];
                int quantity = (int)token["quantity"];
                Orientation orientation = (Orientation)(int)token["orientation"];
                Vector2Int position = token["position"].ToObject<Vector2Int>();
                ItemCustomData customData = new ItemCustomData()
                {
                    JsonData = token["customData"].ToString()
                };

                _containerItems.Add(containerGuid, new ContainerItem()
                {
                     ItemGuid = itemGuid,
                     Item = item,
                     Quantity = quantity,
                     Orientation = orientation,
                     CustomData = customData,
                     Coords = new Vector2Int(position.x, position.y)
                });
            }
        }
    }
}
