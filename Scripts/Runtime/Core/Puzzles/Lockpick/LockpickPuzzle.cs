using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Action = System.Action;

namespace HJ.Runtime
{
    public enum LockpickPuzzleState
    {
        DoingNothing,
        ApplyingPressure,
        Success
    }

    public record LockpickingFailed
    {
        public float damagePercent;
        public bool broken;
    }
    
    public class LockpickPuzzle : MonoBehaviour, ISaveable
    {
        /// <summary>
        /// Player tried to pick and was successful.
        /// </summary>
        public Action OnSuccess = delegate {};
        
        /// <summary>
        /// Player have just finished the puzzle successfully.
        /// </summary>
        public Action OnCompleteSuccess = delegate {};
        
        /// <summary>
        /// The puzzle resets on failed
        /// </summary>
        public Action<LockpickingFailed> OnFailed = delegate {};
        
        /// These could be useful for the sound cues
        public Action OnEnteredAllowedArea = delegate {};
        public Action OnExitedAllowedArea = delegate {};

        public LockpickPuzzleState State { get; private set; } = LockpickPuzzleState.DoingNothing;

        [SerializeField] private bool _debugMode;
        
        [Header("Configuration")]
        [SerializeField] private float _allowedAngleDegrees = 20f;
        
        /// <summary>
        /// A time-window when player should stop applying pressure to success.  
        /// </summary>
        [SerializeField] private Vector2 _goodPressureTimingSeconds = new(0, 1f);
        
        /// <summary>
        /// A time marker after which the player fails in case he was applying pressure for too long  
        /// </summary>
        [SerializeField] private float _applyingPressureForcedFailSeconds = 2f;
        
        [SerializeField] private float _pickRotationSpeed = 1f; 
        [SerializeField] private float _pickRotationSpeedInAllowedArea = .8f;
        [SerializeField] private bool _shouldPickingFailIfInDisallowedArea;
        
        [SerializeField] private List<int> _angles = new() { 60, -45, 0 };
        [SerializeField] private Vector2 _clampAngleDegrees = new(-70, 100);
        
        private int _currentTry;
        private float _lockpickAngle;
        private float _applyingPressureElapsed;
        private bool _wasInAllowedAreaLastFrame;

        [SerializeField] private float _lockPickHealth = 100f;
        [SerializeField] private float _maxLockPickDamage = 100f;
        [SerializeField] private AnimationCurve _lockpickDamageScalingToTheLeftOfTheWindow;
        [SerializeField] private AnimationCurve _lockpickDamageScalingToTheRightOfTheWindow;
        private float _lockPickRemainingHealth = 100f;
        
        private void Awake()
        {
            Assert.IsTrue(_goodPressureTimingSeconds.y > _goodPressureTimingSeconds.x);
            Assert.IsTrue(_applyingPressureForcedFailSeconds >= _goodPressureTimingSeconds.y);
        }
        
        /// <summary>
        /// This method is intended to be called upon starting lock picking. 
        /// </summary>
        public void OnPlayerStartedLockpicking()
        {
            ResetPuzzle();
        }

        private void ResetPuzzle()
        {
            _currentTry = 0;
            _lockpickAngle = 0f;
            State = LockpickPuzzleState.DoingNothing;
        }

        private bool DamageLockpick(float damage)
        {
            if (_debugMode)
                Debug.Log($"Damaged Lockpick for {damage} damage");
            
            _lockPickRemainingHealth -= damage;

            bool broken = _lockPickRemainingHealth <= 0;
            
            if (broken)
            {
                Debug.Log($"Lockpick broke");
                _lockPickRemainingHealth = _lockPickHealth;
            }

            return broken;
        }

        public bool TryStartingApplyingPressure()
        {
            if (State != LockpickPuzzleState.DoingNothing)
            {
                return false;
            }
            
            if (_shouldPickingFailIfInDisallowedArea && !IsInAllowedArea())
            {
                OnFailedPicking();
                return false;
            }

            _applyingPressureElapsed = 0f;
            State = LockpickPuzzleState.ApplyingPressure;
            return true;
        }

        public void StopApplyingPressure()
        {
            if (State != LockpickPuzzleState.ApplyingPressure) 
            {
                Debug.LogWarning(
                    "StopApplyingPressure() was intended to be called when _state = LockpickPuzzleState.ApplyingPressure"
                );
                return;
            }

            var hitTimingWindow = _goodPressureTimingSeconds.x <= _applyingPressureElapsed 
                                  && _applyingPressureElapsed <= _goodPressureTimingSeconds.y;
            
            if (hitTimingWindow)
                OnSuccessfullyPicked();
            else
                OnFailedPicking();
        }

        private void OnSuccessfullyPicked()
        {
            if (_debugMode)
                Debug.Log("Successful lockpicking try");
            
            OnSuccess?.Invoke();

            _currentTry += 1;
            State = LockpickPuzzleState.DoingNothing;
            
            if (_currentTry >= _angles.Count)
            {
                if (_debugMode)
                    Debug.Log("Lockpicking puzzle was finished successfully!");
                
                OnCompleteSuccess?.Invoke();
                State = LockpickPuzzleState.Success;
            }
        }

        private void OnFailedPicking()
        {
            if (_debugMode)
                Debug.Log("Failed a lockpicking try");
            
            _currentTry = 0;
            State = LockpickPuzzleState.DoingNothing;

            var damageFactor = GetDamageFactor();
            Assert.IsTrue(damageFactor >= 0);
            Assert.IsTrue(damageFactor <= 1);
            damageFactor = Mathf.Clamp(damageFactor, 0, 1);
            
            var damageAmount = _maxLockPickDamage * damageFactor;
            var damagePercent = damageAmount / _lockPickHealth;
            var broke = DamageLockpick(damageAmount);

            OnFailed?.Invoke(new LockpickingFailed {
                broken = broke,
                damagePercent = damagePercent
            });
        }

        private float GetDamageFactor()
        {
            var elapsed = _applyingPressureElapsed;
            var leftEnd = _goodPressureTimingSeconds.x;
            var rightStart = _goodPressureTimingSeconds.y;
            var rightEnd = _applyingPressureForcedFailSeconds;

            if (elapsed < leftEnd)
            {
                var c = (leftEnd - elapsed) / leftEnd;
                return _lockpickDamageScalingToTheLeftOfTheWindow.Evaluate(Mathf.Clamp(c, 0, 1));
            }

            if (elapsed > rightStart) 
            {
                var c = (elapsed - rightStart) / (rightEnd - rightStart);
                return _lockpickDamageScalingToTheRightOfTheWindow.Evaluate(Mathf.Clamp(c, 0, 1));
            }

            return 0;
        }

        private void Update()
        {
            if (State == LockpickPuzzleState.ApplyingPressure)
            {
                _applyingPressureElapsed += Time.deltaTime;

                if (_applyingPressureElapsed > _applyingPressureForcedFailSeconds) 
                    OnFailedPicking();
            }
        }

        public float Rotate(float rotationDelta)
        {
            if (State != LockpickPuzzleState.DoingNothing)
            {
                Debug.LogWarning(
                    "Lockpicking.Rotate() was intended to be called when _state = LockpickPuzzleState.DoingNothing"
                );
                return _lockpickAngle;
            }
            
            var speed = _pickRotationSpeed;
            if (IsInAllowedArea())
                speed = _pickRotationSpeedInAllowedArea;

            _lockpickAngle += rotationDelta * speed;
            _lockpickAngle = Mathf.Clamp(_lockpickAngle, _clampAngleDegrees.x, _clampAngleDegrees.y);

            if (IsInAllowedArea() && !_wasInAllowedAreaLastFrame)
            {
                OnEnteredAllowedArea();
                _wasInAllowedAreaLastFrame = true;
            }
            else if (!IsInAllowedArea() && _wasInAllowedAreaLastFrame)
            {
                OnExitedAllowedArea();
                _wasInAllowedAreaLastFrame = false;
            }
            
            return _lockpickAngle;
        }

        private bool IsInAllowedArea()
        {
            float left = _angles[_currentTry] - _allowedAngleDegrees / 2f;
            float right = _angles[_currentTry] + _allowedAngleDegrees / 2f;
            
            // NOTE(Hulvdan): This check probably has an error. Needs tests
            return left <= _lockpickAngle && _lockpickAngle <= right;
        }

        public StorableCollection OnSave()
        {
            StorableCollection storableCollection = new StorableCollection();
            
            storableCollection.Add("completed", State == LockpickPuzzleState.Success);

            return storableCollection;
        }

        public void OnLoad(JToken data)
        {
            if ((bool)data["completed"])
                State = LockpickPuzzleState.Success;
            else
                State = LockpickPuzzleState.DoingNothing;
        }
    }
}