using System.Collections;
using UnityEngine;
using HJ.Tools;
using UnityEngine.Events;

namespace HJ.Runtime
{
    public class LeversPuzzleLever2 : MonoBehaviour, IInteractStart
    {
        [SerializeField] private UnityEvent<bool> _onTriggered;
        
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _limitsObject;
        [SerializeField] private MinMax _switchLimits;
        [SerializeField] private Axis _limitsForward = Axis.Z;
        [SerializeField] private Axis _limitsNormal = Axis.Y;

        public bool LeverState;
        
        private float _currentAngle;
        private bool _canInteract = true;
        private bool _canUse = true;

        private void Awake()
        {
            _currentAngle = LeverState ? _switchLimits.RealMax : _switchLimits.RealMin;
            _canInteract = true;
            _canUse = true;
        }

        public void InteractStart()
        {
            if (!_canInteract || !_canUse) return;
            LeverState = !LeverState;

            OnLeverState(LeverState);

            _canUse = false;
        }

        public void SetInteractState(bool state)
        {
            _canInteract = state;
        }

        public void ResetLever()
        {
            if (LeverState)
            {
                StopAllCoroutines();
                StartCoroutine(DoLeverState(false));
            }

            LeverState = false;
        }

        private void OnLeverState(bool state)
        {
            _onTriggered?.Invoke(state);
            StartCoroutine(DoLeverState(state));
        }

        private IEnumerator DoLeverState(bool state)
        {
            _canUse = false;

            yield return SwitchLever(state ? _switchLimits.RealMax : _switchLimits.RealMin);

            if (state) AudioManager.PostAudioEvent(AudioEnvironment.LeverOn, gameObject);
            else AudioManager.PostAudioEvent(AudioEnvironment.LeverOff, gameObject);

            _canUse = true;
        }

        private IEnumerator SwitchLever(float targetAngle)
        {
            while (!Mathf.Approximately(_currentAngle, targetAngle))
            {
                _currentAngle = Mathf.MoveTowards(_currentAngle, targetAngle, Time.deltaTime * 100);
                Vector3 axis = Quaternion.AngleAxis(_currentAngle, _limitsObject.Direction(_limitsNormal)) * _limitsObject.Direction(_limitsForward);
                _target.rotation = Quaternion.LookRotation(axis);
                yield return null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_limitsObject == null)
                return;

            Vector3 forward = _limitsObject.Direction(_limitsForward);
            Vector3 upward = _limitsObject.Direction(_limitsNormal);
            HandlesDrawing.DrawLimits(_limitsObject.position, _switchLimits, forward, upward, true, radius: 0.25f);
        }
    }
}