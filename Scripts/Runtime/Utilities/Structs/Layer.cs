using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public struct Layer
    {
        public int Index;

        public static implicit operator int(Layer layer)
        {
            return layer.Index;
        }

        public static implicit operator Layer(int intVal)
        {
            Layer result = default;
            result.Index = intVal;
            return result;
        }

        public bool CompareLayer(GameObject obj)
        {
            return obj.layer == this;
        }
    }
}
