using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class BobMotion : SpringMotionModule
    {
        [Header("General Settings")]
        [SerializeField] private float _bobbingSpeed = 1f;
        [SerializeField] private float _resetSpeed = 10f;
        [SerializeField] private float _playerStopSpeed = 0.5f;

        [Header("Amplitude Settings")]
        [SerializeField] private Vector3 _positionAmplitude = Vector3.zero;
        [SerializeField] private Vector3 _rotationAmplitude = Vector3.zero;

        private float _currentBobTime;
        private Vector3 _currentPositionBob;
        private Vector3 _currentRotationBob;
        
        public override string Name => "General/Bob Motion";

        public override void MotionUpdate(float deltaTime)
        {
            float playerSpeed = _component.PlayerCollider.velocity.magnitude;
            bool isIdle = playerSpeed <= _playerStopSpeed || !IsUpdatable;

            if (!isIdle)
            {
                _currentBobTime = Time.time * _bobbingSpeed;
                float bobY = Mathf.Cos(_currentBobTime * 2);

                Vector3 posAmplitude = _positionAmplitude;
                _currentPositionBob = new Vector3
                {
                    x = Mathf.Cos(_currentBobTime) * posAmplitude.x,
                    y = bobY * posAmplitude.y,
                    z = Mathf.Cos(_currentBobTime) * posAmplitude.z
                };

                Vector3 rotAmplitude = _rotationAmplitude;
                _currentRotationBob = new Vector3
                {
                    x = Mathf.Cos(_currentBobTime * 2) * rotAmplitude.x,
                    y = Mathf.Cos(_currentBobTime) * rotAmplitude.y,
                    z = Mathf.Cos(_currentBobTime) * rotAmplitude.z
                };

                Parameters["waveY"] = bobY;
            }
            else
            {
                float resetBobSpeed = deltaTime * _resetSpeed * 10f;
                _currentBobTime = Mathf.MoveTowards(_currentBobTime, 0f, resetBobSpeed);

                if (Mathf.Abs(_currentPositionBob.x + _currentPositionBob.y + _currentPositionBob.y) > 0.001f)
                    _currentPositionBob = Vector3.MoveTowards(_currentPositionBob, Vector3.zero, resetBobSpeed);
                else
                    _currentPositionBob = Vector3.zero;

                if (Mathf.Abs(_currentRotationBob.x + _currentRotationBob.y + _currentRotationBob.y) > 0.001f)
                    _currentRotationBob = Vector3.MoveTowards(_currentRotationBob, Vector3.zero, resetBobSpeed);
                else
                    _currentRotationBob = Vector3.zero;
            }

            SetTargetPosition(_currentPositionBob);
            SetTargetRotation(_currentRotationBob);
        }

        public override void Reset()
        {
            _currentBobTime = 0f;
            _currentPositionBob = Vector3.zero;
            _currentRotationBob = Vector3.zero;
        }
    }
}
