using System;
using UnityEngine;
using HJ.Tools;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    [Serializable]
    public class DynamicPullable : DynamicObjectType
    {
        [Tooltip("Limits that define the minimum/maximum position in which the pullable can be pulled.")]
        [SerializeField] private MinMax _openLimits;
        [Tooltip("The axis in which the object is to be pulled.")]
        [SerializeField] private Axis _pullAxis = Axis.Z;

        // pullable properties
        [Tooltip("The curve that defines the pull speed for modifier. 0 = start to 1 = end.")]
        [SerializeField] private AnimationCurve _openCurve = new(new(0, 1), new(1, 1));
        [Tooltip("Defines the pulling speed.")]
        [SerializeField] private float _openSpeed = 1f;
        [Tooltip("Defines the damping of the pullable object.")]
        [SerializeField] private float _damping = 1f;
        [Tooltip("Defines the minimum mouse input at which to play the drag sound.")]
        [SerializeField] private float _dragSoundPlay = 0.2f;

        [Tooltip("Enable pull sound when dragging the pullable with mouse.")]
        [SerializeField] private bool _dragSounds = true;
        [Tooltip("Flip the mouse drag direction.")]
        [SerializeField] private bool _flipMouse = false;

        // private
        private Vector3 _targetPosition;
        private Vector3 _startPosition;

        private float _targetMove;
        private float _mouseSmooth;

        private bool _isOpened;
        private bool _isMoving;

        public override bool IsOpened => _isOpened;

        public override void OnDynamicInit()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                _startPosition = Target.localPosition.SetComponent(_pullAxis, _openLimits.Min);
                _targetPosition = _startPosition;
            }
        }

        public override void OnDynamicStart(PlayerManager player)
        {
            if (!DynamicObject.IsLocked)
            {
                if (InteractType == DynamicObject.InteractType.Dynamic && !_isMoving)
                {
                    if (_isOpened = !_isOpened)
                    {
                        _targetMove = _openLimits.Max;
                        DynamicObject.PlaySound(DynamicSoundType.Open);
                        DynamicObject.UseEvent1?.Invoke();  // open event
                    }
                    else
                    {
                        _targetMove = _openLimits.Min;
                        DynamicObject.PlaySound(DynamicSoundType.Close);
                        DynamicObject.UseEvent2?.Invoke();  // close event
                    }

                    _targetPosition = _startPosition.SetComponent(_pullAxis, _targetMove);
                }
                else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
                {
                    if (_isOpened = !_isOpened)
                    {
                        Animator.SetTrigger(DynamicObject.UseTrigger1);
                        DynamicObject.PlaySound(DynamicSoundType.Open);
                        DynamicObject.UseEvent1?.Invoke();  // open event
                    }
                    else
                    {
                        Animator.SetTrigger(DynamicObject.UseTrigger2);
                        DynamicObject.PlaySound(DynamicSoundType.Close);
                        DynamicObject.UseEvent2?.Invoke();  // close event
                    }
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
                _targetMove = _openLimits.Min;
                DynamicObject.PlaySound(DynamicSoundType.Close);
                DynamicObject.UseEvent2?.Invoke();

                _isOpened = true;
                _targetPosition = _startPosition.SetComponent(_pullAxis, _targetMove);
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.UseTrigger2);
                DynamicObject.PlaySound(DynamicSoundType.Close);
                DynamicObject.UseEvent2?.Invoke();

                _isOpened = true;
            }
        }

        public override void OnDynamicClose()
        {
            if (InteractType == DynamicObject.InteractType.Dynamic && !_isMoving)
            {
                _targetMove = _openLimits.Max;
                DynamicObject.PlaySound(DynamicSoundType.Open);
                DynamicObject.UseEvent1?.Invoke();

                _isOpened = true;
                _targetPosition = _startPosition.SetComponent(_pullAxis, _targetMove);
            }
            else if (InteractType == DynamicObject.InteractType.Animation && !Animator.IsAnyPlaying())
            {
                Animator.SetTrigger(DynamicObject.UseTrigger1);
                DynamicObject.PlaySound(DynamicSoundType.Open);
                DynamicObject.UseEvent1?.Invoke();

                _isOpened = true;
            }
        }

        public override void OnDynamicUpdate()
        {
            float t = 0;

            if (InteractType == DynamicObject.InteractType.Dynamic)
            {
                float currentAxisPos = Target.localPosition.Component(_pullAxis);
                t = Mathf.InverseLerp(_openLimits.Min, _openLimits.Max, currentAxisPos);

                _isMoving = t > 0 && t < 1;
                float modifier = _openCurve.Evaluate(t);

                Vector3 currentPos = Target.localPosition;
                currentPos = Vector3.MoveTowards(currentPos, _targetPosition, Time.deltaTime * _openSpeed * modifier);
                Target.localPosition = currentPos;
            }
            else if (InteractType == DynamicObject.InteractType.Mouse)
            {
                _mouseSmooth = Mathf.MoveTowards(_mouseSmooth, _targetMove, Time.deltaTime * (_targetMove != 0 ? _openSpeed : _damping));
                Target.Translate(_mouseSmooth * Time.deltaTime * _pullAxis.Convert(), Space.Self);

                Vector3 clampedPosition = Target.localPosition.Clamp(_pullAxis, _openLimits);
                Target.localPosition = clampedPosition;

                float currentAxisPos = Target.localPosition.Component(_pullAxis);
                t = Mathf.InverseLerp(_openLimits.Min, _openLimits.Max, currentAxisPos);

                if(t > 0.99f && !_isOpened)
                {
                    DynamicObject.UseEvent1?.Invoke();  // open event
                    _isOpened = true;
                }
                else if(t < 0.01f && _isOpened)
                {
                    DynamicObject.UseEvent2?.Invoke();  // close event
                    _isOpened = false;
                }

                // TODO: Wwise integration
                // if (dragSounds && AudioSource != null)
                // {
                //     if (mouseSmooth > dragSoundPlay && currentAxisPos < openLimits.max)
                //     {
                //         AudioSource.SetSoundClip(DynamicObject.UseSound1);
                //         if (!AudioSource.isPlaying) AudioSource.Play();
                //     }
                //     else if (mouseSmooth < -dragSoundPlay && currentAxisPos > openLimits.min)
                //     {
                //         AudioSource.SetSoundClip(DynamicObject.UseSound2);
                //         if (!AudioSource.isPlaying) AudioSource.Play();
                //     }
                //     else
                //     {
                //         if(AudioSource.volume > 0.01f)
                //         {
                //             AudioSource.volume = Mathf.MoveTowards(AudioSource.volume, 0f, Time.deltaTime * 4f);
                //         }
                //         else
                //         {
                //             AudioSource.volume = 0f;
                //             AudioSource.Stop();
                //         }
                //     }
                // }
            }

            // value change event
            DynamicObject.OnValueChange?.Invoke(t);
        }

        public override void OnDynamicHold(Vector2 mouseDelta)
        {
            if (InteractType == DynamicObject.InteractType.Mouse)
            {
                mouseDelta.x = 0;
                float mouseInput = Mathf.Clamp(mouseDelta.y, -1, 1) * (_flipMouse ? 1 : -1);
                _targetMove = mouseDelta.magnitude > 0 ? mouseInput : 0;
            }

            IsHolding = true;
        }

        public override void OnDynamicEnd()
        {
            if (InteractType == DynamicObject.InteractType.Mouse)
            {
                _targetMove = 0;
            }

            IsHolding = false;
        }

        public override StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();

            if(InteractType != DynamicObject.InteractType.Animation)
                saveableBuffer.Add("localPosition", Target.localPosition.ToSaveable());

            saveableBuffer.Add(nameof(_isOpened), _isOpened);
            return saveableBuffer;
        }

        public override void OnLoad(JToken token)
        {
            if (InteractType != DynamicObject.InteractType.Animation)
                Target.localPosition = token["localPosition"].ToObject<Vector3>();

            _isOpened = (bool)token[nameof(_isOpened)];
        }
    }
}