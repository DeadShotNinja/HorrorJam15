using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class JumpMotion : SpringMotionModule
    {
        [Header("General Settings")]
        [SerializeField] private OffsetSettings _jumpSettings;

        private float _remainingResetDuration;
        private bool _airborne;
        private bool _jumped;
        
        public override string Name => "General/Jump Motion";

        public override void OnStateChange(string state)
        {
            _jumped = state == PlayerStateMachine.JUMP_STATE;
        }

        public override void MotionUpdate(float deltaTime)
        {
            if (!IsUpdatable)
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
                return;
            }

            if (_remainingResetDuration > 0f)
                _remainingResetDuration -= Time.deltaTime;

            if(_jumped && !_player.StateGrounded && !_airborne)
            {
                _remainingResetDuration = _jumpSettings.Duration;
                SetTargetPosition(_jumpSettings.PositionOffset);
                SetTargetRotation(_jumpSettings.RotationOffset);
                _airborne = true;
                _jumped = false;
            }
            else if(_player.StateGrounded)
            {
                _airborne = false;
            }

            if (_remainingResetDuration <= 0f)
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
                _remainingResetDuration = 0f;
            }
        }

        public override void Reset()
        {
            _remainingResetDuration = 0f;
            _airborne = false;
        }
    }
}
