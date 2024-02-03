using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class Curve3D
    {
        [SerializeField] private AnimationCurve _curveX = new (new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField] private AnimationCurve _curveY = new (new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField] private AnimationCurve _curveZ = new (new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField, Range(-10f, 10f)] private float _multiplier = 1f;

        private float _duration;
        public float Duration
        {
            get
            {
                if (_duration > 0f)
                    return _duration;

                float durationX = _curveX[_curveX.length - 1].time;
                float durationY = _curveY[_curveY.length - 1].time;
                float durationZ = _curveZ[_curveZ.length - 1].time;
                return _duration = Mathf.Max(durationX, durationY, durationZ);
            }
        }

        public Vector3 Evaluate(float time)
        {
            return new Vector3()
            {
                x = _curveX.Evaluate(time) * _multiplier,
                y = _curveY.Evaluate(time) * _multiplier,
                z = _curveZ.Evaluate(time) * _multiplier
            };
        }
    }
}
