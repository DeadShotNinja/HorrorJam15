using System.Linq;
using UnityEngine;
using HJ.Input;

namespace HJ.Runtime
{
    public partial class Inventory
    {
        public struct Shortcut
        {
            public ShortcutSlot Slot;
            public InventoryItem Item;
        }

        private InventoryItem _activeItem;
        private ExamineController _examine;
        private PlayerItemsManager _playerItems;

        private readonly Shortcut[] _shortcuts = new Shortcut[4];

        private bool _bindShortcut;
        private bool _itemSelector;

        public void InitializeContextHandler()
        {
            _contextMenu.ContextUse.onClick.AddListener(UseItem);
            _contextMenu.ContextExamine.onClick.AddListener(ExamineItem);
            _contextMenu.ContextCombine.onClick.AddListener(CombineItem);
            _contextMenu.ContextShortcut.onClick.AddListener(ShortcutItem);
            _contextMenu.ContextDrop.onClick.AddListener(DropItem);
            _contextMenu.ContextDiscard.onClick.AddListener(DiscardItem);
            _examine = _playerPresence.Component<ExamineController>();
            _playerItems = _playerPresence.PlayerManager.PlayerItems;

            // initialize shortcuts
            _shortcuts[0].Slot = _shortcutSettings.Slot01;
            _shortcuts[1].Slot = _shortcutSettings.Slot02;
            _shortcuts[2].Slot = _shortcutSettings.Slot03;
            _shortcuts[3].Slot = _shortcutSettings.Slot04;
        }

        public void ContextUpdate()
        {
            if (_examine.IsExamining || _gameManager.IsPaused) 
                return;

            for (int i = 0; i < _shortcuts.Length; i++)
            {
                if(InputManager.ReadButtonOnce("Shortcut" + (1 + i), Controls.SHORTCUT_PREFIX + (1 + i)))
                {
                    if (_bindShortcut && _activeItem != null)
                    {
                        SetShortcut(i);
                        _bindShortcut = false;
                    }
                    else if (_shortcuts[i].Item != null && !_gameManager.IsInventoryShown)
                    {
                        UseItem(_shortcuts[i].Item);
                    }
                    break;
                }
            }
        }

        public void UseItem()
        {
            UseItem(_activeItem);
            ShowContextMenu(false);
            _activeItem = null;
        }

        private void UseItem(InventoryItem item)
        {
            if (_itemSelector)
            {
                _inventorySelector.OnInventoryItemSelect(this, item);
                _inventorySelector = null;
                _itemSelector = false;
            }
            else
            {
                var usableType = item.Item.UsableSettings.UsableType;
                if (usableType == UsableType.PlayerItem)
                {
                    int playerItemIndex = item.Item.UsableSettings.PlayerItemIndex;
                    if(playerItemIndex >= 0) PlayerItems.SwitchPlayerItem(playerItemIndex);
                }
                else if(usableType == UsableType.HealthItem)
                {
                    PlayerHealth playerHealth = _playerPresence.PlayerManager.PlayerHealth;
                    int healAmount = (int)item.Item.UsableSettings.HealthPoints;
                    int currentHealth = playerHealth.EntityHealth;

                    if(currentHealth < playerHealth.MaxEntityHealth)
                    {
                        playerHealth.OnApplyHeal(healAmount);
                        RemoveItem(item, 1);
                    }
                }
            }

            _gameManager.ShowInventoryPanel(false);
        }

        public void CombineItem()
        {
            foreach (var item in CarryingItems)
            {
                bool hasCombination = _activeItem.Item.CombineSettings.Any(x => x.CombineWithID == item.Key.ItemGuid);
                bool checkPlayerItem = CheckCombinePlayerItem(item.Key);
                item.Key.SetCombinable(true, hasCombination && !checkPlayerItem);
            }

            ShowInventoryPrompt(true, _promptSettings.CombinePrompt);
            ShowContextMenu(false);
        }

        public void CombineWith(InventoryItem secondItem)
        {
            // reset the combinability status of items
            foreach (var item in CarryingItems)
                item.Key.SetCombinable(false, false);

            // active = the item in which the combination was called
            var activeCombination = _activeItem.Item.CombineSettings.FirstOrDefault(x => x.CombineWithID == secondItem.ItemGuid);
            // second = the item that was used after selecting combine
            var secondCombination = secondItem.Item.CombineSettings.FirstOrDefault(x => x.CombineWithID == _activeItem.ItemGuid);

            // active combination events
            if (!string.IsNullOrEmpty(activeCombination.CombineWithID))
            {
                // call active inventory item, player item combination events
                if (activeCombination.EventAfterCombine && secondItem.Item.UsableSettings.UsableType == UsableType.PlayerItem)
                {
                    int playerItemIndex = secondItem.Item.UsableSettings.PlayerItemIndex;
                    var playerItem = _playerItems.PlayerItems[playerItemIndex];

                    // check if it is possible to combine a player item (e.g. reload) with an active item
                    if (playerItem.CanCombine()) playerItem.OnItemCombine(_activeItem);
                }

                // remove the active item if keepAfterCombine is false
                if (!activeCombination.KeepAfterCombine)
                {
                    RemoveShortcut(_activeItem);
                    RemoveItem(_activeItem, 1);
                }
            }

            // second combination events
            if (!string.IsNullOrEmpty(secondCombination.CombineWithID))
            {
                // remove the second item if keepAfterCombine is false
                if (!secondCombination.KeepAfterCombine)
                {
                    RemoveShortcut(secondItem);
                    RemoveItem(secondItem, 1);
                }
            }

            // select player item after combine
            if (activeCombination.SelectAfterCombine)
            {
                int playerItemIndex = activeCombination.PlayerItemIndex;
                if (playerItemIndex >= 0) _playerPresence.PlayerManager.PlayerItems.SwitchPlayerItem(playerItemIndex);
            }

            if (!string.IsNullOrEmpty(activeCombination.ResultCombineID))
            {
                AddItem(activeCombination.ResultCombineID, 1, new());
            }

            _activeItem = null;
            ShowInventoryPrompt(false, null);
        }

        public void ShortcutItem()
        {
            _bindShortcut = true;
            ShowContextMenu(false);
            ShowInventoryPrompt(true, _promptSettings.ShortcutPrompt);
            _contextMenu.BlockerPanel.SetActive(true);
        }

        private void SetShortcut(int index)
        {
            if(_shortcuts[index].Item == _activeItem)
            {
                _shortcuts[index].Item = null;
                _shortcuts[index].Slot.SetItem(null);
            }
            else
            {
                // unbind from other slot
                RemoveShortcut(_activeItem);

                // bind to a new slot
                _shortcuts[index].Item = _activeItem;
                _shortcuts[index].Slot.SetItem(_activeItem);
            }

            _activeItem = null;
            _bindShortcut = false;
            _contextMenu.BlockerPanel.SetActive(false);
            ShowInventoryPrompt(false, null);
        }

        private void SetShortcut(int index, InventoryItem item)
        {
            _shortcuts[index].Item = item;
            _shortcuts[index].Slot.SetItem(item);
        }

        private void RemoveShortcut(InventoryItem item)
        {
            for (int i = 0; i < _shortcuts.Length; i++)
            {
                if (_shortcuts[i].Item == item)
                {
                    _shortcuts[i].Item = null;
                    _shortcuts[i].Slot.SetItem(null);
                    break;
                }
            }
        }

        public void ExamineItem()
        {
            Vector3 examinePosition = _examine.InventoryPosition;
            Item item = _activeItem.Item;

            OnCloseInventory();

            if (item.ItemObject != null)
            {
                GameObject examineObj = Instantiate(item.ItemObject.Object, examinePosition, Quaternion.identity);
                examineObj.name = "Examine " + item.Title;
                _examine.ExamineFromInventory(examineObj);
            }
            else
            {
                Debug.LogError("[Inventory] Could not examine an item because the item does not contain an item drop object!");
            }

            _activeItem = null;
        }

        public void DropItem()
        {
            Vector3 dropPosition = _examine.DropPosition;
            Item item = _activeItem.Item;

            if(item.ItemObject != null)
            {
                GameObject dropObj = SaveGameManager.InstantiateSaveable(item.ItemObject, dropPosition, Vector3.zero, "Drop of " + item.Title);

                if(dropObj.TryGetComponent(out Rigidbody rigidbody))
                {
                    rigidbody.useGravity = true;
                    rigidbody.isKinematic = false;
                    rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    rigidbody.AddForce(_playerPresence.PlayerCamera.transform.forward * _settings.DropStrength);
                }
                else
                {
                    Debug.LogError("[Inventory] Drop item must have a Rigidbody component to apply drop force!");
                    return;
                }

                if(dropObj.TryGetComponent(out InteractableItem interactable))
                {
                    interactable.DisableType = InteractableItem.DisableTypeEnum.Destroy;
                    interactable.Quantity = (ushort)_activeItem.Quantity;
                }

                RemoveItem(_activeItem);
            }
            else
            {
                Debug.LogError("[Inventory] Could not drop an item because the item does not contain an item drop object!");
            }

            RemoveShortcut(_activeItem);
            ShowContextMenu(false);
            _activeItem = null;
        }

        public void DiscardItem()
        {
            RemoveShortcut(_activeItem);
            RemoveItem(_activeItem);
            ShowContextMenu(false);
            _activeItem = null;
        }
    }
}