using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace HJ.Runtime
{
    public class InteractButton : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private GameObject _separator;
        [SerializeField] private TMP_Text _interactInfo;
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Vector2 _buttonSize;

        private RectTransform _buttonRect;
        private LayoutElement _buttonLayout;

        public void SetButton(string name, Sprite button, Vector2 scale)
        {
            if(_buttonRect == null)
                _buttonRect = _buttonImage.rectTransform;

            if (_buttonLayout == null)
                _buttonLayout = _buttonImage.GetComponent<LayoutElement>();

            if (_separator != null)
                _separator.SetActive(true);

            gameObject.SetActive(true);
            _interactInfo.text = name;
            _buttonImage.sprite = button;

            _buttonRect.sizeDelta = _buttonSize * scale;
            _buttonLayout.preferredWidth = _buttonSize.x * scale.x;
            _buttonLayout.preferredHeight = _buttonSize.y * scale.y;
        }

        public void HideButton()
        {
            gameObject.SetActive(false);
            if (_separator != null) 
                _separator.SetActive(false);
        }
    }
}
