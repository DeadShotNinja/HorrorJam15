using System;
using UnityEngine;
using HJ.Tools;
using Newtonsoft.Json.Linq;
using System.Collections;
using TMPro;

namespace HJ.Runtime
{
    [Serializable]
    public class DynamicOpenable : DynamicObjectType
    {
        // limits
        [Tooltip("Limits that define the minimum/maximum angle at which the openable can be opened.")]
        [SerializeField] private MinMax _openLimits;
        [Tooltip("Angle at which an openable is opened when the game is started.")]
        [SerializeField] private float _startingAngle;
        [Tooltip("Usually the axis that defines the open direction. Most likely the Z or negative Z axis.")]
        [SerializeField] private Axis _limitsForward = Axis.Z;
        [Tooltip("Usually the axis that defines the higne joint. Most likely the Y-axis.")]
        [SerializeField] private Axis _limitsUpward = Axis.Y;

        // openable properties
        [Tooltip("The curve that defines the opening speed for modifier. 0 = start to 1 = end.")]
        [SerializeField] private AnimationCurve _openCurve = new(new(0, 1), new(1, 1));
        [Tooltip("The curve that defines the closing speed for modifier. 0 = start to 1 = end.")]
        [SerializeField] private AnimationCurve _closeCurve = new(new(0, 1), new(1, 1));
        [Tooltip("Usually the axis that determines the forward direction of the frame. The direction is used to determine in which direction the door should open. Usually the same axis as the limits forward axis.")]
        [SerializeField] private Axis _openableForward = Axis.Z;
        [Tooltip("Usually the axis that defines the hinge joint. It will help to define where the top is when the openable is flipping at an angle below 0.")]
        [SerializeField] private Axis _openableUp = Axis.Y;

        [Tooltip("Defines the open/close speed of the openable.")]
        [SerializeField] private float _openSpeed = 1f;
        [Tooltip("Defines the damping of an openable joint.")]
        [SerializeField] private float _damper = 1f;
        [Tooltip("Defines the minimum volume at which the open/close motion sound will be played.")]
        [SerializeField] private float _dragSoundPlay = 0.2f;

        [Tooltip("Flip the open direction, for example when the openable is already opened or the open limits are flipped.")]
        [SerializeField] private bool _flipOpenDirection = false;
        [Tooltip("Flip the forward direction, for example when the openable gizmo is pointing in the wrong direction.")]
        [SerializeField] private bool _flipForwardDirection = false;
        [Tooltip("Use the upward direction to determine where the openable up is pointing.")]
        [SerializeField] private bool _useUpwardDirection = false;
        [Tooltip("Defines when the openable can be opened on both sides.")]
        [SerializeField] private bool _bothSidesOpen = false;
        [Tooltip("Allows to use drag sounds.")]
        [SerializeField] private bool _dragSounds = false;
        [Tooltip("Play sound when the openable is closed.")]
        [SerializeField] private bool _playCloseSound = true;
        [Tooltip("Flip the mouse drag direction.")]
        [SerializeField] private bool _flipMouse = false;
        [Tooltip("Flip the openable min/max limits.")]
        [SerializeField] private bool _flipValue = false;
        [Tooltip("Show the openable gizmos to visualize the limits.")]
        [SerializeField] private bool _showGizmos = true;

        [SerializeField] private bool _useLockedMotion = false;
        [SerializeField] private AnimationCurve _lockedPattern = new(new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField] private float _lockedMotionAmount;
        [SerializeField] private float _lockedMotionTime;

        // private
        private float _currentAngle;
        private float _targetAngle;
        private float _openAngle;
        private float _prevAngle;

        private bool _isOpened;
        private bool _isMoving;
        private bool _isOpenSound;
        private bool _isCloseSound;
        private bool _isLockedTry;
        private bool _disableSounds;

        private Vector3 _limitsFwd;
        private Vector3 _limitsUpwd;
        private Vector3 _openableFwd;

        public override bool ShowGizmos => _showGizmos;

        public override bool IsOpened => _isOpened;

        public bool DragSounds
        {
            get => _dragSounds;
            set => _dragSounds = value;
        }

        public bool BothSidesOpen
        {
            get => _bothSidesOpen;
            set => _bothSidesOpen = value;
        }

        public override void OnDynamicInit()
        {
            _limitsFwd = Target.Direction(_limitsForward);
            _limitsUpwd = Target.Direction(_limitsUpward);
            _openableFwd = Target.Direction(_openableForward);

            if (InteractType == DynamicObject.InteractType.Mouse && Joint != null)
            {
                // configure joint limits
                JointLimits limits = Joint.limits;
                limits.min = _openLimits.Min;
                limits.max = _openLimits.Max;
                Joint.limits = limits;

                // configure joint spring
                JointSpring spring = Joint.spring;
                spring.damper = _damper;
                Joint.spring = spring;

                // configure joint motor
                JointMotor motor = Joint.motor;
                motor.force = 1f;
                Joint.motor = motor;

                // enable/disable joint features
                Joint.useSpring = true;
                Joint.useLimits = true;
                Joint.useMotor = false;

                // configure joint axis and rigidbody
                Joint.axis = _openableUp.Convert();
                Rigidbody.isKinematic = false;
                Rigidbody.useGravity = true;
            }

            if(InteractType != DynamicObject.InteractType.Animation)
            {
                SetOpenableAngle(_startingAngle);

                _targetAngle = _startingAngle;
                _currentAngle = _startingAngle;
                _openAngle = _startingAngle;

                float mid = Mathf.Lerp(_openLimits.Min, _openLimits.Max, 0.5f);
                _disableSounds = Mathf.Abs(_startingAngle) > Mathf.Abs(mid);
                _isOpenSound = _disableSounds;
            }
        }

        public override void OnDynamicStart(PlayerManager player)
        {
            if (DynamicObject.IsLocked)
            {
                TryUnlock();
                return;
            }

            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                if (_isMoving) 
                    return;

                if (_bothSidesOpen)
                {
                    float lookDirection = Vector3.Dot(_openableFwd, player.MainCamera.transform.forward);
                    _prevAngle = _openAngle;
                    _openAngle = _targetAngle = (_isOpened = !_isOpened)
                        ? _flipOpenDirection
                            ? (lookDirection > 0 ? _openLimits.Max : _openLimits.Min)
                            : (lookDirection > 0 ? _openLimits.Min : _openLimits.Max)
                        : 0;
                }
                else
                {
                    _prevAngle = _openAngle;
                    _openAngle = _targetAngle = _flipOpenDirection
                        ? (_isOpened ? _openLimits.Max : _openLimits.Min)
                        : (_isOpened ? _openLimits.Min : _openLimits.Max);
                    _isOpened = !_isOpened;
                }
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                if (_isOpened = !_isOpened)
                {
                    if (_bothSidesOpen)
                    {
                        float lookDirection = Vector3.Dot(_openableFwd, player.MainCamera.transform.forward);
                        Animator.SetBool(DynamicObject.UseTrigger3, Mathf.RoundToInt(lookDirection) > 0);
                    }

                    Animator.SetTrigger(DynamicObject.UseTrigger1);
                    DynamicObject.PlaySound(DynamicSoundType.Open);
                    DynamicObject.UseEvent1?.Invoke();  // open event
                }
                else
                {
                    Animator.SetTrigger(DynamicObject.UseTrigger2);
                    DynamicObject.UseEvent2?.Invoke(); // close event
                    _isCloseSound = true;
                }
            }

            if (_disableSounds) _disableSounds = false;
        }

        public override void OnDynamicOpen()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                if (_isMoving)
                    return;

                _prevAngle = _openAngle;
                _openAngle = _targetAngle = _flipOpenDirection
                    ? _openLimits.Min : _openLimits.Max;
                _isOpened = true;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.UseTrigger1);
                DynamicObject.PlaySound(DynamicSoundType.Open);
                DynamicObject.UseEvent1?.Invoke();
                _isOpened = true;
            }
        }

        public override void OnDynamicClose()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                if (_isMoving)
                    return;

                _prevAngle = _openAngle;
                _openAngle = _targetAngle = _flipOpenDirection
                    ? _openLimits.Max : _openLimits.Min;
                _isOpened = false;
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.UseTrigger2);
                DynamicObject.UseEvent2?.Invoke();
                _isCloseSound = true;
                _isOpened = false;
            }
        }

        public override void OnDynamicLocked()
        {
            if (_isLockedTry || !_useLockedMotion)
                return;

            DynamicObject.StartCoroutine(OnLocked());
            _isLockedTry = true;
        }

        IEnumerator OnLocked()
        {
            float elapsedTime = 0f;

            while (elapsedTime < _lockedMotionTime)
            {
                float t = elapsedTime / _lockedMotionTime;
                float pattern = _lockedPattern.Evaluate(t) * _lockedMotionAmount;
                SetOpenableAngle(_currentAngle + pattern);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            SetOpenableAngle(_currentAngle);
            _isLockedTry = false;
        }

        public override void OnDynamicUpdate()
        {
            float t = 0;

            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                t = Mathf.InverseLerp(_prevAngle, _openAngle, _currentAngle);
                DynamicObject.OnValueChange?.Invoke(t);
                _isMoving = t > 0 && t < 1;

                float modifier = _isOpened ? _openCurve.Evaluate(t) : _closeCurve.Evaluate(t);
                _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, Time.deltaTime * _openSpeed * 10 * modifier);
                SetOpenableAngle(_currentAngle);

                if (!_disableSounds)
                {
                    if (_isOpened && !_isOpenSound && t > 0.02f)
                    {
                        DynamicObject.PlaySound(DynamicSoundType.Open);
                        DynamicObject.UseEvent1?.Invoke(); // open event
                        _isOpenSound = true;
                    }
                    else if (!_isOpened && _isOpenSound && t > 0.95f)
                    {
                        DynamicObject.PlaySound(DynamicSoundType.Close);
                        DynamicObject.UseEvent2?.Invoke(); // close event
                        _isOpenSound = false;
                    }
                }
            }
            else if(InteractType == DynamicObject.InteractType.Mouse)
            {
                Vector3 minDir = Quaternion.AngleAxis(_openLimits.Min, _limitsUpwd) * _limitsFwd;
                Vector3 maxDir = Quaternion.AngleAxis(_openLimits.Max, _limitsUpwd) * _limitsFwd;

                Vector3 newMin = _flipValue ? maxDir : minDir;
                Vector3 newMax = _flipValue ? minDir : maxDir;

                Vector3 forward = Target.Direction(_openableForward);
                t = VectorExtension.InverseLerp(newMin, newMax, forward);
                DynamicObject.OnValueChange?.Invoke(t);

                if (!_disableSounds)
                {
                    if (!_isOpened && t > 0.02f)
                    {
                        DynamicObject.PlaySound(DynamicSoundType.Open);
                        DynamicObject.UseEvent1?.Invoke(); // open event
                        _isOpened = true;
                    }
                    else if (_isOpened && t < 0.01f)
                    {
                        DynamicObject.PlaySound(DynamicSoundType.Close);
                        DynamicObject.UseEvent2?.Invoke(); // close event
                        _isOpened = false;
                    }
                }

                //if (dragSounds)
                //{
                //    float angle = Target.localEulerAngles.Component(openableUp).FixAngle(openLimits.Min, openLimits.Max);
                //    float volumeMag = Mathf.Clamp01(Rigidbody.velocity.magnitude);

                //    if (volumeMag > dragSoundPlay && ((Vector2)openLimits).InRange(angle))
                //    {
                //        _audioSource.SetSoundClip(dragSound, volumeMag, true);
                //    }
                //    else
                //    {
                //        if (_audioSource.volume > 0.01f)
                //        {
                //            _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, 0f, Time.deltaTime);
                //        }
                //        else
                //        {
                //            _audioSource.volume = 0f;
                //            _audioSource.Stop();
                //        }
                //    }
                //}
            }

            if(InteractType != DynamicObject.InteractType.Animation)
            {
                // value change event
                DynamicObject.OnValueChange?.Invoke(t);
            }
            else if(_playCloseSound && !_isOpened && _isCloseSound && !Animator.IsAnyPlaying())
            {
                DynamicObject.PlaySound(DynamicSoundType.Close);
                _isCloseSound = false;
            }
        }

        private void SetOpenableAngle(float angle)
        {
            Vector3 upward = Target.Direction(_openableUp);
            int flipForward = _flipForwardDirection ? -1 : 1;

            Vector3 axis = Quaternion.AngleAxis(angle, _limitsUpwd) * _limitsFwd * flipForward;
            if (_useUpwardDirection) Target.rotation = Quaternion.LookRotation(axis, upward);
            else Target.rotation = Quaternion.LookRotation(axis);
        }

        public override void OnDynamicHold(Vector2 mouseDelta)
        {
            if (InteractType == DynamicObject.InteractType.Mouse && Joint != null)
            {
                mouseDelta.y = 0;
                if (mouseDelta.magnitude > 0)
                {
                    Joint.useMotor = true;
                    JointMotor motor = Joint.motor;
                    motor.targetVelocity = mouseDelta.x * _openSpeed * 10 * (_flipMouse ? -1 : 1);
                    Joint.motor = motor;
                }
                else
                {
                    Joint.useMotor = false;
                    JointMotor motor = Joint.motor;
                    motor.targetVelocity = 0f;
                    Joint.motor = motor;
                }
            }

            IsHolding = true;
        }

        public override void OnDynamicEnd()
        {
            if (InteractType == DynamicObject.InteractType.Mouse && Joint != null)
            {
                Joint.useMotor = false;
                JointMotor motor = Joint.motor;
                motor.targetVelocity = 0f;
                Joint.motor = motor;
            }

            IsHolding = false;
        }

        public override void OnDrawGizmos()
        {
            if (DynamicObject == null || Target == null || InteractType == DynamicObject.InteractType.Animation) return;

            Vector3 forward = Application.isPlaying ? _limitsFwd : Target.Direction(_limitsForward);
            Vector3 upward = Application.isPlaying ? _limitsUpwd : Target.Direction(_limitsUpward);
            forward = Quaternion.Euler(0, -90, 0) * forward;
            float radius = 0.3f;

            HandlesDrawing.DrawLimits(DynamicObject.transform.position, _openLimits, forward, upward, true, _flipOpenDirection, radius);

            Vector3 startingDir = Quaternion.AngleAxis(_startingAngle, upward) * forward;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(Target.position, startingDir * radius);
        }

        public override StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();
            saveableBuffer.Add("rotation", Target.eulerAngles.ToSaveable());

            if (InteractType != DynamicObject.InteractType.Animation)
            {
                saveableBuffer.Add("targetAngle", _targetAngle);
                saveableBuffer.Add("currentAngle", _currentAngle);
                saveableBuffer.Add("openAngle", _openAngle);
                saveableBuffer.Add("isOpenSound", _isOpenSound);
                saveableBuffer.Add("disableSounds", _disableSounds);
                saveableBuffer.Add("isOpened", _isOpened);
            }

            return saveableBuffer;
        }

        public override void OnLoad(JToken token)
        {
            Target.eulerAngles = token["rotation"].ToObject<Vector3>();

            if (InteractType != DynamicObject.InteractType.Animation)
            {
                _targetAngle = (float)token["targetAngle"];
                _currentAngle = (float)token["currentAngle"];
                _openAngle = (float)token["openAngle"];
                _isOpenSound = (bool)token["isOpenSound"];
                _disableSounds = (bool)token["disableSounds"];
                _isOpened = (bool)token["isOpened"];
            }
        }
    }
}