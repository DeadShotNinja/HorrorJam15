using UnityEngine;
using HJ.Scriptable;
using HJ.Input;
using HJ.Tools;
using static HJ.Runtime.MovableObject;

namespace HJ.Runtime.States
{
    public class PushingStateAsset : PlayerStateAsset
    {
        [SerializeField] private PushingStateData _stateData;

        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new PushingPlayerState(machine, _stateData);
        }

        public override string GetStateKey() => PlayerStateMachine.PUSHING_STATE;

        public override string ToString() => "Pushing";

        public class PushingPlayerState : FSMPlayerState
        {
            private readonly PushingStateData _data;

            private BoxCollider _collider;
            private Collider _interactCollider;
            private LayerMask _collisionMask;

            private MovableObject _movableObject;
            private Transform _movable;

            private Axis _forwardAxis;
            private MoveDirectionEnum _direction;
            private Vector3 _holdOffset;
            private bool _allowRotation;

            private bool _useLimits;
            private MinMax _verticalLimits;

            // TODO: this was used for testing, need to convert to Wwise
            // private AudioSource audioSource;
            private float _slidingVolume;
            private float _volumeFadeSpeed;

            private float _holdDistance;
            private float _oldSensitivity;

            private Vector3 _startingPosition;
            private Vector3 _targetPosition;
            private Vector2 _targetLook;
            private bool _isMoved;

            private float _movementSpeed;
            private float _prevRotationX;

            private float _lerpFactor = 0f;
            private float _tVel;

            private Vector3 CameraForward => _cameraLook.RotationX * Vector3.forward;

            public PushingPlayerState(PlayerStateMachine machine, PushingStateData data) : base(machine)
            {
                _data = data;
                _data.ControlExit.SubscribeGloc();
            }

            public override void OnStateEnter()
            {
                _movableObject = (MovableObject)StateData["reference"];
                //audioSource = movableObject.AudioSource;
                _movable = _movableObject.RootMovable;
                _collider = _movable.GetComponent<BoxCollider>();

                if (_movableObject.TryGetComponent(out _interactCollider))
                    _interactCollider.enabled = false;

                _forwardAxis = _movableObject.ForwardAxis;
                _collisionMask = _movableObject.CollisionMask;
                _direction = _movableObject.MoveDirection;
                _holdOffset = _movableObject.HoldOffset;
                _allowRotation = _movableObject.AllowRotation;

                _useLimits = _movableObject.UseMouseLimits;
                _verticalLimits = _movableObject.MouseVerticalLimits;

                _slidingVolume = _movableObject.SlideVolume;
                _volumeFadeSpeed = _movableObject.VolumeFadeSpeed;
                _holdDistance = _movableObject.HoldDistance;

                float weight = _movableObject.ObjectWeight;
                float walkMultiplier = _movableObject.WalkMultiplier;
                float lookMultiplier = _movableObject.LookMultiplier;

                _oldSensitivity = _cameraLook.SensitivityX;

                float walkSpeed = _machine.PlayerBasicSettings.WalkSpeed;
                float walkMul = Mathf.Min(1f, walkSpeed * 10f / weight);
                float lookMul = Mathf.Min(1f, _oldSensitivity * 10f / weight);

                _movementSpeed = walkSpeed * walkMul * walkMultiplier;
                if (_allowRotation) _cameraLook.SensitivityX = _oldSensitivity * lookMul * lookMultiplier;
                else _cameraLook.SensitivityX = 0f;

                Vector3 forwardGlobal = _forwardAxis.Convert();
                Vector3 forwardLocal = _movable.Direction(_forwardAxis);
                Vector3 movablePos = _movable.position;

                movablePos.y = Position.y;
                _targetPosition = _movable.TransformPoint((-forwardGlobal * _holdDistance) + _holdOffset);
                _targetPosition.y = Position.y;

                _holdOffset = Quaternion.LookRotation(forwardGlobal) * _holdOffset;
                float angleOffset = Vector3.Angle(_movable.forward, forwardLocal);
                float lookX = _movable.eulerAngles.y - angleOffset;

                _targetLook = new Vector2(lookX, 0f);
                _motionController.SetEnabled(false);
                InputManager.ResetToggledButtons();

                _startingPosition = Position;
                _machine.Motion = Vector3.zero;
                _lerpFactor = 0f;

                //audioSource.volume = 0f;
                //audioSource.Play();

                _playerItems.DeactivateCurrentItem();
                _playerItems.IsItemsUsable = false;

                _gameManager.ShowControlsInfo(true, _data.ControlExit);
            }

            

            public override void OnStateUpdate()
            {
                _lerpFactor = Mathf.SmoothDamp(_lerpFactor, 1.001f, ref _tVel, _data.ToMovableTime);

                if (_lerpFactor < 1f && !_isMoved)
                {
                    Position = Vector3.Lerp(_startingPosition, _targetPosition, _lerpFactor);
                    _cameraLook.CustomLerp(_targetLook, _lerpFactor);
                }
                else if(!_isMoved)
                {
                    Position = _targetPosition;
                    _cameraLook.ResetCustomLerp();
                    if (_useLimits) _cameraLook.SetVerticalLimits(_verticalLimits);
                    _isMoved = true;
                }
                else
                {
                    MovementUpdate();
                    // SoundUpdate();
                }

                _controllerState = _machine.StandingState;
                PlayerHeightUpdate();
            }

            // private void SoundUpdate()
            // {
            //     Vector3 motion = controller.velocity;
            //     motion.y = 0f;
            //
            //     float magnitude = motion.magnitude > 0 ? 1f : 0f;
            //     float targetVolume = slidingVolume * magnitude;
            //     audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, Time.deltaTime * volumeFadeSpeed);
            // }

            private void MovementUpdate()
            {
                Vector3 wishDir = _direction switch
                {
                    MoveDirectionEnum.ForwardBackward   => new Vector3(0, 0, _machine.Input.y),
                    MoveDirectionEnum.LeftRight         => new Vector3(_machine.Input.x, 0, 0),
                    MoveDirectionEnum.AllDirections     => new Vector3(_machine.Input.x, 0, _machine.Input.y),
                    _ => Vector3.zero
                };

                wishDir = _cameraLook.RotationX * wishDir;

                if (_machine.IsGrounded)
                {
                    if (PushingUtilities.CanMove(wishDir, _movable, _movementSpeed, _collider, _collisionMask)) 
                        _machine.Motion = wishDir * _movementSpeed;
                    else 
                        _machine.Motion = Vector3.zero;

                    _machine.Motion.y = -_machine.PlayerControllerSettings.AntiBumpFactor;
                }

                ApplyGravity(ref _machine.Motion);

                Vector3 movablePosition = Position;
                Vector3 offset = _cameraLook.RotationX * _holdOffset;

                movablePosition += CameraForward * _holdDistance;
                movablePosition += offset;
                movablePosition.y = _movable.position.y;
                _movable.position = movablePosition;

                if (_allowRotation)
                {
                    Vector3 movableForward = _forwardAxis.Convert();
                    Quaternion rotation = Quaternion.FromToRotation(movableForward, CameraForward);

                    if (PushingUtilities.CanRotate(rotation, _movable, _collider, _collisionMask)) 
                        _prevRotationX = _cameraLook.Rotation.x;
                    else 
                        _cameraLook.Rotation.x = _prevRotationX;

                    _movable.rotation = rotation;
                }
            }
            
            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<WalkingStateAsset>(() => InputManager.ReadButtonOnce("Jump", Controls.JUMP)),
                    Transition.To<DeathStateAsset>(() => IsDead)
                };
            }
            
            public override void OnStateExit()
            {
                _playerItems.IsItemsUsable = true;
                if (!IsDead) _motionController.SetEnabled(true);
                //movableObject.FadeSoundOut();
                _cameraLook.ResetCustomLerp();
                _cameraLook.ResetLookLimits();
                _cameraLook.SensitivityX = _oldSensitivity;
                _targetPosition = Vector3.zero;
                _isMoved = false;

                if (_interactCollider != null)
                    _interactCollider.enabled = true;

                _gameManager.ShowControlsInfo(false, null);
            }
        }
    }
}