using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class CurvesMotion : SpringMotionModule
    {
        [Header("Enter Curve Settings")]
        [SerializeField] private Curve3D _enterPositionCurves;
        [SerializeField] private Curve3D _enterRotationCurves;
        [SerializeField] private float _enterTimeModifier = 1f;

        [Header("Exit Curve Settings")]
        [SerializeField] private Curve3D _exitPositionCurves;
        [SerializeField] private Curve3D _exitRotationCurves;
        [SerializeField] private float _exitTimeModifier = 1f;

        private float _currentCurveTime;
        private bool _posCurveCompleted;
        private bool _rotCurveCompleted;
        private bool _hasEntered;
        private bool _reset;
        
        public override string Name => "General/Curves Motion";

        public override void MotionUpdate(float deltaTime)
        {
            if (IsUpdatable)
            {
                if (!_reset)
                {
                    _posCurveCompleted = false;
                    _rotCurveCompleted = false;
                    _currentCurveTime = 0f;
                    _reset = true;
                }

                if (!_posCurveCompleted || !_rotCurveCompleted)
                {
                    EvaluateEnterCurves(deltaTime);
                    _hasEntered = true;
                }
                else
                {
                    SetTargetPosition(Vector3.zero);
                    SetTargetRotation(Vector3.zero);
                }
            }
            else if(_hasEntered)
            {
                if (_reset)
                {
                    _posCurveCompleted = false;
                    _rotCurveCompleted = false;
                    _currentCurveTime = 0f;
                    _reset = false;
                }

                if (!_posCurveCompleted || !_rotCurveCompleted)
                {
                    EvaluateExitCurves(deltaTime);
                }
                else
                {
                    SetTargetPosition(Vector3.zero);
                    SetTargetRotation(Vector3.zero);
                    _hasEntered = false;
                }
            }
        }

        private void EvaluateEnterCurves(float deltaTime)
        {
            _posCurveCompleted = _enterPositionCurves.Duration < _currentCurveTime;
            if (!_posCurveCompleted)
            {
                Vector3 positionCurve = _enterPositionCurves.Evaluate(_currentCurveTime);
                SetTargetPosition(positionCurve);
            }

            _rotCurveCompleted = _enterRotationCurves.Duration < _currentCurveTime;
            if (!_rotCurveCompleted)
            {
                Vector3 rotationCurve = _enterRotationCurves.Evaluate(_currentCurveTime);
                SetTargetRotation(rotationCurve);
            }

            _currentCurveTime += deltaTime * _enterTimeModifier;
        }

        private void EvaluateExitCurves(float deltaTime)
        {
            _posCurveCompleted = _exitPositionCurves.Duration < _currentCurveTime;
            if (!_posCurveCompleted)
            {
                Vector3 positionCurve = _exitPositionCurves.Evaluate(_currentCurveTime);
                SetTargetPosition(positionCurve);
            }

            _rotCurveCompleted = _exitRotationCurves.Duration < _currentCurveTime;
            if (!_rotCurveCompleted)
            {
                Vector3 rotationCurve = _exitRotationCurves.Evaluate(_currentCurveTime);
                SetTargetRotation(rotationCurve);
            }

            _currentCurveTime += deltaTime * _exitTimeModifier;
        }

        public override void Reset()
        {
            _posCurveCompleted = false;
            _rotCurveCompleted = false;
            _currentCurveTime = 0f;
            _hasEntered = false;
        }
    }
}
