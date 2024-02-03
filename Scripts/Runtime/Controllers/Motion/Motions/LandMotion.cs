using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class LandMotion : SpringMotionModule
    {
        [Header("General Settings")]
        [SerializeField] private OffsetSettings _landSettings;

        [Header("Impact Settings")]
        [SerializeField] private float _maxImpactAirTime = 2f;
        [SerializeField] private float _positionMultiplier = 1f;
        [SerializeField] private float _rotationMultiplier = 1f;

        private float _remainingResetDuration;
        private float _airTime;
        private bool _airborne;
        
        public override string Name => "General/Land Motion";

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

            if (!_player.StateGrounded)
            {
                _airTime += deltaTime;
                _airborne = true;
            }
            else if(_airborne)
            {
                _remainingResetDuration = _landSettings.Duration;

                float multMod = Mathf.InverseLerp(0f, _maxImpactAirTime, _airTime);
                float posMult = Mathf.Lerp(0f, _positionMultiplier, multMod);
                float rotMult = Mathf.Lerp(0f, _rotationMultiplier, multMod);

                SetTargetPosition(_landSettings.PositionOffset * posMult);
                SetTargetRotation(_landSettings.RotationOffset * rotMult);

                _airborne = false;
                _airTime = 0f;
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
            _airTime = 0f;
            _airborne = false;
        }
    }
}
