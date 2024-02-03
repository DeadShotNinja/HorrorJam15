using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;


namespace HJ.Runtime
{
    public class GearsSystem : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onStartedTurningOn;
        [SerializeField] private UnityEvent _onStoppedTurningOn;
        
        [SerializeField] private List<Gear> _gears = new();

        [SerializeField] private float _driverGearRotationSpeedDegrees = 4f;
        [SerializeField] private int _driverIndex = -1;
        
        [SerializeField]
        [ReadOnly]
        private Gear _debugDriverGear;
        
        [SerializeField] [Min(0)] private float _turningOnDuration = 1f;

        private float _currentDriverGearAngle;
        private bool _isTurnedOn;
        private float _factor;
        
        public List<Gear> Gears => _gears;

        private void OnValidate()
        {
            _debugDriverGear = null;
            if (_driverIndex < 0)
                return;
            
            if (_driverIndex >= _gears.Count)
            {
                if (_gears.Count > 0)
                    Debug.LogWarning($"_driver is incorrect! Should not be >= than number of gears ({_gears.Count})!");    
                else
                    Debug.LogWarning("_driver is incorrect! If there's no gears, it should be = -1!");
                
                return;
            }

            _debugDriverGear = _gears[_driverIndex];
        }

        public void SetTurnedOn(bool isTurnedOn)
        {
            _isTurnedOn = isTurnedOn;
            
            var startValue = 0f;
            var endValue = 1f;
            if (!_isTurnedOn)
                (startValue, endValue) = (endValue, startValue);
            
            DOTween.To(() => startValue, x => startValue = x, endValue, _turningOnDuration)
                .OnUpdate(() => { _factor = startValue; })
                .OnComplete(() => { _factor = _isTurnedOn ? 1f : 0f; });

            if (isTurnedOn)
                VisitAllGearsAroundAndIncludingDriver(OnGearStartedRotating);
            else
                VisitAllGearsAroundAndIncludingDriver(OnGearStoppedRotating);
        }
        
        public void ToggleTurnedOn()
        {
            SetTurnedOn(!_isTurnedOn);
        }

        void Update()
        {
            if (_driverIndex < 0 || _driverIndex >= _gears.Count || _gears[_driverIndex] == null) 
                return;
            
            _currentDriverGearAngle += _driverGearRotationSpeedDegrees * _factor * Time.deltaTime;
            VisitAllGearsAroundAndIncludingDriver(SetGearRotation);
        }

        private void VisitAllGearsAroundAndIncludingDriver(Action<Gear, bool> action)
        {
            if (_driverIndex < 0)
                return;
            
            var driverGear = _gears[_driverIndex];
            if (driverGear == null)
                return;
            
            action(driverGear, false);
            
            var flipRotation = true;
            for (var i = _driverIndex - 1; i >= 0; i--)
            {
                var gear = _gears[i];
                if (gear == null || !gear.isActiveAndEnabled)
                    break;
                
                action(gear, flipRotation);
                flipRotation = !flipRotation;
            }

            flipRotation = true;
            for (var i = _driverIndex + 1; i < _gears.Count; i++)
            {
                var gear = _gears[i];
                if (gear == null || !gear.isActiveAndEnabled)
                    break;
                
                action(gear, flipRotation);
                flipRotation = !flipRotation;
            }
        }

        private void OnGearStartedRotating(Gear gear, bool _)
        {
            gear.OnStartedRotating();
        }

        private void OnGearStoppedRotating(Gear gear, bool _)
        {
            gear.OnStoppedRotating();
        }

        private void SetGearRotation(Gear gear, bool flipRotation)
        {
            var driverGearAngleTimesTeeth = _gears[_driverIndex].NumberOfTeeth * _currentDriverGearAngle;
            if (flipRotation)
                driverGearAngleTimesTeeth *= -1;
            
            gear.SetGearRotation(driverGearAngleTimesTeeth);
        }

        public void SetGear(int index, Gear gear)
        {
            _gears[index] = gear;
            
            if (gear != null)
                VisitAllGearsAroundAndIncludingDriver(SetGearRotation);
        }
    }
}