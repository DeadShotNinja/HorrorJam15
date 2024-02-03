using System;
using System.Collections.Generic;
using System.Linq;
using HJ.Input;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace HJ.Runtime
{
    /// <summary>
    /// A thing where the player can place/take out a gear.
    /// </summary>
    public class ShaftV2 : MonoBehaviour, IInteractStart
    {
        [SerializeField]
        private GearsSystem _system;

        [SerializeField]
        private int _systemGearIndex = -1;

        [SerializeField]
        [ReadOnly]
        private Gear _gear;

        [Required]
        [SerializeField]
        private InteractController _interactController;

        public InventoryItem CurrentlySelectedItem
        {
            get
            {
                if (_currentlySelectedItem == null && CountOfRequiredItemsPlayerHas > 0)
                    SelectNextGearItem();

                return _currentlySelectedItem;
            }
        }

        private InventoryItem _currentlySelectedItem = null;

        public string InteractTitle_WhenPlayerDoesntHaveAnyOfTheRequiredItems
        {
            get
            {
                // var msg = $"One of the following items is required: ";
                // msg += string.Join(", ", _itemsForPlacement.Select(i => i.ToString()));
                // return msg;
                return "You don't have any suitable item";
            }
        }

        public int CountOfRequiredItemsPlayerHas
        {
            get
            {
                var count = 0;
                foreach (var item in _itemsForPlacement)
                {
                    foreach (var inventoryItem in Inventory.Instance.CarryingItems.Keys)
                    {
                        if (inventoryItem.ItemGuid == item.item.GUID)
                            count += 1;
                    }
                }

                return count;
            }
        }

        [SerializeField]
        private List<ItemForPlacement> _itemsForPlacement = new();

        private ItemForPlacement _placedGear;

        public ItemForPlacement PlacedGear => _placedGear;

        private void OnValidate()
        {
            _gear = null;
            if (_systemGearIndex < 0 || _system == null)
                return;

            if (_systemGearIndex >= _system.Gears.Count)
            {
                Debug.LogWarning("Value of _systemGearIndex is higher than count of available gears in _system!");
                return;
            }

            _gear = _system.Gears[_systemGearIndex];
            if (_gear != null)
            {
                Debug.LogError("Setting up a shaft with a gear already placed currently is not supported");
            }
        }

        private void Update()
        {
            if (_interactController.RaycastObject != gameObject)
                return;

            if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.EXAMINE))
            {
                SelectNextGearItem();
                // TODO: Sound?
            }
        }

        private void SelectNextGearItem()
        {
            if (CountOfRequiredItemsPlayerHas == 0)
                return;
            
            var placeableIds = _itemsForPlacement.Select(i => i.item.GUID).ToArray();
            if (CountOfRequiredItemsPlayerHas == 1 || _currentlySelectedItem == null)
            {
                foreach (var item in Inventory.Instance.CarryingItems.Keys)
                {
                    if (!placeableIds.Contains(item.ItemGuid))
                        continue;

                    _currentlySelectedItem = item;
                    return;
                }
            }
            
            var shouldSelectNextOne = false;
            foreach (var item in Inventory.Instance.CarryingItems.Keys)
            {
                if (!placeableIds.Contains(item.ItemGuid))
                    continue;

                if (shouldSelectNextOne)
                {
                    _currentlySelectedItem = item;
                    return;
                }

                if (item.ItemGuid == _currentlySelectedItem.ItemGuid)
                    shouldSelectNextOne = true;
            }

            foreach (var item in Inventory.Instance.CarryingItems.Keys)
            {
                if (!placeableIds.Contains(item.ItemGuid))
                    continue;

                _currentlySelectedItem = item;
                return;
            }
        }

        GameObject GetGearPrefab()
        {
            for (var i = 0; i < _itemsForPlacement.Count; i++)
            {
                if (_itemsForPlacement[i].item.GUID == CurrentlySelectedItem.ItemGuid)
                    return _itemsForPlacement[i].respectivePrefab;
            }

            return null;
        }

        public void InteractStart()
        {
            if (_placedGear == null)
            {
                if (CurrentlySelectedItem == null)
                {
                    // TODO: Error WWise sound
                    return;
                }
                
                var item = _itemsForPlacement.Find(i => i.item.GUID == CurrentlySelectedItem.ItemGuid);
                if (item.disallowedForPlacement)
                {
                    // TODO: Error WWise sound
                    GameManager.Instance.ShowHintMessage($"{item.item.GetItem().Title} does not fit here", 3);
                    return;
                }
                
                var gearPrefab = GetGearPrefab();
                var gear = Instantiate(gearPrefab, transform);
                _gear = (Gear)gear.GetComponent(typeof(Gear));
                _system.SetGear(_systemGearIndex, _gear);

                _placedGear = item;
                Inventory.Instance.RemoveItem(CurrentlySelectedItem.ItemGuid, 1);

                AudioManager.PostAudioEvent(AudioItems.ItemPickup, gameObject);
            }
            else
            {
                _system.SetGear(_systemGearIndex, null);
                Destroy(_gear.gameObject);

                Inventory.Instance.AddItem(_placedGear.item.GUID, 1, new());
                _placedGear = null;
                
                // TODO: Show loot text?
                AudioManager.PostAudioEvent(AudioItems.ItemPickup, gameObject);
            }
        }
    }

    [Serializable]
    public class ItemForPlacement
    {
        public ItemProperty item;
        public GameObject respectivePrefab;

        [FormerlySerializedAs("diallowedForPlacement")]
        public bool disallowedForPlacement;
    }
}
