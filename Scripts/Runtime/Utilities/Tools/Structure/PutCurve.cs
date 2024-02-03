using UnityEngine;

namespace HJ.Runtime
{
    public sealed class PutCurve
    {
        private readonly AnimationCurve _curve;
        
        public float EvalMultiply = 1f;
        public float CurveTime = 0.1f;

        public PutCurve(AnimationCurve curve)
        {
            _curve = curve;
        }

        public float Eval(float time) => _curve.Evaluate(time) * EvalMultiply;
    }
}
