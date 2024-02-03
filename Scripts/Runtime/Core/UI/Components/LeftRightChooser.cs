using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace HJ.Runtime
{
    public class LeftRightChooser : MonoBehaviour
    {
        [SerializeField] private Button _buttonLeft;
        [SerializeField] private Button _buttonRight;
        [SerializeField] private TextMeshProUGUI _label;

        private List<string> _texts;
        private int _current;

        public Action<int> OnCurrentChanged = delegate { };
        
        public void Init(List<string> texts, int current, bool isInteractable = true)
        {
            _texts = texts;
            _current = current;
            
            Assert.IsTrue(current >= 0);
            Assert.IsTrue(current < texts.Count);

            if (!isInteractable) 
                SetInteractable(false);
            
            ResetLabel();
        }

        public void SetInteractable(bool interactable)
        {
            _buttonLeft.interactable = interactable;
            _buttonRight.interactable = interactable;   
        }
        
        // Called from UI
        public void OnButtonLeftPressed()
        {
            _current -= 1;
            if (_current < 0)
                _current += _texts.Count;
            
            ResetLabel();
        }
        
        // Called from UI
        public void OnButtonRightPressed()
        {
            _current += 1;
            if (_current >= _texts.Count)
                _current = 0;
            
            ResetLabel();
        }

        private void ResetLabel()
        {
            _label.text = _texts[_current];
            OnCurrentChanged?.Invoke(_current);
        }
    }
}
