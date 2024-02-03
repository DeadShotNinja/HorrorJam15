using System;
using UnityEngine;
using HJ.Tools;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    [Serializable]
    public class DynamicRotable : DynamicObjectType
    {
        // limits
        [Tooltip("The maximum limit at which rotable can be rotated.")]
        [SerializeField] private float _rotationLimit = 360f;
        [Tooltip("The axis around which to rotate.")]
        [SerializeField] private Axis _rotateAroundAxis = Axis.Z;
        [Tooltip("Rotation offset when the rotatable object has an incorrect rotation.")]
        [SerializeField] private Vector3 _rotationOffset = Vector3.zero;

        // rotable properties
        [Tooltip("The curve that defines the rotable speed for modifier. 0 = start to 1 = end.")]
        [SerializeField] private AnimationCurve _rotateCurve = new(new(0, 1), new(1, 1));
        [Tooltip("Defines the rotation speed.")]
        [SerializeField] private float _rotationSpeed = 2f;
        [Tooltip("Mouse multiplier to adjust mouse input.")]
        [SerializeField] private float _mouseMultiplier = 1f;
        [Tooltip("Defines the damping of the rotable object.")]
        [SerializeField] private float _damping = 1f;

        [Tooltip("Hold use button to rotate the object.")]
        [SerializeField] private bool _holdToRotate = true;
        [Tooltip("When the maximum limit is reached, lock the rotable object.")]
        [SerializeField] private bool _lockOnRotate = false;
        [Tooltip("Show the rotable gizmos to visualize the limits.")]
        [SerializeField] private bool _showGizmos = true;

        // private
        private float _currentAngle;
        private float _targetAngle;

        private float _mouseSmooth;
        private float _targetMove;

        private bool _isHolding;
        private bool _isRotated;
        private bool _isMoving;
        private bool _isRotateLocked;
        private bool _isTurnSound;

        private Vector3 _rotableForward;

        public override bool ShowGizmos => _showGizmos;

        public override bool IsOpened => _isRotated;

        public override void OnDynamicInit()
        {
            _rotableForward = Target.Direction(_rotateAroundAxis);
            _targetAngle = _rotationLimit;
        }

        public override void OnDynamicStart(PlayerManager player)
        {
            if (_isRotateLocked) return;

            if (!DynamicObject.IsLocked)
            {
                if (InteractType == DynamicObject.InteractType.Dynamic)
                {
                    if (_lockOnRotate)
                    {
                        _targetAngle = _rotationLimit;
                        DynamicObject.UseEvent1?.Invoke();  // rotate on event
                    }
                    else if (!_isMoving)
                    {
                        if(_isRotated = !_isRotated)
                        {
                            _targetAngle = _rotationLimit;
                            DynamicObject.UseEvent1?.Invoke();  // rotate on event
                        }
                        else
                        {
                            _targetAngle = 0;
                            DynamicObject.UseEvent2?.Invoke();  // rotate off event
                        }
                    }

                    _isHolding = true;
                }
                else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
                {
                    if (_isRotated = !_isRotated)
                    {
                        Animator.SetTrigger(DynamicObject.UseTrigger1);
                        DynamicObject.UseEvent1?.Invoke();  // rotate on event
                    }
                    else
                    {
                        Animator.SetTrigger(DynamicObject.UseTrigger2);
                        DynamicObject.UseEvent2?.Invoke();  // rotate off event
                    }

                    if (_lockOnRotate) _isRotateLocked = true;
                }
            }
            else
            {
                TryUnlock();
            }
        }

        public override void OnDynamicOpen()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                _targetAngle = _rotationLimit;
                DynamicObject.UseEvent1?.Invoke();
                _isRotated = true;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.UseTrigger1);
                DynamicObject.UseEvent1?.Invoke();  // rotate on event

                _isRotated = true;
                if (_lockOnRotate) _isRotateLocked = true;
            }
        }

        public override void OnDynamicClose()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                _targetAngle = 0;
                DynamicObject.UseEvent2?.Invoke();
                _isRotated = false;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.UseTrigger2);
                DynamicObject.UseEvent2?.Invoke();

                _isRotated = false;
                if (_lockOnRotate) _isRotateLocked = true;
            }
        }

        public override void OnDynamicUpdate()
        {
            float t = 0;

            if(InteractType == DynamicObject.InteractType.Dynamic)
            {
                t = Mathf.InverseLerp(0, _rotationLimit, _currentAngle);
                _isMoving = t > 0 && t < 1;

                float modifier = _rotateCurve.Evaluate(t);
                if ((_holdToRotate && _isHolding) || !_holdToRotate) 
                    _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, Time.deltaTime * _rotationSpeed * 10 * modifier);

                Quaternion rotation = Quaternion.AngleAxis(_currentAngle, _rotableForward);
                Target.rotation = rotation * Quaternion.Euler(_rotationOffset);
            }
            else if(InteractType == DynamicObject.InteractType.Mouse)
            {
                t = Mathf.InverseLerp(0, _rotationLimit, _currentAngle);
                if (_lockOnRotate) { if (t >= 1) _isRotateLocked = true; }

                _mouseSmooth = Mathf.MoveTowards(_mouseSmooth, _targetMove, Time.deltaTime * (_targetMove != 0 ? _rotationSpeed : _damping));
                _currentAngle = Mathf.Clamp(_currentAngle + _mouseSmooth, 0, _rotationLimit);

                Quaternion rotation = Quaternion.AngleAxis(_currentAngle, _rotableForward);
                Target.rotation = rotation * Quaternion.Euler(_rotationOffset);
            }

            if(InteractType != DynamicObject.InteractType.Animation)
            {
                if (t > 0.99f && !_isRotated)
                {
                    DynamicObject.UseEvent1?.Invoke();  // rotate on event
                    _isRotated = true;
                }
                else if (t < 0.01f && _isRotated)
                {
                    DynamicObject.UseEvent2?.Invoke();  // rotate off event
                    _isRotated = false;
                }

                if(t > 0.05f && !_isTurnSound && !_isRotated)
                {
                    DynamicObject.PlaySound(DynamicSoundType.Open);
                    _isTurnSound = true;
                }
                else if(t < 0.95f && _isTurnSound && _isRotated)
                {
                    DynamicObject.PlaySound(DynamicSoundType.Close);
                    _isTurnSound = false;
                }

                // value change event
                DynamicObject.OnValueChange?.Invoke(t);
            }
        }

        public override void OnDynamicHold(Vector2 mouseDelta)
        {
            if(InteractType == DynamicObject.InteractType.Mouse && !_isRotateLocked)
            {
                mouseDelta.x = 0;
                float mouseInput = Mathf.Clamp(mouseDelta.y, -1, 1) * _mouseMultiplier;
                _targetMove = mouseDelta.magnitude > 0 ? mouseInput : 0;
            }

            IsHolding = true;
        }

        public override void OnDynamicEnd()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                _isHolding = false;
            }
            else if (InteractType == DynamicObject.InteractType.Mouse)
            {
                _targetMove = 0;
            }

            IsHolding = false;
        }

        public override void OnDrawGizmos()
        {
            if (DynamicObject == null || Target == null || InteractType == DynamicObject.InteractType.Animation) return;

            Vector3 forward = Application.isPlaying ? _rotableForward : Target.Direction(_rotateAroundAxis);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Target.position, forward * 0.1f);
        }

        public override StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();

            if (InteractType != DynamicObject.InteractType.Animation)
            {
                saveableBuffer.Add("rotation", Target.eulerAngles.ToSaveable());
                saveableBuffer.Add("angle", _currentAngle);
                saveableBuffer.Add(nameof(_isTurnSound), _isTurnSound);
            }

            saveableBuffer.Add(nameof(_isRotateLocked), _isRotateLocked);
            saveableBuffer.Add(nameof(_isRotated), _isRotated);
            return saveableBuffer;
        }

        public override void OnLoad(JToken token)
        {
            if (InteractType != DynamicObject.InteractType.Animation)
            {
                Target.eulerAngles = token["rotation"].ToObject<Vector3>();
                _currentAngle = (float)token["angle"];
                _isTurnSound = (bool)token[nameof(_isTurnSound)];
            }

            _isRotateLocked = (bool)token[nameof(_isRotateLocked)];
            _isRotated = (bool)token[nameof(_isRotated)];
        }
    }
}