using UnityEngine;
using HJ.Scriptable;
using HJ.Tools;

namespace HJ.Runtime.States
{
    public class DeathStateAsset : BasicStateAsset
    {
        [SerializeField] private DeathStateData _stateData;

        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new DeathPlayerState(machine, group, _stateData);
        }

        public override string GetStateKey() => PlayerStateMachine.DEATH_STATE;

        public override string ToString() => "Death";

        public class DeathPlayerState : BasicPlayerState
        {
            private readonly DeathStateData _data;

            public override bool CanTransitionWhenDisabled => true;

            private float lerpFactor;
            private float velocity;
            private bool isGrounded;

            private Vector3 positionStart;
            private Vector3 rotationStart;

            public DeathPlayerState(PlayerStateMachine machine, PlayerStatesGroup group, DeathStateData data) : base(machine, group) 
            {
                _data = data;
            }

            public override void OnStateEnter()
            {
                positionStart = _machine.PlayerManager.CameraHolder.localPosition;
                rotationStart = _machine.PlayerManager.CameraHolder.localEulerAngles;
                _cameraLook.enabled = false;
            }

            public override void OnStateUpdate()
            {
                if (IsGrounded || isGrounded)
                {
                    lerpFactor = Mathf.SmoothDamp(lerpFactor, 1f, ref velocity, _data.DeathChangeTime);
                    float rotationBlend = GameTools.Remap(_data.RotationChangeStart, 1f, 0f, 1f, lerpFactor);

                    Vector3 localPos = Vector3.Lerp(positionStart, _data.DeathCameraPosition, lerpFactor);
                    Vector3 localRot = Vector3.Lerp(rotationStart, rotationStart + _data.DeathCameraRotation, rotationBlend);

                    _machine.PlayerManager.CameraHolder.localPosition = localPos;
                    _machine.PlayerManager.CameraHolder.localEulerAngles = localRot;

                    if (!isGrounded)
                    {
                        _machine.PlayerCollider.enabled = false;
                        _machine.Motion = Vector3.zero;
                        isGrounded = true;
                    }
                }
                else
                {
                    ApplyGravity(ref _machine.Motion);
                }
            }
        }
    }
}
