using UnityEngine;
using HJ.Runtime;

namespace HJ.Tools
{
    public static class AxisExtension
    {
        /// <summary>
        /// Convert Axis to Vector3 Direction.
        /// </summary>
        public static Vector3 Convert(this Axis axis) => axis switch
        {
            Axis.X => Vector3.right,
            Axis.X_Negative => Vector3.left,
            Axis.Y => Vector3.up,
            Axis.Y_Negative => Vector3.down,
            Axis.Z => Vector3.forward,
            Axis.Z_Negative => Vector3.back,
            _ => Vector3.up,
        };

        /// <summary>
        /// Convert Axis to Transform Direction.
        /// </summary>
        public static Vector3 Direction(this Transform transform, Axis axis)
        {
            return axis switch
            {
                Axis.X => transform.right,
                Axis.X_Negative => -transform.right,
                Axis.Y => transform.up,
                Axis.Y_Negative => -transform.up,
                Axis.Z => transform.forward,
                Axis.Z_Negative => -transform.forward,
                _ => transform.up,
            };
        }

        /// <summary>
        /// Get Vector Axis Component.
        /// </summary>
        public static float Component(this Vector3 vector, Axis axis)
        {
            return axis switch
            {
                Axis.X or Axis.X_Negative => vector.x,
                Axis.Y or Axis.Y_Negative => vector.y,
                Axis.Z or Axis.Z_Negative => vector.z,
                _ => vector.y,
            };
        }

        /// <summary>
        /// Set Vector Axis Component Value.
        /// </summary>
        public static Vector3 SetComponent(this Vector3 vector, Axis axis, float value)
        {
            switch (axis)
            {
                case Axis.X:
                case Axis.X_Negative: 
                    vector.x = value; break;
                case Axis.Y:
                case Axis.Y_Negative: 
                    vector.y = value; break;
                case Axis.Z:
                case Axis.Z_Negative: 
                    vector.z = value; break;
            }
            return vector;
        }

        /// <summary>
        /// Clamp Vector Axis to Range.
        /// </summary>
        public static Vector3 Clamp(this Vector3 vector, Axis axis, MinMax limits)
        {
            switch (axis)
            {
                case Axis.X:
                case Axis.X_Negative:
                    vector.x = Mathf.Clamp(vector.x, limits.RealMin, limits.RealMax); break;
                case Axis.Y:
                case Axis.Y_Negative:
                    vector.y = Mathf.Clamp(vector.y, limits.RealMin, limits.RealMax); break;
                case Axis.Z:
                case Axis.Z_Negative:
                    vector.z = Mathf.Clamp(vector.z, limits.RealMin, limits.RealMax); break;
            }
            return vector;
        }
    }
}
