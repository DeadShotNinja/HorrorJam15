using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class OffsetMotion : SpringMotionModule
    {
        [Header("General Settings")]
        [SerializeField] private OffsetSettings _enterOffset;
        [SerializeField] private OffsetSettings _exitOffset;

        private bool _hasEntered;
        private float _remainingResetDuration;
        
        public override string Name => "General/Offset Motion";

        public override void MotionUpdate(float deltaTime)
        {
            if (_remainingResetDuration > 0f) 
                _remainingResetDuration -= Time.deltaTime;

            // Check if the object is updatable and has just entered
            if (IsUpdatable)
            {
                if (!_hasEntered) _remainingResetDuration = _enterOffset.Duration;

                SetTargetPosition(_enterOffset.PositionOffset);
                SetTargetRotation(_enterOffset.RotationOffset);

                _hasEntered = true;
            }
            // Check if the object is not updatable and has just exited
            else if (_hasEntered)
            {
                if (_hasEntered) _remainingResetDuration = _exitOffset.Duration;

                SetTargetPosition(_exitOffset.PositionOffset);
                SetTargetRotation(_exitOffset.RotationOffset);

                _hasEntered = false;
            }

            // Reset position and rotation once the reset duration is over
            if (_remainingResetDuration <= 0f)
            {
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
            }
        }

        public override void Reset()
        {
            _hasEntered = false;
            _remainingResetDuration = 0f;
        }
    }
}
