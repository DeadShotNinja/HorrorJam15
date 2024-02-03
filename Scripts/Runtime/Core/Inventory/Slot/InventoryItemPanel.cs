using TMPro;
using UnityEngine;

namespace HJ.Runtime
{
    public class InventoryItemPanel : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private TMP_Text _quantityText;

        public TMP_Text QuantityText => _quantityText;
    }
}
