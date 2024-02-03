using System.Collections;
using UnityEngine;
using HJ.Tools;

namespace HJ.Runtime
{
    public class LeversPuzzleLever : MonoBehaviour, IInteractStart
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _limitsObject;
        [SerializeField] private MinMax _switchLimits;
        [SerializeField] private Axis _limitsForward = Axis.Z;
        [SerializeField] private Axis _limitsNormal = Axis.Y;

        [SerializeField] private Light _leverLight;
        [SerializeField] private RendererMaterial _lightRenderer;
        public string _emissionKeyword = "_EMISSION";

        [SerializeField] private bool _useLight;
        public bool LeverState;

        private LeversPuzzle _leversPuzzle;
        
        private float _currentAngle;
        private bool _canInteract = true;
        private bool _canUse = true;

        private void Awake()
        {
            _leversPuzzle = GetComponentInParent<LeversPuzzle>();
            _currentAngle = LeverState ? _switchLimits.RealMax : _switchLimits.RealMin;
            _canInteract = true;
            _canUse = true;
        }

        public void InteractStart()
        {
            if (!_canInteract || !_canUse) return;
            LeverState = !LeverState;

            bool leverState = _leversPuzzle.LeversPuzzleType == LeversPuzzle.PuzzleType.LeversOrder || LeverState;
            OnLeverState(leverState);

            if(_leversPuzzle.LeversPuzzleType == LeversPuzzle.PuzzleType.LeversChain)
                _leversPuzzle.OnLeverInteract(this);

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
                StartCoroutine(DoLeverState(false, false));
            }

            LeverState = false;
        }

        public void ChangeLeverState()
        {
            if (!_canInteract || !_canUse) 
                return;

            LeverState = !LeverState;
            StopAllCoroutines();
            OnLeverState(LeverState);
            _canUse = false;
        }

        public void SetLeverState(bool state)
        {
            _currentAngle = state ? _switchLimits.RealMax : _switchLimits.RealMin;
            Vector3 axis = Quaternion.AngleAxis(_currentAngle, _limitsObject.Direction(_limitsNormal)) * _limitsObject.Direction(_limitsForward);
            _target.rotation = Quaternion.LookRotation(axis);
            LeverState = state;
        }

        private void OnLeverState(bool state)
        {
            if (_leversPuzzle.LeversPuzzleType != LeversPuzzle.PuzzleType.LeversOrder)
                StartCoroutine(DoLeverState(state, true));
            else
                StartCoroutine(LeverOrderPress());
        }

        private IEnumerator DoLeverState(bool state, bool sendInteractEvent)
        {
            _canUse = false;

            yield return SwitchLever(state ? _switchLimits.RealMax : _switchLimits.RealMin);

            if(sendInteractEvent && _leversPuzzle.LeversPuzzleType != LeversPuzzle.PuzzleType.LeversChain) 
                _leversPuzzle.OnLeverInteract(this);

            
            if (state) AudioManager.PostAudioEvent(AudioEnvironment.LeverOn, gameObject);
            else AudioManager.PostAudioEvent(AudioEnvironment.LeverOff, gameObject);

            if (_leversPuzzle.LeversPuzzleType != LeversPuzzle.PuzzleType.LeversOrder)
            {
                if (_useLight)
                {
                    if (state) _lightRenderer.ClonedMaterial.EnableKeyword(_emissionKeyword);
                    else _lightRenderer.ClonedMaterial.DisableKeyword(_emissionKeyword);
                    _leverLight.enabled = state;
                }
            }

            _canUse = true;
        }

        private IEnumerator SwitchLever(float targetAngle)
        {
            while (!Mathf.Approximately(_currentAngle, targetAngle))
            {
                _currentAngle = Mathf.MoveTowards(_currentAngle, targetAngle, Time.deltaTime * _leversPuzzle.LeverSwitchSpeed * 100);
                Vector3 axis = Quaternion.AngleAxis(_currentAngle, _limitsObject.Direction(_limitsNormal)) * _limitsObject.Direction(_limitsForward);
                _target.rotation = Quaternion.LookRotation(axis);
                yield return null;
            }
        }

        private IEnumerator LeverOrderPress() 
        {
            yield return DoLeverState(true, true);
            yield return DoLeverState(false, false);
            LeverState = false;
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