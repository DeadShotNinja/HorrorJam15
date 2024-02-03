using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HJ.Runtime
{
    public class ShortcutSlot : MonoBehaviour
    {
        [SerializeField] private GameObject _itemPanel;

        [Header("References")]
        [SerializeField] private CanvasGroup _fadePanel;
        [SerializeField] private Image _background;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TMP_Text _quantity;

        [Header("Slot Colors")]
        [SerializeField] private Color _emptySlotColor;
        [SerializeField] private Color _normalSlotColor;

        private InventoryItem _inventoryItem;
        private Inventory _inventory;

        public void SetItem(InventoryItem inventoryItem)
        {
            _inventoryItem = inventoryItem;

            if(inventoryItem != null)
            {
                _inventory = inventoryItem.Inventory;
                Item item = inventoryItem.Item;

                // icon orientation and scaling
                Vector2 slotSize = _itemIcon.rectTransform.rect.size;
                slotSize -= new Vector2(10, 10);
                Vector2 iconSize = item.Icon.rect.size;

                Vector2 scaleRatio = slotSize / iconSize;
                float scaleFactor = Mathf.Min(scaleRatio.x, scaleRatio.y);

                _itemIcon.sprite = item.Icon;
                _itemIcon.rectTransform.sizeDelta = iconSize * scaleFactor;
                _quantity.text = inventoryItem.Quantity.ToString();

                _background.color = _normalSlotColor;
                _fadePanel.alpha = 1f;
                _itemPanel.SetActive(true);
            }
            else
            {
                _itemIcon.sprite = null;
                _quantity.text = string.Empty;

                _background.color = _emptySlotColor;
                _fadePanel.alpha = 0.5f;
                _itemPanel.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateItemQuantity();
        }

        private void UpdateItemQuantity()
        {
            if (_inventoryItem == null)
                return;

            int itemQuantity = _inventoryItem.Quantity;

            if (!_inventoryItem.Item.Settings.AlwaysShowQuantity)
            {
                if (itemQuantity > 1) 
                    _quantity.text = _inventoryItem.Quantity.ToString();
                else _quantity.text = string.Empty;
            }
            else
            {
                _quantity.text = itemQuantity.ToString();
                _quantity.color = itemQuantity >= 1
                    ? _inventory.SlotSettings.NormalQuantityColor
                    : _inventory.SlotSettings.ZeroQuantityColor;
            }
        }
    }
}