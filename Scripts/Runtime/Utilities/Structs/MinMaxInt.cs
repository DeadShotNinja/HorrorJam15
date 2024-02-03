using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public struct MinMaxInt
    {
        public int Min;
        public int Max;

        public bool IsFlipped => Max < Min;

        public int RealMin => IsFlipped ? Max : Min;
        public int RealMax => IsFlipped ? Min : Max;
        public Vector2Int RealVector => this;
        public Vector2Int Vector => new Vector2Int(Min, Max);

        public MinMaxInt(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public static implicit operator Vector2Int(MinMaxInt minMax)
        {
            return new Vector2Int(minMax.RealMin, minMax.RealMax);
        }

        public static implicit operator MinMaxInt(Vector2Int vector)
        {
            MinMaxInt result = default;
            result.Min = vector.x;
            result.Max = vector.y;
            return result;
        }

        public MinMaxInt Flip() => new MinMaxInt(Max, Min);
    }
}
