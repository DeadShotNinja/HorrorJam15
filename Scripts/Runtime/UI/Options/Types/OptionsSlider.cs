using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HJ.Runtime
{
    public class OptionsSlider : OptionBehaviour
    {
        public enum SliderTypeEnum { FloatSlider, IntegerSlider }

        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _sliderText;

        [Header("Slider Settings")]
        [SerializeField] private SliderTypeEnum _sliderType = SliderTypeEnum.FloatSlider;
        [SerializeField] private MinMax _sliderLimits = new(0, 1);
        [SerializeField] private float _sliderValue = 0f;

        [Header("Snap Settings")]
        [SerializeField] private bool _useSnapping;
        [SerializeField] private float _snapValue = 0.05f;

        private void Start()
        {
            float value = _sliderValue;
            _slider.wholeNumbers = _sliderType == SliderTypeEnum.IntegerSlider;
            _slider.minValue = _sliderLimits.RealMin;
            _slider.maxValue = _sliderLimits.RealMax;

            _slider.value = value;
            _sliderText.text = _sliderValue.ToString();
        }

        public void SetSliderValue(float value)
        {
            if (_sliderType == SliderTypeEnum.FloatSlider)
                _sliderValue = (float)Math.Round(value, 2);
            else if (_sliderType == SliderTypeEnum.IntegerSlider)
                _sliderValue = Mathf.RoundToInt(value);

            if(_useSnapping) 
                _sliderValue = SnapTo(_sliderValue, _snapValue);

            _sliderText.text = _sliderValue.ToString();
            IsChanged = true;
        }

        private float SnapTo(float value, float multiple)
        {
            return Mathf.Round(value / multiple) * multiple;
        }

        public override object GetOptionValue()
        {
            return _sliderType switch
            {
                SliderTypeEnum.FloatSlider => _sliderValue,
                SliderTypeEnum.IntegerSlider => Mathf.RoundToInt(_sliderValue),
                _ => _sliderValue
            };
        }

        public override void SetOptionValue(object value)
        {
            SetSliderValue((float)value);
            _slider.value = _sliderValue;
            IsChanged = false;
        }
    }
}