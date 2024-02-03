using System;
using UnityEngine;
using HJ.Tools;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    [Serializable]
    public class DynamicSwitchable : DynamicObjectType
    {
        // limits
        [Tooltip("Limits that define the minimum/maximum angle at which the switchable can be switched.")]
        [SerializeField] private MinMax _switchLimits;
        [Tooltip("Angle at which an switchable is switched when the game is started.")]
        [SerializeField] private float _startingAngle;
        [Tooltip("Usually the axis that defines the switch direction. Most likely the Z or negative Z axis.")]
        [SerializeField] private Axis _limitsForward = Axis.Z;
        [Tooltip("Usually the axis that defines the higne joint. Most likely the Y-axis.")]
        [SerializeField] private Axis _limitsUpward = Axis.Y;

        // switchable properties
        [Tooltip("Handle parent object, usually the base object where the child is handle of switchable.")]
        [SerializeField] private Transform _rootObject;
        [Tooltip("The curve that defines the switch on speed for modifier. 0 = start to 1 = end.")]
        [SerializeField] private AnimationCurve _switchOnCurve = new(new(0, 1), new(1, 1));
        [Tooltip("The curve that defines the switch off speed for modifier. 0 = start to 1 = end.")]
        [SerializeField] private AnimationCurve _switchOffCurve = new(new(0, 1), new(1, 1));
        [Tooltip("Defines the switch speed of the switchable.")]
        [SerializeField] private float _switchSpeed = 1f;
        [Tooltip("Defines the damping of an switchable joint.")]
        [SerializeField] private float _damping = 1f;

        [Tooltip("Flip the switch direction, for example when the switchable is already switched on or the switch limits are flipped.")]
        [SerializeField] private bool _flipSwitchDirection = false;
        [Tooltip("Flip the mouse drag direction.")]
        [SerializeField] private bool _flipMouse = false;
        [Tooltip("Lock switchable when switched.")]
        [SerializeField] private bool _lockOnSwitch = true;
        [Tooltip("Show the switchable gizmos to visualize the limits.")]
        [SerializeField] private bool _showGizmos = true;

        // private
        private float _currentAngle;
        private float _targetAngle;
        private float _mouseSmooth;

        private bool _isSwitched;
        private bool _isMoving;
        private bool _isSwitchLocked;
        private bool _isSwitchSound;

        public override bool ShowGizmos => _showGizmos;

        public override bool IsOpened => _isSwitched;

        public override void OnDynamicInit()
        {
            if(InteractType == DynamicObject.InteractType.Dynamic)
            {
                _targetAngle = _startingAngle;
                _currentAngle = _startingAngle;
            }
            else if(InteractType == DynamicObject.InteractType.Mouse)
            {
                _currentAngle = _startingAngle;
            }
        }

        public override void OnDynamicStart(PlayerManager player)
        {
            if (_isSwitchLocked) return;
            if (!DynamicObject.IsLocked)
            {
                if (InteractType == DynamicObject.InteractType.Dynamic && !_isMoving)
                {
                    _isSwitched = !_isSwitched;
                    _targetAngle = _flipSwitchDirection
                        ? (_isSwitched ? _switchLimits.Max : _switchLimits.Min)
                        : (_isSwitched ? _switchLimits.Min : _switchLimits.Max);

                    if (_lockOnSwitch) _isSwitchLocked = true;
                }
                else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
                {
                    if (_isSwitched = !_isSwitched)
                    {
                        Animator.SetTrigger(DynamicObject.UseTrigger1);
                        DynamicObject.UseEvent1?.Invoke(); // on event
                    }
                    else
                    {
                        Animator.SetTrigger(DynamicObject.UseTrigger2);
                        DynamicObject.UseEvent2?.Invoke(); // off eevent
                    }

                    if (_lockOnSwitch) _isSwitchLocked = true;
                }
            }
            else
            {
                TryUnlock();
            }
        }

        public override void OnDynamicOpen()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic && !_isMoving)
            {
                _targetAngle = _flipSwitchDirection
                    ? _switchLimits.Max : _switchLimits.Min;

                _isSwitched = true;
                if (_lockOnSwitch) _isSwitchLocked = true;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.UseTrigger1);
                DynamicObject.UseEvent1?.Invoke();

                _isSwitched = true;
                if (_lockOnSwitch) _isSwitchLocked = true;
            }
        }

        public override void OnDynamicClose()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic && !_isMoving)
            {
                _targetAngle = _flipSwitchDirection
                    ? _switchLimits.Min : _switchLimits.Max;

                _isSwitched = false;
                if (_lockOnSwitch) _isSwitchLocked = true;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.UseTrigger2);
                DynamicObject.UseEvent2?.Invoke();

                _isSwitched = false;
                if (_lockOnSwitch) _isSwitchLocked = true;
            }
        }

        public override void OnDynamicUpdate()
        {
            float t = 0;

            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                t = Mathf.InverseLerp(_switchLimits.Min, _switchLimits.Max, _currentAngle);
                _isMoving = t > 0 && t < 1;

                float modifier = _isSwitched ? _switchOnCurve.Evaluate(t) : _switchOffCurve.Evaluate(1 - t);
                _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, Time.deltaTime * _switchSpeed * 10 * modifier);

                Vector3 axis = Quaternion.AngleAxis(_currentAngle, _rootObject.Direction(_limitsUpward)) * _rootObject.Direction(_limitsForward);
                Target.rotation = Quaternion.LookRotation(axis);
            }
            else if(InteractType == DynamicObject.InteractType.Mouse)
            {
                _mouseSmooth = Mathf.MoveTowards(_mouseSmooth, _targetAngle, Time.deltaTime * (_targetAngle != 0 ? _switchSpeed : _damping));
                _currentAngle = Mathf.Clamp(_currentAngle + _mouseSmooth, _switchLimits.RealMin, _switchLimits.RealMax);

                Vector3 axis = Quaternion.AngleAxis(_currentAngle, _rootObject.Direction(_limitsUpward)) * _rootObject.Direction(_limitsForward);
                Target.rotation = Quaternion.LookRotation(axis);

                t = Mathf.InverseLerp(_switchLimits.Min, _switchLimits.Max, _currentAngle);
                if(t > 0.99f && !_isSwitched) _isSwitched = true;
                else if(t < 0.01f && _isSwitched) _isSwitched = false;
            }

            if(InteractType != DynamicObject.InteractType.Animation)
            {
                if (_isSwitched && !_isSwitchSound && t > 0.99f)
                {
                    DynamicObject.UseEvent1?.Invoke(); // on event
                    DynamicObject.PlaySound(DynamicSoundType.Open);
                    _isSwitchSound = true;
                }
                else if (!_isSwitched && _isSwitchSound && t < 0.01f)
                {
                    DynamicObject.UseEvent2?.Invoke(); // off event
                    DynamicObject.PlaySound(DynamicSoundType.Close);
                    _isSwitchSound = false;
                }
            }

            // value change event
            DynamicObject.OnValueChange?.Invoke(t);
        }

        public override void OnDynamicHold(Vector2 mouseDelta)
        {
            if (InteractType == DynamicObject.InteractType.Mouse && !_isSwitchLocked)
            {
                mouseDelta.x = 0;
                float mouseInput = Mathf.Clamp(mouseDelta.y, -1, 1) * (_flipMouse ? 1 : -1);
                _targetAngle = mouseDelta.magnitude > 0 ? mouseInput : 0;
            }

            IsHolding = true;
        }

        public override void OnDynamicEnd()
        {
            if (InteractType == DynamicObject.InteractType.Mouse && !_isSwitchLocked)
            {
                _targetAngle = 0;
            }

            IsHolding = false;
        }

        public override void OnDrawGizmos()
        {
            if (DynamicObject == null || _rootObject == null || InteractType == DynamicObject.InteractType.Animation) return;

            Vector3 forward = _rootObject.Direction(_limitsForward);
            Vector3 upward = _rootObject.Direction(_limitsUpward);
            HandlesDrawing.DrawLimits(_rootObject.position, _switchLimits, forward, upward, true, radius: 0.25f);

            Vector3 from = Quaternion.AngleAxis(_switchLimits.Min - _switchLimits.Min, upward) * forward;
            Quaternion angleRotation = Quaternion.AngleAxis(_startingAngle, upward);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(_rootObject.position, angleRotation * from * 0.25f);
        }

        public override StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();

            if (InteractType != DynamicObject.InteractType.Animation)
            {
                saveableBuffer.Add("rotation", Target.eulerAngles.ToSaveable());
                saveableBuffer.Add("angle", _currentAngle);
                saveableBuffer.Add(nameof(_isSwitchSound), _isSwitchSound);
                saveableBuffer.Add(nameof(_isSwitchLocked), _isSwitchLocked);
            }

            saveableBuffer.Add(nameof(_isSwitched), _isSwitched);
            return saveableBuffer;
        }

        public override void OnLoad(JToken token)
        {
            if (InteractType != DynamicObject.InteractType.Animation)
            {
                Target.eulerAngles = token["rotation"].ToObject<Vector3>();
                _currentAngle = (float)token["angle"];
                _isSwitchSound = (bool)token[nameof(_isSwitchSound)];
                _isSwitchLocked = (bool)token[nameof(_isSwitchLocked)];
            }

            _isSwitched = (bool)token[nameof(_isSwitched)];
        }
    }
}