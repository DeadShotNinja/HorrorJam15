using DG.Tweening;
using HJ.Input;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HJ.Runtime
{
    public class LockpickPuzzleInterface : PuzzleBase
    {
        public UnityEvent OnSuccess;
        public UnityEvent OnCompleteSuccess;
        public UnityEvent OnCanceled;
        public UnityEvent OnLockPickBroken;

        [Header("Configuration")]
        [SerializeField] private float _easingOpacityDuration = 1f;
        [SerializeField] private AnimationCurve _startingPuzzleOpacityCurve;
        [SerializeField] private AnimationCurve _stoppingPuzzleOpacityCurve;
        
        [SerializeField] [Min(.1f)] private float _soundThreshold = 20f;

        [Header("Dependencies")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _lockImage;
        [SerializeField] private Image _pickerImage;
        [SerializeField] private Image _rotatorImage;
        [SerializeField] private LockpickPuzzle _puzzle;

        private Inventory _inventory;
        
        private InputAction _lmbAction;
        private float _lockpickAngle;
        private bool _inputListeningTurnedOn;
        
        private float _rotationBeforeSound;

        public override void Awake()
        {
            base.Awake();
            
            Assert.IsNotNull(_canvasGroup);
            Assert.IsNotNull(_lockImage);
            Assert.IsNotNull(_pickerImage);
            Assert.IsNotNull(_rotatorImage);
            
            _canvasGroup.alpha = 0;
            
            _lmbAction = InputManager.Action(Controls.LEFT_BUTTON);

            _inputListeningTurnedOn = false;

            _puzzle.OnSuccess += () =>
            {
                OnSuccess?.Invoke();
                AudioManager.PostAudioEvent(AudioUI.UILockpickSuccess, gameObject);
            };
            _puzzle.OnCompleteSuccess += () => {
                DisableInteract(); 
                OnCompleteSuccess?.Invoke();
                SwitchBack();
                
                DOTween.To(() => _canvasGroup.alpha, val => _canvasGroup.alpha = val, 0, 1f)
                    .OnComplete(() => _canvasGroup.alpha = 0);
                
                _inputListeningTurnedOn = false;
            };
            _puzzle.OnFailed += data => { 
                if (data.broken)
                {
                    OnLockPickBroken?.Invoke();
                    SwitchBack();
                } 
                
                AudioManager.PostAudioEvent(AudioUI.UILockpickFail, gameObject);
            };
            _puzzle.OnEnteredAllowedArea += () => {
                AudioManager.PostAudioEvent(AudioUI.UILockpickMove, gameObject);
            };
            _puzzle.OnExitedAllowedArea += () => {
                AudioManager.PostAudioEvent(AudioUI.UILockpickMove, gameObject);
            };
        }

        public override void Update()
        {
            base.Update();

            if (!_inputListeningTurnedOn)
                return;
            
            var pickerRotation = _pickerImage.transform.localRotation.eulerAngles;
            Vector2 pointerDelta = InputManager.ReadInput<Vector2>(Controls.POINTER_DELTA);

            if (_puzzle.State == LockpickPuzzleState.DoingNothing && pointerDelta.y != 0)
            {
                var oldAngle = _lockpickAngle; 
                _lockpickAngle = _puzzle.Rotate(pointerDelta.y);
                
                var newRotation = Quaternion.identity;
                newRotation.eulerAngles = new Vector3(pickerRotation.x, pickerRotation.y, _lockpickAngle);
                _pickerImage.transform.localRotation = newRotation;
                
                _rotationBeforeSound += Mathf.Abs(_lockpickAngle - oldAngle);
                if (_rotationBeforeSound >= _soundThreshold)
                {
                    _rotationBeforeSound -= _soundThreshold;
                    AudioManager.PostAudioEvent(AudioUI.UILockpickMove, gameObject);
                }
            }
            
            if (_lmbAction.WasPressedThisFrame())
            {
                _puzzle.TryStartingApplyingPressure();
                AudioManager.PostAudioEvent(AudioUI.UILockpickStart, gameObject);
            }
            else if (_puzzle.State == LockpickPuzzleState.ApplyingPressure && _lmbAction.WasReleasedThisFrame())
            {
                _puzzle.StopApplyingPressure();
            }
        }

        public override void InteractStart()
        {
            if (!isActive)
            {
                DOTween.To(() => _canvasGroup.alpha, val => _canvasGroup.alpha = val, 1f, _easingOpacityDuration)
                    .OnComplete(() => _canvasGroup.alpha = 1)
                    .SetEase(_startingPuzzleOpacityCurve);

                _puzzle.OnPlayerStartedLockpicking();
                _inputListeningTurnedOn = true;
                base.InteractStart();
                _gameManager.SetBlur(true, true);
            }
        }
        
        protected override void SwitchBack()
        {
            if (isActive)
            {
                DOTween.To(() => _canvasGroup.alpha, val => _canvasGroup.alpha = val, 0, _easingOpacityDuration)
                    .OnComplete(() => _canvasGroup.alpha = 0)
                    .SetEase(_stoppingPuzzleOpacityCurve);
                
                _inputListeningTurnedOn = false;
                base.SwitchBack();
                _gameManager.SetBlur(false, true);
            }   
        }
    }
}