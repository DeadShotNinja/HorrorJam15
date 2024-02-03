using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace HJ.Runtime
{
    public class LameLockpickExchangeAnimation : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onComplete;

        [SerializeField] private RectTransform _transformToOffset;   
        [SerializeField] private RectTransform _transformForRotationGetting;   
        [SerializeField] private Vector3 _offset;
        [SerializeField] [Min(0)] private float _pullingDuration;
        [SerializeField] [Min(0)] private float _returningDuration;
        [SerializeField] private AnimationCurve _easing;

        private float _totalDuration => _pullingDuration + _returningDuration;

        private bool _playing;
        
        private float _elapsed;
        private Vector3 _offsetWithAngleApplied;
        
        public void PlayAnimation()
        {
            _playing = true;
            _offsetWithAngleApplied = Quaternion.Euler(0, 0, _transformForRotationGetting.localRotation.eulerAngles.z) * _offset;
            _elapsed = 0;
        }

        void Update()
        {
            if (!_playing) 
                return;

            _elapsed += Time.deltaTime;

            if (_elapsed <= _pullingDuration)
            {
                var f = _elapsed / _pullingDuration;
                _transformToOffset.localPosition = _offsetWithAngleApplied * _easing.Evaluate(f);
            }
            else {
                var f = 1 - (_elapsed - _pullingDuration) / _returningDuration;
                _transformToOffset.localPosition = _offsetWithAngleApplied * _easing.Evaluate(f);
            }

            if (_elapsed > _totalDuration)
            {
                _playing = false;
                _transformToOffset.localPosition = Vector3.zero;
                _onComplete?.Invoke();
            }
        }
        
        
    }
}
