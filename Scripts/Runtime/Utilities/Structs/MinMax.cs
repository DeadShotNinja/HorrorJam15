using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public struct MinMax
    {
        public float Min;
        public float Max;

        public bool IsFlipped => Max < Min;

        public bool HasValue => Min != 0 || Max != 0;

        public float RealMin => IsFlipped ? Max : Min;
        public float RealMax => IsFlipped ? Min : Max;
        public Vector2 RealVector => this;
        public Vector2 Vector => new Vector2(Min, Max);

        public MinMax(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public static implicit operator Vector2(MinMax minMax)
        {
            return new Vector2(minMax.RealMin, minMax.RealMax);
        }

        public static implicit operator MinMax(Vector2 vector)
        {
            MinMax result = default;
            result.Min = vector.x;
            result.Max = vector.y;
            return result;
        }

        public MinMax Flip() => new MinMax(Max, Min);
    }
}
