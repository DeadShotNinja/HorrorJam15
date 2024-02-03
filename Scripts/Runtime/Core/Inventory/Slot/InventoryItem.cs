using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HJ.Input;
using HJ.Tools;

namespace HJ.Runtime
{
    public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private enum HoverType { Normal, Hover, Move, Error }

        [SerializeField] private Orientation _orientation;
        [SerializeField] private Image _itemImage;
        [SerializeField] private Image _background;
        [Space]
        [SerializeField] private InventoryItemPanel _horizontalPanel;
        [SerializeField] private InventoryItemPanel _verticalPanel;
        [SerializeField] private InventoryItemPanel _activePanel;

        private RectTransform _rectTransform;
        private Color _currentColor;

        private Orientation _lastOrientation;
        private Vector2Int _currentSlot;
        private Vector2Int _lastSlot;

        private Vector2 _dragOffset;
        private Vector2 _dragVelocity;

        private Vector2 _mousePosition;
        private Vector2 _mouseDelta;

        private float _targetRotation;
        private float _itemRotation;
        private float _rotationVelocity;
        private int _containerColumns;

        private bool _isCombining;
        private bool _isCombinable;
        private bool _isInitialized;
        
        [HideInInspector] public bool IsOver;
        [HideInInspector] public bool IsMoving;
        [HideInInspector] public bool IsRotating;
        [HideInInspector] public bool IsContainerItem;
        
        public Vector2Int Position => _lastSlot;
        public Orientation Orientation => _orientation;
        
        public Inventory Inventory { get; set; }
        public string ContainerGuid { get; set; }
        public string ItemGuid { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
        public ItemCustomData CustomData { get; set; }

        // This is to fix a issue with the improper positioning of item
        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            _rectTransform.anchoredPosition = GetItemPosition(_lastSlot);
            _isInitialized = true;
        }

        public void ContainerOpened(ushort columns)
        {
            _containerColumns = columns;
            if (string.IsNullOrEmpty(ContainerGuid))
            {
                _currentSlot.x += _containerColumns;
                _lastSlot.x += _containerColumns;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CheckAnyItemMoving() && (!_isCombining || _isCombinable))
            {
                IsOver = true;
                Inventory.ShowItemInfo(ItemGuid);

                foreach (var item in Inventory.CarryingItems)
                {
                    if (item.Key != this) item.Key.IsOver = false;
                }

                if (Inventory.ContainerOpened)
                {
                    foreach (var item in Inventory.ContainerItems)
                    {
                        if (item.Key != this) item.Key.IsOver = false;
                    }
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData) { }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsOver && !CheckAnyItemMoving() && (!_isCombining || _isCombinable))
            {
                if(!_isCombinable) Inventory.ShowContextMenu(true, this);
                else Inventory.CombineWith(this);
            }
        }

        /// <summary>
        /// Set and initialize inventory item.
        /// </summary>
        public void SetItem(Inventory inventory, ItemData itemData)
        {
            Inventory = inventory;
            ItemGuid = itemData.Guid;
            Item = itemData.Item;
            Quantity = itemData.Quantity;
            CustomData = itemData.CustomData;
            _orientation = itemData.Orientation;

            _lastOrientation = _orientation;
            _currentSlot = itemData.SlotSpace;
            _lastSlot = itemData.SlotSpace;
            _itemImage.sprite = Item.Icon;

            _rectTransform = GetComponent<RectTransform>();

            // icon orientation and scaling
            Vector2 slotSize = _rectTransform.rect.size;
            slotSize -= new Vector2(20, 20);
            Vector2 iconSize = Item.Icon.rect.size;

            Vector2 newIconSize = Item.Orientation == ImageOrientation.Normal
                ?  iconSize : new Vector2(iconSize.y, iconSize.x);

            Vector2 scaleRatio = slotSize / newIconSize;
            float scaleFactor = Mathf.Min(scaleRatio.x, scaleRatio.y);

            // icon flipping
            if (Item.Orientation == ImageOrientation.Flipped)
            {
                float flipDirection = Item.FlipDirection == FlipDirection.Left ? 90 : -90;
                _itemImage.rectTransform.localEulerAngles = new Vector3(0, 0, flipDirection);
            }

            _itemImage.rectTransform.sizeDelta = iconSize * scaleFactor;

            ShowOrientationPanel();
            _background.color = _currentColor = inventory.SlotSettings.ItemNormalColor;
        }

        /// <summary>
        /// Set the combinability status of the item.
        /// </summary>
        public void SetCombinable(bool combine, bool combinable)
        {
            _isCombining = combine;
            _isCombinable = combinable;

            Color color = _itemImage.color;
            color.a = !_isCombining || _isCombinable ? 1f : 0.3f;
            _itemImage.color = color;
            IsOver = false;
        }

        /// <summary>
        /// Set item quantity.
        /// </summary>
        public void SetQuantity(int quantity)
        {
            Quantity = quantity;
            UpdateItemQuantity();
        }

        /// <summary>
        /// Event when inventory closes.
        /// </summary>
        public void OnCloseInventory()
        {
            if(_lastOrientation != _orientation)
            {
                _orientation = _lastOrientation;
                float lastRotation = 0;

                if (_lastOrientation == Orientation.Vertical)
                    lastRotation = -90;

                Vector3 angles = transform.localEulerAngles;
                angles.z = lastRotation;
                transform.eulerAngles = angles;
            }

            if (_containerColumns > -1 && string.IsNullOrEmpty(ContainerGuid))
            {
                _currentSlot.x -= _containerColumns;
                _lastSlot.x -= _containerColumns;
                _containerColumns = -1;
            }

            if (_lastSlot != _currentSlot)
            {
                _currentSlot = _lastSlot;
                _rectTransform.position = GetItemPosition(_lastSlot);
            }

            _background.color = Inventory.SlotSettings.ItemNormalColor;
            SetCombinable(false, false);
            ShowOrientationPanel();

            IsOver = false;
            IsMoving = false;
            IsRotating = false;
        }

        /// <summary>
        /// Get item dimensions from current orientation.
        /// </summary>
        public Vector2Int GetItemDimensions()
        {
            return _orientation == Orientation.Vertical
                ? new Vector2Int(Item.Height, Item.Width)
                : new Vector2Int(Item.Width, Item.Height);
        }

        private void Update()
        {
            if (Inventory != null)
            {
                // background color changing
                if (IsOver && !IsMoving) UpdateBackground(HoverType.Hover);
                else if (IsMoving) UpdateBackground(HoverType.Move);
                else UpdateBackground(HoverType.Normal);

                // item slot selection
                if (IsMoving && !IsRotating && _mouseDelta.magnitude > 0)
                {
                    Vector2Int dimensions = GetItemDimensions();
                    Vector2 dragPos = _mousePosition - _dragOffset;
                    Vector2Int newSlotPosition = _currentSlot;
                    float distance = Mathf.Infinity;

                    for (int y = 0; y < Inventory.MaxSlotXY.y; y++)
                    {
                        for (int x = 0; x < Inventory.MaxSlotXY.x; x++)
                        {
                            InventorySlot slot = Inventory[y, x];
                            if (slot == null) continue;

                            Vector2 position = new Vector2(slot.transform.position.x, slot.transform.position.y);
                            float slotDistance = Vector2.Distance(dragPos, position);

                            if (slotDistance < distance)
                            {
                                if (!Inventory.IsCoordsValid(x, y, dimensions.x, dimensions.y))
                                    continue;

                                newSlotPosition = new Vector2Int(x, y);
                                distance = slotDistance;
                            }
                        }
                    }

                    if (_currentSlot != newSlotPosition)
                        Inventory.PlayInventorySound(InventorySound.ItemMove);

                    _currentSlot = newSlotPosition;
                }

                if (!IsRotating && _isInitialized)
                {
                    // item movement
                    Vector2 position = _rectTransform.localPosition;
                    Vector2 slotPosition = GetItemPosition(_currentSlot);

                    position = Vector2.SmoothDamp(position, slotPosition, ref _dragVelocity, Inventory.Settings.DragTime);
                    _rectTransform.localPosition = position;
                }
                else if(IsRotating)
                {
                    // item rotation
                    if (Mathf.Abs(_itemRotation - _targetRotation) > 1f)
                    {
                        _itemRotation = Mathf.SmoothDamp(_itemRotation, _targetRotation, ref _rotationVelocity, Inventory.Settings.RotateTime);
                    }
                    else
                    {
                        _itemRotation = _targetRotation;
                        _rotationVelocity = 0f;
                        ShowOrientationPanel();
                        IsRotating = false;
                    }

                    Vector3 angles = transform.localEulerAngles;
                    angles.z = _itemRotation;
                    transform.eulerAngles = angles;
                }

                // inventory inputs
                GetInput();
            }
        }

        private Vector2 WorldToLocalPosition(RectTransform sourceRectTransform, RectTransform targetRectTransform)
        {
            Vector3 worldPosition = sourceRectTransform.position;
            Vector3 targetLocalPosition = targetRectTransform.InverseTransformPoint(worldPosition);
            return new Vector2(targetLocalPosition.x, targetLocalPosition.y);
        }

        private Vector2 GetItemPosition(Vector2Int coords)
        {
            RectTransform slot = Inventory[coords.y, coords.x].GetComponent<RectTransform>();
            RectTransform parent = transform.parent.GetComponent<RectTransform>();

            Vector2 slotPos = WorldToLocalPosition(slot, parent);
            Vector2 offset = GetOrientationOffset();
            return slotPos + offset;
        }

        public Vector2 GetOrientationOffset()
        {
            if (_rectTransform)
            {
                Vector2 sizeDelta = _rectTransform.sizeDelta;

                if (_orientation == Orientation.Horizontal)
                {
                    return new Vector2(sizeDelta.x / 2f, -sizeDelta.y / 2f);
                }
                else
                {
                    return new Vector2(sizeDelta.y / 2f, -sizeDelta.x / 2f);
                }
            }

            return Vector2.zero;
        }

        private void UpdateBackground(HoverType hoverType)
        {
            switch (hoverType)
            {
                case HoverType.Normal:
                    _currentColor = Inventory.SlotSettings.ItemNormalColor;
                    break;
                case HoverType.Hover:
                    _currentColor = Inventory.SlotSettings.ItemHoverColor;
                    break;
                case HoverType.Move:
                    _currentColor = Inventory.SlotSettings.ItemMoveColor;
                    break;
                case HoverType.Error:
                    _background.color = Inventory.SlotSettings.ItemErrorColor;
                    break;
            }

            float defaultAlpha = _currentColor.a;

            if (hoverType == HoverType.Hover)
                _currentColor.a = GameTools.PingPong(0.3f, 1f);
            else _currentColor.a = defaultAlpha;

            _background.color = Color.Lerp(_background.color, _currentColor, Time.deltaTime * Inventory.SlotSettings.ColorChangeSpeed);
        }

        private void UpdateItemQuantity()
        {
            if(_activePanel != null)
            {
                if (!Item.Settings.AlwaysShowQuantity)
                {
                    if (Quantity > 1)
                    {
                        _activePanel.gameObject.SetActive(true);
                        _horizontalPanel.QuantityText.text = Quantity.ToString();
                        _verticalPanel.QuantityText.text = Quantity.ToString();
                    }
                    else _activePanel.gameObject.SetActive(false);
                }
                else
                {
                    _horizontalPanel.QuantityText.text = Quantity.ToString();
                    _verticalPanel.QuantityText.text = Quantity.ToString();
                    _activePanel.QuantityText.color = Quantity >= 1
                        ? Inventory.SlotSettings.NormalQuantityColor
                        : Inventory.SlotSettings.ZeroQuantityColor;

                    if (_activePanel != _horizontalPanel)
                        _horizontalPanel.QuantityText.color = Inventory.SlotSettings.NormalQuantityColor;
                    else if (_activePanel != _verticalPanel)
                        _verticalPanel.QuantityText.color = Inventory.SlotSettings.NormalQuantityColor;
                }
            }
        }

        private void ShowOrientationPanel()
        {
            if (_activePanel != null) _activePanel.gameObject.SetActive(false);

            if (_orientation == Orientation.Horizontal)
            {
                _horizontalPanel.gameObject.SetActive(true);
                _activePanel = _horizontalPanel;
            }
            else
            {
                _verticalPanel.gameObject.SetActive(true);
                _activePanel = _verticalPanel;
            }

            UpdateItemQuantity();
        }

        private void GetInput()
        {
            if (IsOver || IsMoving)
            {
                // get mouse position and mouse delta
                _mousePosition = InputManager.ReadInput<Vector2>(Controls.POINTER);
                _mouseDelta = InputManager.ReadInput<Vector2>(Controls.POINTER_DELTA);

                // item movement input
                if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.INVENTORY_ITEM_MOVE))
                {
                    if (!IsMoving)
                    {
                        if(Inventory.ContainerOpened)
                            transform.SetParent(Inventory.InventoryContainers);

                        Inventory.ShowContextMenu(false);
                        Inventory.PlayInventorySound(InventorySound.ItemSelect);
                        _dragOffset = GetOrientationOffset();
                        IsMoving = true;
                    }
                    else if (CheckPutSpace())
                    {
                        Inventory.PlayInventorySound(InventorySound.ItemPut);
                        Inventory.MoveItem(_lastSlot, _currentSlot, this);
                        _lastOrientation = _orientation;
                        _lastSlot = _currentSlot;
                        IsMoving = false;

                        if (!(IsOver = IsPointerOverItem()))
                            Inventory.HideItemInfo();
                    }
                    else
                    {
                        Inventory.PlayInventorySound(InventorySound.ItemError);
                        UpdateBackground(HoverType.Error);
                    }

                    transform.SetAsLastSibling();
                }
            }

            // item rotation input
            if (InputManager.ReadButtonOnce(GetInstanceID(), Controls.INVENTORY_ITEM_ROTATE) && IsMoving && !IsRotating && Item.Width != Item.Height)
            {
                Inventory.PlayInventorySound(InventorySound.ItemMove);

                if (_orientation == Orientation.Horizontal)
                {
                    _targetRotation = -90;
                    _orientation = Orientation.Vertical;
                }
                else
                {
                    _targetRotation = 0;
                    _orientation = Orientation.Horizontal;
                }

                _dragOffset = GetOrientationOffset();
                _activePanel.gameObject.SetActive(false);
                IsRotating = true;
            }
        }

        private bool CheckPutSpace()
        {
            Vector2Int dimensions = GetItemDimensions();
            return Inventory.CheckSpaceFromPosition(_currentSlot.x, _currentSlot.y, dimensions.x, dimensions.y, this);
        }

        private bool CheckAnyItemMoving()
        {
            if(Inventory != null)
            {
                foreach (var item in Inventory.CarryingItems)
                {
                    if (item.Key.IsMoving) return true;
                }

                if (Inventory.ContainerOpened)
                {
                    foreach (var item in Inventory.ContainerItems)
                    {
                        if (item.Key.IsMoving) return true;
                    }
                }
            }

            return false;
        }

        private bool IsPointerOverItem()
        {
            EventSystem eventSystem = EventSystem.current;
            PointerEventData eventDataCurrentPosition = new(EventSystem.current);
            eventDataCurrentPosition.position = _mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            return results.Any(x => x.gameObject == transform.GetChild(0).gameObject);
        }
    }
}