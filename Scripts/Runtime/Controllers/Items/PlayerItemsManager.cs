using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HJ.Input;

namespace HJ.Runtime
{
    public class PlayerItemsManager : PlayerComponent
    {
        [Header("Setup")]
        [SerializeField] private List<PlayerItemBehaviour> _playerItems = new();
        [SerializeField] private float _antiSpamDelay = 0.5f;
        [SerializeField] private bool _isItemsUsable = true;

        private PlayerItemBehaviour _currentItem;
        private PlayerItemBehaviour _previousItem;
        private PlayerItemBehaviour _nextItem;

        private bool _canSwitch = true;
        private bool _wasDeactivated = false;

        public List<PlayerItemBehaviour> PlayerItems => _playerItems;
        public bool IsItemsUsable
        {
            get => _isItemsUsable;
            set => _isItemsUsable = value;
        }
        public bool CanInteract => _isEnabled && _isItemsUsable;
        public bool IsAnyEquipped => CurrentItemIndex != -1;

        public PlayerItemBehaviour CurrentItem => _currentItem;
        public int CurrentItemIndex => _playerItems.IndexOf(_currentItem);

        public PlayerItemBehaviour PreviousItem => _previousItem;
        public int PreviousItemIndex => _playerItems.IndexOf(_previousItem);

        private void Awake()
        {
            InputManager.Performed(Controls.ITEM_UNEQUIP, _ => DeselectCurrent());
            _canSwitch = true;
        }

        /// <summary>
        /// Switch or select a player item.
        /// </summary>
        /// <param name="itemID">Index of the player item in the PlayerItems list.</param>
        public void SwitchPlayerItem(int itemID)
        {
            if (_currentItem != null && _currentItem.IsBusy() || !_canSwitch || !_isItemsUsable || ExamineController.IsExamining)
                return;

            StopAllCoroutines();
            _nextItem = _playerItems[itemID];
            _wasDeactivated = false;
            _canSwitch = false;

            if (_nextItem != _currentItem)
            {
                if (_currentItem != null && _currentItem.IsEquipped())
                {
                    StartCoroutine(SwitchItem());
                }
                else
                {
                    _previousItem = _currentItem;
                    _currentItem = _nextItem;
                    _nextItem = null;

                    StartCoroutine(SelectItem());
                }
            }
            else
            {
                DeselectCurrent();
                _nextItem = null;
            }
        }

        /// <summary>
        /// Activate player item.
        /// </summary>
        /// <param name="itemID">Index of the player item in the PlayerItems list.</param>
        public void ActivateItem(int itemID)
        {
            _nextItem = _playerItems[itemID];

            if (_currentItem == null || _nextItem != _currentItem)
                return;

            _previousItem = _currentItem;
            _currentItem = _nextItem;
            _nextItem = null;

            _previousItem.OnItemDeactivate();
            _currentItem.OnItemActivate();
            _wasDeactivated = false;
            _canSwitch = true;
        }

        /// <summary>
        /// Activate a player item that was previously deselected.
        /// </summary>
        public void ActivatePreviousItem()
        {
            if (_previousItem == null)
                return;

            var current = _currentItem;
            _currentItem = _previousItem;
            _previousItem = current;

            _currentItem.OnItemActivate();
            _wasDeactivated = false;
            _canSwitch = true;
        }

        /// <summary>
        /// Select a previously deselected player item.
        /// </summary>
        public void SelectPreviousItem()
        {
            if (_previousItem == null)
                return;

            var current = _currentItem;
            _currentItem = _previousItem;
            _previousItem = current;

            StopAllCoroutines();
            StartCoroutine(SelectItem());
            _wasDeactivated = false;
            _canSwitch = false;
        }

        /// <summary>
        /// Activate a player item that was previously deactivated.
        /// </summary>
        public void ActivatePreviouslyDeactivatedItem()
        {
            if (_previousItem == null || !_wasDeactivated)
                return;

            var current = _currentItem;
            _currentItem = _previousItem;
            _previousItem = current;

            _currentItem.OnItemActivate();
            _wasDeactivated = false;
            _canSwitch = true;
        }

        /// <summary>
        /// Select a player item that was previously deactivated.
        /// </summary>
        public void SelectPreviouslyDeactivatedItem()
        {
            if (_previousItem == null || !_wasDeactivated)
                return;

            var current = _currentItem;
            _currentItem = _previousItem;
            _previousItem = current;

            StopAllCoroutines();
            StartCoroutine(SelectItem());
            _wasDeactivated = false;
            _canSwitch = false;
        }

        /// <summary>
        /// Deselect the currently equipped player item.
        /// </summary>
        public void DeselectCurrent()
        {
            if (_currentItem == null)
                return;

            _previousItem = _currentItem;

            StopAllCoroutines();
            StartCoroutine(DeselectItem());
        }

        /// <summary>
        /// Deactivate the currently equipped player item.
        /// </summary>
        public void DeactivateCurrentItem()
        {
            if (_currentItem == null)
                return;

            _previousItem = _currentItem;
            _currentItem = null;
            _wasDeactivated = true;

            StopAllCoroutines();
            _previousItem.OnItemDeactivate();
        }

        /// <summary>
        /// Register the current item as a previously deactivated item.
        /// </summary>
        public void RegisterPreviousItem()
        {
            if (_currentItem == null)
                return;

            _previousItem = _currentItem;
            _wasDeactivated = true;
        }

        IEnumerator SwitchItem()
        {
            _currentItem.OnItemDeselect();
            yield return new WaitUntil(() => !_currentItem.IsEquipped());

            _previousItem = _currentItem;
            _currentItem = _nextItem;
            _nextItem = null;

            _currentItem.OnItemSelect();
            yield return AntiSpam();
        }

        IEnumerator SelectItem()
        {
            _currentItem.OnItemSelect();
            yield return AntiSpam();
        }

        IEnumerator DeselectItem()
        {
            _currentItem.OnItemDeselect();
            yield return new WaitUntil(() => !_currentItem.IsEquipped());
            yield return AntiSpam();

            _currentItem = null;
            _nextItem = null;
        }

        IEnumerator AntiSpam()
        {
            yield return new WaitForSeconds(_antiSpamDelay);
            _canSwitch = true;
        }
    }
}