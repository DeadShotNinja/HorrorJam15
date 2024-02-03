using System;
using System.Reactive;
using System.Reactive.Disposables;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using HJ.Input;
using HJ.Tools;
using TMPro;
using static HJ.Input.InputManager;

namespace HJ.Runtime
{
    public class OptionsInput : MonoBehaviour
    {
        [SerializeField] private InputReference _inputReference;

        [Header("References")]
        [SerializeField] private Button _binding;
        [SerializeField] private TMP_Text _inputText;

        [Header("Texts")]
        [SerializeField] private GString _rebindText;
        [SerializeField] private GString _noneText;

        private InputManager _input;
        private readonly CompositeDisposable _disposables = new();

        private bool _isRebinding;
        private string _prevName;

        private void Awake()
        {
            _input = InputManager.Instance;
            _input.OnRebindStart.Subscribe(OnRebindStart).AddTo(_disposables);
            _input.OnRebindEnd.Subscribe(OnRebindEnd).AddTo(_disposables);

            _rebindText.SubscribeGloc();
            _noneText.SubscribeGloc();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void Start()
        {
            InputManagerExtention.ObserveBindingPath(_inputReference.ActionName, _inputReference.BindingIndex, (apply, newPath) =>
            {
                if (newPath == NULL)
                {
                    _inputText.text = _noneText;
                    return;
                }

                InputBinding inputBinding = new(newPath);
                _inputText.text = inputBinding.ToDisplayString(InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
                //Debug.Log("Start ran, text is now: " + _inputText.text);
            });
        }

        public void StartRebind()
        {
            _prevName = _inputText.text;
            _binding.interactable = false;

            StartRebindOperation(_inputReference.ActionName, _inputReference.BindingIndex);
            //Debug.Log(_rebindText);
            //Debug.Log("Started Rebind. Changing to: " + _rebindText);
            _inputText.text = _rebindText;
            _isRebinding = true;
        }

        private void OnRebindStart(Unit _)
        {
            _binding.interactable = false;
        }

        private void OnRebindEnd(bool completed)
        {
            if (!completed && _isRebinding)
            {
                //Debug.Log("Not complete and is rebinding true. Changing to: " + _prevName);
                _inputText.text = _prevName;
            }

            _binding.interactable = true;
            _isRebinding = false;
        }
    }
}