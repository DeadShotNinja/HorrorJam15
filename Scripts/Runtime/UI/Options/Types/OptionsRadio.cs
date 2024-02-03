using System;
using System.Linq;
using UnityEngine.Events;
using UnityEngine;
using HJ.Tools;
using TMPro;

namespace HJ.Runtime
{
    public class OptionsRadio : OptionBehaviour
    {
        [SerializeField] private TMP_Text _radioText;
        [SerializeField] private uint _current = 0;

        [Header("Radio Options")]
        [SerializeField] private bool _isCustomData;
        [SerializeField] private GString[] _options;

        [Header("Events")]
        [SerializeField] private UnityEvent<int> _onChange;

        private void Start()
        {
            if (_isCustomData)
                return;

            bool listenToChange = false;
            for (int i = 0; i < _options.Length; i++)
            {
                _options[i].SubscribeGloc(text =>
                {
                    if (!listenToChange)
                        return;

                    int index = i;
                    if (index == _current)
                        _radioText.text = text;
                });
            }

            SetOption((int)_current);
            listenToChange = true;
        }

        public void ChangeOption(int change)
        {
            int nextOption = GameTools.Wrap((int)_current + change, 0, _options.Length);
            SetOption(nextOption);
        }

        public void SetOption(int index)
        {
            _current = (uint)index;
            _radioText.text = _options[_current];
            _onChange?.Invoke((int)_current);
            IsChanged = true;
        }

        public override void SetOptionData(string[] data)
        {
            _options = new GString[0];
            _options = data.Select(x => new GString(x)).ToArray();
            _radioText.text = _options[_current];
        }

        public override object GetOptionValue()
        {
            return (int)_current;
        }

        public override void SetOptionValue(object value)
        {
            int radio = Convert.ToInt32(value);
            SetOption(radio);
            IsChanged = false;
        }
    }
}