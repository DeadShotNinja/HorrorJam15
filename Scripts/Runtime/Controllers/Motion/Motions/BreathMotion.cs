using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class BreathMotion : SimpleMotionModule
    {
        public override string Name => "General/Breath Motion";

        [Header("General Settings")]
        [SerializeField] private AnimationCurve _breathingPattern = new(new(0, 1), new(1, 1));
        [SerializeField] private float _breathingRate;
        [SerializeField] private float _breathingIntensity;

        private float _currentBreathingCycleTime;

        public override void MotionUpdate(float deltaTime)
        {
            // If not updatable, reset to initial conditions
            if (!IsUpdatable)
            {
                SetTargetPosition(Vector3.zero);
                _currentBreathingCycleTime = 0f;
                return;
            }

            // Check if we've completed the breathing cycle, if so, reset the cycle
            if (_currentBreathingCycleTime > _breathingPattern[_breathingPattern.length - 1].time)
                _currentBreathingCycleTime = 0f;

            // Advance the breathing cycle
            _currentBreathingCycleTime += Time.deltaTime * _breathingRate;
            float evaluatedBreathingValue = _breathingPattern.Evaluate(_currentBreathingCycleTime) * _breathingIntensity;

            // Create the breathing motion vector
            Vector3 breathingMotion = new Vector3(0, evaluatedBreathingValue, 0);
            SetTargetPosition(breathingMotion);
        }

        public override void Reset()
        {
            _currentBreathingCycleTime = 0f;
        }
    }
}
