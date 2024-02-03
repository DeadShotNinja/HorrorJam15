using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace HJ.Runtime
{
    public class MenuHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Setup")]
        [SerializeField] private Button _buttonHover;
        [SerializeField] private TMP_Text _tooltipText;
        [SerializeField] private GString _tooltipMessage;

        private bool _isHover;

        private void Awake()
        {
            _tooltipMessage.SubscribeGloc(text =>
            {
                if (_isHover) _tooltipText.text = text;
            });
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_buttonHover != null && !_buttonHover.interactable)
                return;

            _tooltipText.text = _tooltipMessage;
            _isHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltipText.text = "";
            _isHover = false;
        }
    }
}