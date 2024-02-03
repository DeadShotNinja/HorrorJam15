using System.Collections.Generic;
using System.Reactive.Subjects;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using HJ.Scriptable;
using HJ.Tools;
using TMPro;

namespace HJ.Runtime
{
    public enum Orientation { Horizontal, Vertical };
    public enum InventorySound { ItemSelect, ItemMove, ItemPut, ItemError }

    public partial class Inventory : Singleton<Inventory>, ISaveableCustom
    {
        public enum SlotType { Restricted, Inventory, Container }
        
        [Header("Setup")]
        [SerializeField] private InventoryAsset _inventoryAsset;

        // references
        [SerializeField] private Transform _inventoryContainers;
        [SerializeField] private GridLayoutGroup _slotsLayoutGrid;
        [SerializeField] private Transform _itemsTransform;

        // control contexts
        [SerializeField] private ControlsContext[] _controlsContexts;

        // settings
        [SerializeField] private Settings _settings;
        [SerializeField] private SlotSettings _slotSettings;
        [SerializeField] private ContainerSettings _containerSettings;
        [SerializeField] private ItemInfo _itemInfo;
        [SerializeField] private ShortcutSettings _shortcutSettings;
        [SerializeField] private PromptSettings _promptSettings;
        [SerializeField] private ContextMenu _contextMenu;

        // features
        [SerializeField] private List<StartingItem> _startingItems = new();
        [SerializeField] private ExpandableSlots _expandableSlots;

        // inventory
        private SlotType[,] slotArray;
        public InventorySlot[,] slots;
        public Dictionary<string, Item> Items { get; private set; }
        public Dictionary<InventoryItem, InventorySlot[]> CarryingItems { get; private set; }

        // container
        [SerializeField] private InventoryContainer _currentContainer;
        public InventorySlot[,] containerSlots;
        public Dictionary<InventoryItem, InventorySlot[]> ContainerItems { get; private set; }

        private int _expandedSlots;
        private float _nextSoundDelay;
        private bool _contextShown;

        private IInventorySelector _inventorySelector;
        private PlayerPresenceManager _playerPresence;
        private GameManager _gameManager;

        public List<StartingItem> StartingItems => _startingItems;
        public InventoryAsset InventoryAsset => _inventoryAsset;
        public Transform InventoryContainers => _inventoryContainers;
        public Settings Settings => _settings;
        public SlotSettings SlotSettings => _slotSettings;
        public ControlsContext[] ControlsContexts => _controlsContexts;
        
        public GameObject Player => _playerPresence.Player;
        public PlayerItemsManager PlayerItems => _playerPresence.PlayerManager.PlayerItems;
        public PlayerHealth PlayerHealth => _playerPresence.PlayerManager.PlayerHealth;
        public bool ContainerOpened => _currentContainer != null;

        public Subject<InventoryItem> OnItemAdded = new(); 
        public Subject<string> OnItemRemoved = new();

        public Vector2Int SlotXY
        {
            get
            {
                int x = _settings.Columns;
                int y = _settings.Rows;
                return new(x, y);
            }
        }

        public Vector2Int MaxSlotXY
        {
            get
            {
                int x = slotArray.GetLength(1);
                int y = slotArray.GetLength(0);
                return new(x, y);
            }
        }

        public InventorySlot this[int y, int x]
        {
            get
            {
                try
                {
                    if (ContainerOpened)
                    {
                        if (IsContainerCoords(x, y))
                        {
                            return containerSlots[y, x];
                        }
                        else
                        {
                            return slots[y, x - _currentContainer.Columns];
                        }
                    }

                    return slots[y, x];
                }
                catch
                {
                    return null;
                }
            }
        }

        private void Awake()
        {
            slotArray = new SlotType[_settings.Rows, _settings.Columns];
            slots = new InventorySlot[_settings.Rows, _settings.Columns];
            Items = new Dictionary<string, Item>();
            CarryingItems = new Dictionary<InventoryItem, InventorySlot[]>();
            ContainerItems = new Dictionary<InventoryItem, InventorySlot[]>();

            // slot grid setting
            _slotsLayoutGrid.cellSize = new Vector2(_settings.CellSize, _settings.CellSize);
            _slotsLayoutGrid.spacing = new Vector2(_settings.Spacing, _settings.Spacing);

            // slot instantiation
            for (int y = 0; y < _settings.Rows; y++)
            {
                for (int x = 0; x < _settings.Columns; x++)
                {
                    GameObject slot = Instantiate(_slotSettings.SlotPrefab, _slotsLayoutGrid.transform);
                    slot.name = $"Slot [{y},{x}]";

                    RectTransform rect = slot.GetComponent<RectTransform>();
                    rect.localScale = Vector3.one;

                    InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();
                    slots[y, x] = inventorySlot;
                    slotArray[y, x] = SlotType.Inventory;

                    if (_expandableSlots.Enabled && y >= _settings.Rows - _expandableSlots.ExpandableRows)
                    {
                        inventorySlot.Frame.sprite = _slotSettings.RestrictedSlotFrame;
                        inventorySlot.CanvasGroup.alpha = 0.3f;
                        slotArray[y, x] = SlotType.Restricted;
                    }
                }
            }

            if (!_inventoryAsset) throw new NullReferenceException("Inventory asset is not set!");

            // item caching
            foreach (var item in _inventoryAsset.Items)
            {
                Item itemClone = item.Item.DeepCopy();
                
#if HJ_LOCALIZATION
                // item title
                itemClone.LocalizationSettings.TitleKey.SubscribeGloc(text =>
                {
                    if(!string.IsNullOrEmpty(text))
                        itemClone.Title = text;
                });

                // item description
                itemClone.LocalizationSettings.DescriptionKey.SubscribeGloc(text =>
                {
                    if (!string.IsNullOrEmpty(text))
                        itemClone.Description = text;
                });
#endif
                
                Items.Add(item.Guid, itemClone);
            }

            // initialize other stuff
            _playerPresence = GetComponent<PlayerPresenceManager>();
            _gameManager = GetComponent<GameManager>();
            //_inventorySounds = GetComponent<AudioSource>();
            _contextMenu.ContextMenuGO.SetActive(false);
            _contextMenu.BlockerPanel.SetActive(false);
            _itemInfo.InfoPanel.SetActive(false);

            // initialize context handler
            InitializeContextHandler();
        }

        private void Start()
        {
            if (!SaveGameManager.IsGameJustLoaded)
            {
                foreach (var item in _startingItems)
                {
                    AddItem(item.GUID, item.Quantity, item.Data);
                }
            }
            // TODO: Testing fix... Will prob add items on load to new scenes...
            else
            {
                foreach (var item in _startingItems)
                {
                    AddItem(item.GUID, item.Quantity, item.Data);
                }
            }

            foreach (var control in _controlsContexts)
            {
                control.SubscribeGloc();
            }

            _promptSettings.ShortcutPrompt.SubscribeGlocMany();
            _promptSettings.CombinePrompt.SubscribeGloc();
        }

        private void Update()
        {
            _nextSoundDelay = _nextSoundDelay > 0
                ? _nextSoundDelay -= Time.deltaTime : 0;

            ContextUpdate();
        }

        /// <summary>
        /// Add item to the free inventory space.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <param name="quantity">Quantity of the item to be added.</param>
        /// <param name="customData">Custom data of specified item.</param>
        /// <returns>Status whether the item has been added to the inventory.</returns>
        public bool AddItem(string guid, ushort quantity, ItemCustomData customData)
        {
            if (Items.ContainsKey(guid))
            {
                Item item = Items[guid];
                ushort maxStack = item.Properties.MaxStack;
                InventoryItem inventoryItem = null;

                if (ContainsItem(guid, out OccupyData itemData) && item.Settings.IsStackable)
                {
                    inventoryItem = itemData.InventoryItem;
                    int currQuantity = itemData.InventoryItem.Quantity;
                    int remainingQ = 0;

                    if (maxStack == 0)
                    {
                        currQuantity += quantity;
                        itemData.InventoryItem.SetQuantity(currQuantity);
                    }
                    else if (currQuantity <= maxStack)
                    {
                        int newQ = currQuantity + quantity;
                        int q = Mathf.Min(maxStack, newQ);
                        remainingQ = newQ - q;
                        itemData.InventoryItem.SetQuantity(q);
                    }

                    if (remainingQ > 0)
                    {
                        int iterations = (int)Math.Ceiling((float)remainingQ / maxStack);
                        for (int i = 0; i < iterations; i++)
                        {
                            int q = Mathf.Min(maxStack, remainingQ);
                            inventoryItem = CreateItem(guid, (ushort)q, customData);
                            remainingQ -= maxStack;
                        }
                    }
                }
                else
                {
                    if (quantity < maxStack || maxStack == 0)
                    {
                        inventoryItem = CreateItem(guid, quantity, customData);
                    }
                    else
                    {
                        int iterations = (int)Math.Ceiling((float)quantity / maxStack);
                        for (int i = 0; i < iterations; i++)
                        {
                            int q = Mathf.Min(maxStack, quantity);
                            inventoryItem = CreateItem(guid, (ushort)q, customData);
                            quantity -= maxStack;
                        }
                    }
                }

                if (inventoryItem != null)
                {
                    OnItemAdded.OnNext(inventoryItem);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove item from inventory completly.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <returns>Status whether the item has been removed from the inventory.</returns>
        public bool RemoveItem(string guid)
        {
            if (ContainsItem(guid, out OccupyData itemData))
            {
                CarryingItems.Remove(itemData.InventoryItem);
                Destroy(itemData.InventoryItem.gameObject);
                foreach (var slot in itemData.OccupiedSlots)
                {
                    slot.ItemInSlot = null;
                }

                OnItemRemoved.OnNext(guid);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove item quantity from inventory.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <param name="quantity">Quantity of the item to be removed.</param>
        /// <returns>Quantity of the item in the inevntory.</returns>
        public int RemoveItem(string guid, ushort quantity)
        {
            if (ContainsItem(guid, out OccupyData itemData))
            {
                if ((itemData.InventoryItem.Quantity - quantity) >= 1)
                {
                    int q = itemData.InventoryItem.Quantity - quantity;
                    itemData.InventoryItem.SetQuantity(q);
                    OnItemRemoved.OnNext(guid);
                    return q;
                }
                else
                {
                    CarryingItems.Remove(itemData.InventoryItem);
                    Destroy(itemData.InventoryItem.gameObject);
                    foreach (var slot in itemData.OccupiedSlots)
                    {
                        slot.ItemInSlot = null;
                    }

                    OnItemRemoved.OnNext(guid);
                }
            }

            return 0;
        }

        /// <summary>
        /// Remove item from inventory or container completly.
        /// </summary>
        public void RemoveItem(InventoryItem inventoryItem)
        {
            if (!inventoryItem.IsContainerItem && CarryingItems.ContainsKey(inventoryItem))
            {
                InventorySlot[] occupiedSlots = CarryingItems[inventoryItem];
                CarryingItems.Remove(inventoryItem);
                foreach (var slot in occupiedSlots)
                {
                    slot.ItemInSlot = null;
                }
            }
            else if (ContainerItems.ContainsKey(inventoryItem))
            {
                InventorySlot[] occupiedSlots = ContainerItems[inventoryItem];
                ContainerItems.Remove(inventoryItem);
                foreach (var slot in occupiedSlots)
                {
                    slot.ItemInSlot = null;
                }

                if (_currentContainer != null)
                    _currentContainer.Remove(inventoryItem);
            }

            OnItemRemoved.OnNext(inventoryItem.ItemGuid);
            Destroy(inventoryItem.gameObject);
        }

        /// <summary>
        /// Remove item quantity from inventory.
        /// </summary>
        public int RemoveItem(InventoryItem inventoryItem, ushort quantity)
        {
            if (CarryingItems.ContainsKey(inventoryItem))
            {
                if ((inventoryItem.Quantity - quantity) >= 1)
                {
                    int q = inventoryItem.Quantity - quantity;
                    inventoryItem.SetQuantity(q);
                    OnItemRemoved.OnNext(inventoryItem.ItemGuid);
                    return q;
                }
                else
                {
                    InventorySlot[] occupiedSlots = CarryingItems[inventoryItem];
                    CarryingItems.Remove(inventoryItem);
                    foreach (var slot in occupiedSlots)
                    {
                        slot.ItemInSlot = null;
                    }

                    OnItemRemoved.OnNext(inventoryItem.ItemGuid);
                    Destroy(inventoryItem.gameObject);
                }
            }

            return 0;
        }

        /// <summary>
        /// Get <see cref="InventoryItem"/> reference from Inventory.
        /// </summary>
        public InventoryItem GetInventoryItem(string guid)
        {
            if(ContainsItem(guid, out OccupyData occupyData))
                return occupyData.InventoryItem;

            return null;
        }

        /// <summary>
        /// Get <see cref="OccupyData"/> reference from Inventory.
        /// </summary>
        public OccupyData GetOccupyData(string guid)
        {
            if (ContainsItem(guid, out OccupyData occupyData))
                return occupyData;

            return default;
        }

        /// <summary>
        /// Get the quantity of the item in the inevntory.
        /// </summary>
        public int GetItemQuantity(string guid)
        {
            if (ContainsItem(guid, out OccupyData occupyData))
                return occupyData.InventoryItem.Quantity;

            return 0;
        }

        /// <summary>
        /// Set the quantity of the item in the inevntory.
        /// </summary>
        public void SetItemQuantity(string guid, ushort quantity, bool removeWhenZero = true)
        {
            if (ContainsItem(guid, out OccupyData occupyData))
            {
                if (quantity >= 1 || !removeWhenZero)
                {
                    occupyData.InventoryItem.SetQuantity(quantity);
                }
                else if(removeWhenZero)
                {
                    CarryingItems.Remove(occupyData.InventoryItem);
                    Destroy(occupyData.InventoryItem.gameObject);
                    foreach (var slot in occupyData.OccupiedSlots)
                    {
                        slot.ItemInSlot = null;
                    }
                }
            }
        }

        /// <summary>
        /// Expand the inventory slots that are expandable.
        /// </summary>
        /// <param name="rows">Rows to be expanded.</param>
        public void ExpandInventory(int expandSlots, bool expandRows)
        {
            if (_expandableSlots.Enabled)
            {
                int expandableY = SlotXY.y - _expandableSlots.ExpandableRows;
                int expandable = expandRows ? expandSlots * SlotXY.x : expandSlots;
                int toExpand = expandable;

                for (int y = expandableY; y < SlotXY.y; y++)
                {
                    for (int x = 0; x < SlotXY.x; x++)
                    {
                        if (toExpand == 0) 
                            break;

                        if(slotArray[y, x] == SlotType.Restricted)
                        {
                            InventorySlot slot = slots[y, x];
                            slot.Frame.sprite = _slotSettings.NormalSlotFrame;
                            slot.CanvasGroup.alpha = 1f;

                            slotArray[y, x] = SlotType.Inventory;
                            toExpand--;
                        }
                    }
                }

                _expandedSlots += expandable;
            }
        }

        /// <summary>
        /// Check if there is a free space from the desired position.
        /// </summary>
        /// <param name="x">Slot X position.</param>
        /// <param name="y">Slot Y position.</param>
        /// <returns>Status whether there is free space in desired position.</returns>
        public bool CheckSpaceFromPosition(int x, int y, int width, int height, InventoryItem item = null)
        {
            for (int yy = y; yy < y + height; yy++)
            {
                for (int xx = x; xx < x + width; xx++)
                {
                    if (yy < MaxSlotXY.y && xx < MaxSlotXY.x)
                    {
                        if (slotArray[yy, xx] == SlotType.Restricted)
                            return false;

                        InventorySlot slot = this[yy, xx];
                        if (slot == null) return false;

                        if (slot.ItemInSlot != null)
                        {
                            if (item != null && slot.ItemInSlot == item) 
                                continue;
                            return false;
                        }
                    }
                    else return false;
                }
            }

            // check if width of the item has not overflowed from the container into the inventory
            if (_currentContainer != null)
            {
                return (x <= _currentContainer.Columns && x + width <= _currentContainer.Columns) ||
                    (x >= _currentContainer.Columns && x + width >= _currentContainer.Columns);
            }

            return true;
        }

        /// <summary>
        /// Move item to the desired position.
        /// </summary>
        public void MoveItem(Vector2Int lastCoords, Vector2Int newCoords, InventoryItem inventoryItem)
        {
            // check if the last and new coordinates are in the inventory or container space
            bool lastContainerSpace = false, newContainerSpace = false;
            if (ContainerOpened)
            {
                lastContainerSpace = IsContainerCoords(lastCoords.x, lastCoords.y);
                newContainerSpace = IsContainerCoords(newCoords.x, newCoords.y);
            }

            if (!lastContainerSpace)
            {
                // unoccupy slots from inventory space
                if (CarryingItems.TryGetValue(inventoryItem, out var inventorySlots))
                {
                    foreach (var slot in inventorySlots)
                    {
                        slot.ItemInSlot = null;
                    }
                }
            }
            else
            {
                // unoccupy slots from container space
                if (ContainerItems.TryGetValue(inventoryItem, out var containerSlots))
                {
                    foreach (var slot in containerSlots)
                    {
                        slot.ItemInSlot = null;
                    }
                }
            }

            // if the new coordinates are in inventory space
            if (!newContainerSpace)
            {
                if (lastContainerSpace)
                {
                    // remove item from the container space
                    _currentContainer.Remove(inventoryItem);
                    ContainerItems.Remove(inventoryItem);
                    inventoryItem.ContainerGuid = string.Empty;
                    inventoryItem.IsContainerItem = false;

                    // add item to inventory space
                    CarryingItems.Add(inventoryItem, null);
                }

                // set item parent to the inventory panel transform
                inventoryItem.transform.SetParent(_itemsTransform);
            }
            // if the new coordinates are in container space
            else
            {
                if (!lastContainerSpace)
                {
                    // remove item from the inventory space
                    CarryingItems.Remove(inventoryItem);
                    RemoveShortcut(inventoryItem);

                    // add item to container space
                    _currentContainer.Store(inventoryItem, newCoords);
                    ContainerItems.Add(inventoryItem, null);
                    inventoryItem.IsContainerItem = true;
                }
                else
                {
                    // move a container item to new coordinates
                    _currentContainer.Move(inventoryItem, new FreeSpace()
                    {
                        X = newCoords.x,
                        Y = newCoords.y,
                        Orientation = inventoryItem.Orientation
                    });
                }

                // set item parent to the container panel transform
                inventoryItem.transform.SetParent(_containerSettings.ContainerItems);

                // if the item is equipped, unequip the current item
                if (inventoryItem.Item.UsableSettings.UsableType == UsableType.PlayerItem)
                {
                    int playerItemIndex = inventoryItem.Item.UsableSettings.PlayerItemIndex;
                    if (playerItemIndex >= 0 && PlayerItems.CurrentItemIndex == playerItemIndex)
                        PlayerItems.DeselectCurrent();
                }
            }

            // occupy new slots
            OccupySlots(newContainerSpace, newCoords, inventoryItem);
        }

        /// <summary>
        /// Occupy slots with the item in the new coordinates.
        /// </summary>
        private void OccupySlots(bool isContainerSpace, Vector2Int newCoords, InventoryItem inventoryItem)
        {
            Item item = inventoryItem.Item;
            int maxY = item.Height, maxX = item.Width;

            // rotate the item if the orientation is vertical
            if (inventoryItem.Orientation == Orientation.Vertical)
            {
                maxY = item.Width;
                maxX = item.Height;
            }

            InventorySlot[] slotsToOccupy = new InventorySlot[maxY * maxX];

            int slotIndex = 0;
            for (int yy = newCoords.y; yy < newCoords.y + maxY; yy++)
            {
                for (int xx = newCoords.x; xx < newCoords.x + maxX; xx++)
                {
                    InventorySlot slot = this[yy, xx];
                    slot.ItemInSlot = inventoryItem;
                    slotsToOccupy[slotIndex++] = slot;
                }
            }

            if (!isContainerSpace)
            {
                CarryingItems[inventoryItem] = slotsToOccupy;
            }
            else
            {
                ContainerItems[inventoryItem] = slotsToOccupy;
            }
        }

        /// <summary>
        /// Open the inventory container.
        /// </summary>
        /// <param name="container">Container to be opened.</param>
        public void OpenContainer(InventoryContainer container)
        {
            // expand inventory slots with container slots
            SetInventorySlots(container, true);

            // initialize container slots
            containerSlots = new InventorySlot[container.Rows, container.Columns];
            _currentContainer = container;

            // slot grid setting
            _containerSettings.ContainerSlots.cellSize = new Vector2(_settings.CellSize, _settings.CellSize);
            _containerSettings.ContainerSlots.spacing = new Vector2(_settings.Spacing, _settings.Spacing);

            // set the container panel size to fit the number of container columns
            Vector2 grdLayoutSize = _containerSettings.ContainerObject.sizeDelta;
            grdLayoutSize.x = _settings.CellSize * container.Columns + _settings.Spacing * (container.Columns - 1);
            _containerSettings.ContainerObject.sizeDelta = grdLayoutSize;

            // slot instantiation
            for (int y = 0; y < container.Rows; y++)
            {
                for (int x = container.Columns - 1; x >= 0; x--)
                {
                    GameObject slot = Instantiate(_slotSettings.SlotPrefab, _containerSettings.ContainerSlots.transform);
                    slot.name = $"Container Slot [{y},{x}]";

                    RectTransform rect = slot.GetComponent<RectTransform>();
                    rect.localScale = Vector3.one;

                    InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();
                    containerSlots[y, x] = inventorySlot;
                }
            }

            // container items creation
            foreach (var containerItem in container.ContainerItems)
            {
                InventoryItem inventoryItem = CreateItem(new ItemCreationData()
                {
                    ItemGuid = containerItem.Value.ItemGuid,
                    Quantity = (ushort)containerItem.Value.Quantity,
                    Orientation = containerItem.Value.Orientation,
                    Coords = containerItem.Value.Coords,
                    CustomData = containerItem.Value.CustomData,
                    Parent = _containerSettings.ContainerItems,
                    SlotsSpace = containerSlots
                });

                inventoryItem.ContainerGuid = containerItem.Key;
                inventoryItem.IsContainerItem = true;
                inventoryItem.ContainerOpened(_currentContainer.Columns);

                ContainerItems.Add(inventoryItem, null);
                OccupySlots(true, containerItem.Value.Coords, inventoryItem);
            }

            // set carrying items container opened
            foreach (var carryingItem in CarryingItems.Keys)
            {
                carryingItem.ContainerOpened(_currentContainer.Columns);
            }

            _containerSettings.ContainerObject.gameObject.SetActive(true);

            if (!string.IsNullOrEmpty((container.ContainerTitle)))
            {
                string title = container.ContainerTitle;
                _containerSettings.ContainerName.text = title.ToUpper();
                _containerSettings.ContainerName.enabled = true;
            }

            _gameManager.SetBlur(true, true);
            _gameManager.FreezePlayer(true, true, false);
            _gameManager.ShowInventoryPanel(true);
        }

        private void SetInventorySlots(InventoryContainer container, bool add)
        {
            int newRows = Mathf.Max(SlotXY.y, add ? container.Rows : 0);
            int newColumns = SlotXY.x + (add ? container.Columns : 0);
            SlotType[,] newSlotArray = new SlotType[newRows, newColumns];

            if (add)
            {
                for (int y = 0; y < newRows; y++)
                {
                    for (int x = 0; x < newColumns; x++)
                    {
                        if (x < container.Columns)
                        {
                            newSlotArray[y, x] = SlotType.Container;
                        }
                        else if (y < SlotXY.y)
                        {
                            int invX = x - container.Columns;
                            newSlotArray[y, x] = slotArray[y, invX]; 
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < newRows; y++)
                {
                    for (int x = 0; x < newColumns; x++)
                    {
                        newSlotArray[y, x] = slotArray[y, container.Columns + x];
                    }
                }
            }

            slotArray = newSlotArray;
        }

        /// <summary>
        /// Open the inventory item selection menu.
        /// </summary>
        public void OpenItemSelector(IInventorySelector inventorySelector)
        {
            _itemSelector = true;
            this._inventorySelector = inventorySelector;
            _gameManager.ShowInventoryPanel(true);
        }

        // TODO: Wwise integration.. SOUND DELAY NEEDED MAYBE?
        /// <summary>
        /// Play inventory sound.
        /// </summary>
        public void PlayInventorySound(InventorySound sound)
        {
            if (_nextSoundDelay > 0f) return;

            switch (sound)
            {
                case InventorySound.ItemSelect:
                    AudioManager.PostAudioEvent(AudioUI.UIInvItemSelect, gameObject);
                    break;
                case InventorySound.ItemMove:
                    AudioManager.PostAudioEvent(AudioUI.UIInvItemMove, gameObject);
                    break;
                case InventorySound.ItemPut:
                    AudioManager.PostAudioEvent(AudioUI.UIInvItemPut, gameObject);
                    break;
                case InventorySound.ItemError:
                    AudioManager.PostAudioEvent(AudioUI.UIInvItemError, gameObject);
                    break;
            }

            //_nextSoundDelay = sounds.NextSoundDelay;
        }

        public void ShowInventoryPrompt(bool show, string text, bool forceHide = false)
        {
            if (!show && !_promptSettings.PromptPanel.gameObject.activeSelf)
                return;

            if (show)
            {
                _promptSettings.PromptPanel.gameObject.SetActive(true);
                _promptSettings.PromptPanel.GetComponentInChildren<TMP_Text>().text = text;
            }
            else if(forceHide)
            {
                _promptSettings.PromptPanel.alpha = 0f;
                _promptSettings.PromptPanel.gameObject.SetActive(false);
                return;
            }

            var coroutine = CanvasGroupFader.StartFade(_promptSettings.PromptPanel, show, 5f, () =>
            {
                if(!show) _promptSettings.PromptPanel.gameObject.SetActive(false);
            });

            StartCoroutine(coroutine);
        }

        /// <summary>
        /// Check if the item is in inventory.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <returns>Status whether the item is in inventory.</returns>
        public bool ContainsItem(string guid, out OccupyData occupyData)
        {
            if (CarryingItems.Count > 0)
            {
                foreach (var item in CarryingItems)
                {
                    if (item.Key.ItemGuid == guid)
                    {
                        occupyData = new OccupyData()
                        {
                            InventoryItem = item.Key,
                            OccupiedSlots = item.Value
                        };
                        return true;
                    }
                }
            }

            occupyData = new OccupyData();
            return false;
        }

        /// <summary>
        /// Check if the item is in inventory.
        /// </summary>
        /// <param name="guid">Unique ID of the item.</param>
        /// <returns>Status whether the item is in inventory.</returns>
        public bool ContainsItem(string guid)
        {
            if (CarryingItems.Count > 0)
            {
                foreach (var item in CarryingItems)
                {
                    if (item.Key.ItemGuid == guid)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the item coordinates are in the container view.
        /// </summary>
        /// <param name="isContainer">Result if the coordinates of the item are in the container view.</param>
        /// <returns>Result if the coordinates are not overflowned.</returns>
        public bool IsContainerCoords(int x, int y)
        {
            if (y >= 0 && x >= 0 && x < MaxSlotXY.x && y < MaxSlotXY.y)
                return slotArray[y, x] == SlotType.Container;

            return false;
        }

        /// <summary>
        /// Check if the item coordinates are valid.
        /// </summary>
        public bool IsCoordsValid(int x, int y, int width, int height)
        {
            if (x < 0 || y < 0 || 
                x >= MaxSlotXY.x || y >= MaxSlotXY.y ||
                x + (width - 1) >= MaxSlotXY.x || y + (height - 1) >= MaxSlotXY.y)
                return false;

            SlotType prevType = slotArray[y, x];
            for (int yy = y; yy < y + height; yy++)
            {
                for (int xx = x; xx < x + width; xx++)
                {
                    SlotType currType = slotArray[yy, xx];

                    if (prevType != currType || 
                        prevType == SlotType.Restricted ||
                        currType == SlotType.Restricted) 
                        return false;

                    prevType = currType;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if the item has a combination partner in the inventory.
        /// </summary>
        public int CheckCombinePartner(InventoryItem invItem)
        {
            int combinePartners = 0;

            foreach (var item in CarryingItems)
            {
                if (item.Key.ItemGuid == invItem.ItemGuid)
                    continue;

                foreach (var itemCombine in invItem.Item.CombineSettings)
                {
                    if (itemCombine.CombineWithID == item.Key.ItemGuid)
                        if (!itemCombine.EventAfterCombine)
                            combinePartners++;
                }
            }

            return combinePartners;
        }

        /// <summary>
        /// Check if the currently selected inventory item is the currently equipped player item combination partner.
        /// </summary>
        public bool CheckCombinePlayerItem(InventoryItem invItem)
        {
            if (_playerItems.IsAnyEquipped)
            {
                foreach (var itemCombine in invItem.Item.CombineSettings)
                {
                    var playerItem = Items[itemCombine.CombineWithID];
                    int playerItemIndex = playerItem.UsableSettings.PlayerItemIndex;

                    if (_playerItems.CurrentItemIndex == playerItemIndex)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the currently equipped player item can be combined.
        /// </summary>
        public bool CheckPlayerItemCanCombine()
        {
            if (_playerItems.IsAnyEquipped)
            {
                int playerItemIndex = _playerItems.CurrentItemIndex;
                var playerItem = _playerItems.PlayerItems[playerItemIndex];
                return playerItem.CanCombine();
            }

            return false;
        }

        #region Events
        public void ShowContextMenu(bool show, InventoryItem invItem = null)
        {
            if (!ContainerOpened && show && invItem != null)
            {
                _activeItem = invItem;
                Item item = invItem.Item;

                Vector3[] itemCorners = new Vector3[4];
                invItem.GetComponent<RectTransform>().GetWorldCorners(itemCorners);

                if (invItem.Orientation == Orientation.Horizontal)
                    _contextMenu.ContextMenuGO.transform.position = itemCorners[2];
                else if (invItem.Orientation == Orientation.Vertical)
                    _contextMenu.ContextMenuGO.transform.position = itemCorners[1];

                // use button
                bool use = item.Settings.IsUsable || _itemSelector;
                bool canHeal = PlayerHealth.EntityHealth < PlayerHealth.MaxEntityHealth;
                bool isHealthItem = item.UsableSettings.UsableType == UsableType.HealthItem;
                bool useEnabled = !isHealthItem || isHealthItem && canHeal;
                float useAlpha = useEnabled ? 1f : _contextMenu.DisabledAlpha;
                _contextMenu.ContextUse.GetComponent<CanvasGroup>().alpha = useAlpha;
                _contextMenu.ContextUse.interactable = useEnabled;
                _contextMenu.ContextUse.gameObject.SetActive(use);

                // examine button
                bool examine = item.Settings.IsExaminable && !_itemSelector;
                _contextMenu.ContextExamine.gameObject.SetActive(examine);

                // combine button
                int combinePartners = CheckCombinePartner(invItem);
                bool combinePlayerItem = CheckCombinePlayerItem(invItem);
                bool playerItemCombinable = CheckPlayerItemCanCombine();

                bool combineEnabled = playerItemCombinable && combinePlayerItem || !combinePlayerItem && combinePartners > 0;
                bool combine = item.Settings.IsCombinable && !invItem.IsContainerItem && !_itemSelector;
                float combineAlpha = combineEnabled ? 1f : _contextMenu.DisabledAlpha;

                _contextMenu.ContextCombine.GetComponent<CanvasGroup>().alpha = combineAlpha;
                _contextMenu.ContextCombine.interactable = combineEnabled;
                _contextMenu.ContextCombine.gameObject.SetActive(combine);

                // shortcut button
                bool shortcut = item.Settings.CanBindShortcut && !invItem.IsContainerItem && !_itemSelector;
                _contextMenu.ContextShortcut.gameObject.SetActive(shortcut);

                // drop button
                bool drop = item.Settings.IsDroppable;
                _contextMenu.ContextDrop.gameObject.SetActive(drop);

                // discard button
                bool discard = item.Settings.IsDiscardable;
                _contextMenu.ContextDiscard.gameObject.SetActive(discard);

                if (use || examine || combine || shortcut || drop || discard)
                {
                    _contextMenu.ContextMenuGO.SetActive(true);
                    _contextMenu.BlockerPanel.SetActive(true);
                    _contextShown = true;
                }
            }
            else
            {
                _contextMenu.ContextMenuGO.SetActive(false);
                _contextMenu.BlockerPanel.SetActive(false);
                _contextMenu.ContextUse.gameObject.SetActive(false);
                _contextMenu.ContextExamine.gameObject.SetActive(false);
                _contextMenu.ContextCombine.gameObject.SetActive(false);
                _contextMenu.ContextShortcut.gameObject.SetActive(false);
                _contextMenu.ContextDrop.gameObject.SetActive(false);
                _contextMenu.ContextDiscard.gameObject.SetActive(false);
                _contextShown = false;
            }
        }

        public void ShowItemInfo(string guid)
        {
            Item item = Items[guid];
            _itemInfo.ItemTitle.text = item.Title;
            _itemInfo.ItemDescription.text = item.Description;
            _itemInfo.InfoPanel.SetActive(true);
        }

        public void HideItemInfo()
        {
            if (!_contextShown) _itemInfo.InfoPanel.SetActive(false);
        }

        public void OnBlockerClicked()
        {
            ShowContextMenu(false);
            ShowInventoryPrompt(false, null);

            if (_bindShortcut)
            {
                _bindShortcut = false;
                _activeItem = null;
            }
        }

        public void OnCloseInventory()
        {
            if (ContainerOpened)
            {
                foreach (var item in ContainerItems)
                {
                    Destroy(item.Key.gameObject);
                }

                if (containerSlots != null)
                {
                    foreach (var slot in containerSlots)
                    {
                        Destroy(slot.gameObject);
                    }
                }

                SetInventorySlots(_currentContainer, false);

                containerSlots = new InventorySlot[0, 0];
                ContainerItems.Clear();

                _containerSettings.ContainerObject.gameObject.SetActive(false);
                _containerSettings.ContainerName.enabled = false;

                _currentContainer.OnStorageClose();
                _currentContainer = null;
            }

            foreach (var item in CarryingItems)
            {
                item.Key.OnCloseInventory();
            }

            _itemSelector = false;
            _bindShortcut = false;

            _inventorySelector = null;
            _activeItem = null;

            _gameManager.ShowControlsInfo(false, null);
            ShowInventoryPrompt(false, null, true);
            ShowContextMenu(false);
            HideItemInfo();
        }
        #endregion

        private InventoryItem CreateItem(string guid, ushort quantity, ItemCustomData customData)
        {
            Item item = Items[guid];

            if (CheckSpace(item.Width, item.Height, out FreeSpace space))
            {
                InventoryItem inventoryItem = CreateItem(new ItemCreationData()
                {
                    ItemGuid = guid,
                    Quantity = quantity,
                    Orientation = space.Orientation,
                    Coords = new Vector2Int(space.X, space.Y),
                    CustomData = customData,
                    Parent = _itemsTransform,
                    SlotsSpace = slots
                });

                AddItemToFreeSpace(space, inventoryItem);
                return inventoryItem;
            }

            return null;
        }

        private InventoryItem CreateItem(ItemCreationData itemCreationData)
        {
            Item item = Items[itemCreationData.ItemGuid];

            GameObject itemGO = Instantiate(_slotSettings.SlotItemPrefab, itemCreationData.Parent);
            RectTransform rect = itemGO.GetComponent<RectTransform>();
            InventoryItem inventoryItem = itemGO.GetComponent<InventoryItem>();

            if (itemCreationData.Orientation == Orientation.Vertical)
                rect.localEulerAngles = new Vector3(0, 0, -90);

            float width = _settings.CellSize * item.Width;
            width += item.Width > 1 ? _settings.Spacing * (item.Width - 1) : 0;

            float height = _settings.CellSize * item.Height;
            height += item.Height > 1 ? _settings.Spacing * (item.Height - 1) : 0;

            rect.sizeDelta = new Vector2(width, height);
            rect.localScale = Vector3.one;

            RectTransform slot = itemCreationData.SlotsSpace[itemCreationData.Coords.y, itemCreationData.Coords.x].GetComponent<RectTransform>();
            Vector2 offset = inventoryItem.GetOrientationOffset();
            Vector2 position = new Vector2(slot.localPosition.x, slot.localPosition.y) + offset;
            rect.anchoredPosition = position;

            inventoryItem.SetItem(this, new ItemData()
            {
                Guid = itemCreationData.ItemGuid,
                Item = item,
                Quantity = itemCreationData.Quantity,
                Orientation = itemCreationData.Orientation,
                CustomData = itemCreationData.CustomData,
                SlotSpace = itemCreationData.Coords
            });

            return inventoryItem;
        }

        private void AddItemToFreeSpace(FreeSpace space, InventoryItem inventoryItem)
        {
            Item item = inventoryItem.Item;
            int maxY = item.Height, maxX = item.Width;

            if (space.Orientation == Orientation.Vertical)
            {
                maxY = item.Width;
                maxX = item.Height;
            }

            InventorySlot[] occupiedSlots = new InventorySlot[maxY * maxX];
            int slotIndex = 0;

            for (int y = space.Y; y < space.Y + maxY; y++)
            {
                for (int x = space.X; x < space.X + maxX; x++)
                {
                    InventorySlot slot = slots[y, x];
                    slot.ItemInSlot = inventoryItem;
                    occupiedSlots[slotIndex++] = slot;
                }
            }

            CarryingItems.Add(inventoryItem, occupiedSlots);
        }

        private bool CheckSpace(ushort width, ushort height, out FreeSpace slotSpace)
        {
            for (int y = 0; y < SlotXY.y; y++)
            {
                for (int x = 0; x < SlotXY.x; x++)
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

        public StorableCollection OnCustomSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();
            StorableCollection itemsToSave = new StorableCollection();
            StorableCollection shortcutsSave = new StorableCollection();

            int index = 0;
            foreach (var item in CarryingItems)
            {
                itemsToSave.Add("item_" + index++, new StorableCollection()
                {
                    { "item", item.Key.ItemGuid },
                    { "quantity", item.Key.Quantity },
                    { "orientation", item.Key.Orientation },
                    { "position", item.Key.Position.ToSaveable() },
                    { "customData", item.Key.CustomData?.GetJson() },
                });
            }

            shortcutsSave.Add("shortcut_0", _shortcuts[0].Item != null ? _shortcuts[0].Item.ItemGuid : "{}");
            shortcutsSave.Add("shortcut_1", _shortcuts[1].Item != null ? _shortcuts[1].Item.ItemGuid : "{}");
            shortcutsSave.Add("shortcut_2", _shortcuts[2].Item != null ? _shortcuts[2].Item.ItemGuid : "{}");
            shortcutsSave.Add("shortcut_3", _shortcuts[3].Item != null ? _shortcuts[3].Item.ItemGuid : "{}");

            saveableBuffer.Add("expanded", _expandedSlots);
            saveableBuffer.Add("items", itemsToSave);
            saveableBuffer.Add("shortcuts", shortcutsSave);
            return saveableBuffer;
        }

        public void OnCustomLoad(JToken data)
        {
            int expandedRowsCount = (int)data["expanded"];
            if (expandedRowsCount > 0) ExpandInventory(expandedRowsCount, false);

            JObject items = (JObject)data["items"];

            foreach (var itemProp in items.Properties())
            {
                JToken token = itemProp.Value;

                string itemGuid = token["item"].ToString();
                int quantity = (int)token["quantity"];
                Orientation orientation = (Orientation)(int)token["orientation"];
                Vector2Int position = token["position"].ToObject<Vector2Int>();
                ItemCustomData customData = new ItemCustomData()
                {
                    JsonData = token["customData"].ToString()
                };

                InventoryItem inventoryItem = CreateItem(new ItemCreationData()
                {
                    ItemGuid = itemGuid,
                    Quantity = (ushort)quantity,
                    Orientation = orientation,
                    Coords = position,
                    CustomData = customData,
                    Parent = _itemsTransform,
                    SlotsSpace = slots
                });

                AddItemToFreeSpace(new FreeSpace()
                {
                    X = position.x,
                    Y = position.y,
                    Orientation = orientation
                }, inventoryItem);
            }

            LoadShortcut(0, data["shortcuts"]["shortcut_0"].ToString());
            LoadShortcut(1, data["shortcuts"]["shortcut_1"].ToString());
            LoadShortcut(2, data["shortcuts"]["shortcut_2"].ToString());
            LoadShortcut(3, data["shortcuts"]["shortcut_3"].ToString());
        }

        private void LoadShortcut(int index, string itemGuid)
        {
            if (string.IsNullOrEmpty(itemGuid)) return;
            InventoryItem inventoryItem = GetInventoryItem(itemGuid);
            if(inventoryItem != null) SetShortcut(index, inventoryItem);
        }
    }
}