using UnityEngine;
using UnityEngine.UI;

namespace HJ.Runtime
{
    public class InventorySlot : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private Image _frame;
        [SerializeField] private InventoryItem _itemInSlot;

        public Image Frame => _frame;

        public InventoryItem ItemInSlot
        {
            get => _itemInSlot;
            set => _itemInSlot = value;
        }

        private CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if(_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();

                return _canvasGroup;
            }
        }
    }
}
