using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using HJ.Tools;
using TMPro;

namespace HJ.Runtime
{
    public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
    {
        [SerializeField] private Image _buttonImage;
        [SerializeField] private TMP_Text _buttonText;

        [SerializeField] private bool _interactable = true;
        [SerializeField] private bool _autoDeselectOther = false;

        [SerializeField] private bool _useFade = false;
        [SerializeField] private float _fadeSpeed = 3f;

        [SerializeField] private bool _pulsating = false;
        [SerializeField] private Color _pulseColor = Color.white;
        [SerializeField] private float _pulseSpeed = 1f;
        [Range(0f, 1f)] 
        [SerializeField] private float _pulseBlend = 0.5f;

        [SerializeField] private bool _useButtonColors = true;
        [SerializeField] private Color _buttonNormal = Color.white;
        [SerializeField] private Color _buttonHover = Color.white;
        [SerializeField] private Color _buttonPressed = Color.white;
        [SerializeField] private Color _buttonSelected = Color.white;

        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _hoverSprite;
        [SerializeField] private Sprite _selectedSprite;

        [SerializeField] private Color _textNormal = Color.white;
        [SerializeField] private Color _textHover = Color.white;
        [SerializeField] private Color _textPressed = Color.white;
        [SerializeField] private Color _textSelected = Color.white;

        [SerializeField] private UnityEvent<UIButton> _onClick;

        private bool _isSelected;
        private Color _textColor;

        private Color _setButtonColor;
        private Color _currButtonColor;
        private Color _buttonColor
        {
            get => _currButtonColor;
            set
            {
                _setButtonColor = value;
                _currButtonColor = value;
            }
        }

        public UnityEvent<UIButton> OnClick => _onClick;

        private void Awake()
        {
            _buttonColor = _buttonNormal;
            _textColor = _textNormal;
            if (!_useButtonColors) _buttonImage.sprite = _normalSprite;
        }

        private void Update()
        {
            if (_pulsating && _isSelected && _useButtonColors)
            {
                float pulseBlend = GameTools.PingPong(0f, _pulseBlend, _pulseSpeed);
                _currButtonColor = Color.Lerp(_setButtonColor, _pulseColor, pulseBlend);
            }

            if (_useFade)
            {
                if (_buttonImage != null && _useButtonColors) _buttonImage.color = Color.Lerp(_buttonImage.color, _buttonColor, Time.deltaTime * _fadeSpeed);
                if (_buttonText != null) _buttonText.color = _textColor;
            }
            else
            {
                if (_buttonImage != null && _useButtonColors) _buttonImage.color = _buttonColor;
                if (_buttonText != null) _buttonText.color = _textColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_interactable || _isSelected)
                return;

            if (_useButtonColors) _buttonColor = _buttonHover;
            else
            {
                _buttonImage.sprite = _hoverSprite;
                _buttonImage.color = new Color(_buttonImage.color.r, _buttonImage.color.g, _buttonImage.color.b, 1f);
            }
            
            _textColor = _textHover;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_interactable || _isSelected)
                return;

            if (_useButtonColors) _buttonColor = _buttonNormal;
            else
            {
                _buttonImage.sprite = _normalSprite;
                _buttonImage.color = new Color(_buttonImage.color.r, _buttonImage.color.g, _buttonImage.color.b, 0f);
            }
            
            _textColor = _textNormal;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable)
                return;

            if (_useButtonColors)  _buttonColor = _buttonPressed;
            else
            {
                _buttonImage.sprite = _selectedSprite;
                _buttonImage.color = new Color(_buttonImage.color.r, _buttonImage.color.g, _buttonImage.color.b, 1f);
            }
            
            _textColor = _textPressed;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_interactable)
                return;

            if (_autoDeselectOther)
            {
                foreach (var button in transform.parent.GetComponentsInChildren<UIButton>())
                {
                    if (button == this)
                        continue;

                    button.DeselectButton();
                }
            }

            if (_useButtonColors)  _buttonColor = _buttonSelected;
            else
            {
                _buttonImage.sprite = _selectedSprite;
                _buttonImage.color = new Color(_buttonImage.color.r, _buttonImage.color.g, _buttonImage.color.b, 1f);
            }
            
            _textColor = _textSelected;
            _onClick?.Invoke(this);
            _isSelected = true;
        }

        public void SelectButton()
        {
            if (_useButtonColors)  _buttonColor = _buttonSelected;
            else
            {
                _buttonImage.sprite = _selectedSprite;
                _buttonImage.color = new Color(_buttonImage.color.r, _buttonImage.color.g, _buttonImage.color.b, 1f);
            }
            
            _textColor = _textSelected;
            _isSelected = true;
        }

        public void DeselectButton()
        {
            if (_useButtonColors)  _buttonColor = _buttonNormal;
            else
            {
                _buttonImage.sprite = _normalSprite;
                _buttonImage.color = new Color(_buttonImage.color.r, _buttonImage.color.g, _buttonImage.color.b, 0f);
            }
            
            _textColor = _textNormal;
            _isSelected = false;
        }
    }
}