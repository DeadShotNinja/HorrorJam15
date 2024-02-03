using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class SwayMotion : SpringMotionModule
    {
        [Header("General Settings")]
        [SerializeField] private float _maxSwayLength = 10f;

        [Header("Position Sway")]
        [SerializeField] private Vector3 _positionSway;
        [SerializeField] private float _positionMultiplier = 1f;

        [Header("Rotation Sway")]
        [SerializeField] private Vector3 _rotationSway;
        
        private const float PositionMod = 0.02f;
        
        public override string Name => "Player Item/Sway Motion";

        public override void MotionUpdate(float deltaTime)
        {
            if (!IsUpdatable)
                return;

            Vector2 lookDelta = _look.DeltaInput;
            lookDelta = Vector2.ClampMagnitude(lookDelta, _maxSwayLength);

            Vector3 posSway = new(
                lookDelta.x * _positionSway.x * PositionMod * _positionMultiplier,
                lookDelta.y * _positionSway.y * PositionMod * _positionMultiplier);

            Vector3 rotSway = new(
                lookDelta.y * _rotationSway.x * _positionMultiplier,
                lookDelta.x * _rotationSway.y * _positionMultiplier,
                lookDelta.x * _rotationSway.z * _positionMultiplier);

            SetTargetPosition(posSway);
            SetTargetRotation(rotSway);
        }
    }
}
