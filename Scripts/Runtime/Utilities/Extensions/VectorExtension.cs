using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HJ.Tools
{
    public static class VectorExtension
    {
        /// <summary>
        /// Determines where a value lies between two vectors.
        /// </summary>
        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            if (a != b)
            {
                Vector3 AB = b - a;
                Vector3 AV = value - a;
                float t = Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
                return Mathf.Clamp01(t);
            }

            return 0f;
        }

        /// <summary>
        /// Get position in Quadratic Bezier Curve.
        /// </summary>
        /// <param name="p1">Starting point</param>
        /// <param name="p2">Ending point</param>
        /// <param name="cp">Control point</param>
        public static Vector3 QuadraticBezier(Vector3 p1, Vector3 p2, Vector3 cp, float t)
        {
            t = Mathf.Clamp01(t);
            Vector3 m1 = Vector3.LerpUnclamped(p1, cp, t);
            Vector3 m2 = Vector3.LerpUnclamped(cp, p2, t);
            return Vector3.LerpUnclamped(m1, m2, t);
        }

        /// <summary>
        /// Bezier Curve between multiple points.
        /// </summary>
        public static Vector3 BezierCurve(float t, params Vector3[] points)
        {
            if (points.Length < 1) return Vector3.zero;
            else if (points.Length == 1) return points[0];

            t = Mathf.Clamp01(t);
            Vector3[] cp = points;
            int n = points.Length - 1;

            while (n > 1)
            {
                Vector3[] rp = new Vector3[n];
                for (int i = 0; i < rp.Length; i++)
                {
                    rp[i] = Vector3.LerpUnclamped(cp[i], cp[i + 1], t);
                }

                cp = rp;
                n--;
            }

            return Vector3.LerpUnclamped(cp[0], cp[1], t);
        }

        /// <summary>
        /// Linearly interpolates between three points.
        /// </summary>
        public static Vector3 Lerp3(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            t = Mathf.Clamp01(t);
            if (t <= 0.5f) return Vector3.LerpUnclamped(a, b, t * 2f);
            return Vector3.LerpUnclamped(b, c, (t * 2f) - 1f);
        }

        /// <summary>
        /// Linearly interpolates between multiple points.
        /// </summary>
        public static Vector3 RangeLerp(float t, params Vector3[] points)
        {
            if (points.Length < 1) return Vector3.zero;
            else if (points.Length == 1) return points[0];

            t = Mathf.Clamp01(t);
            int pointsCount = points.Length - 1;
            float scale = 1f / pointsCount;
            float remap = GameTools.Remap(0, 1, 0, pointsCount, t);
            int index = Mathf.Clamp(Mathf.FloorToInt(remap), 0, pointsCount - 1);
            float indexT = Mathf.InverseLerp(index * scale, (index + 1) * scale, t);
            return Vector3.LerpUnclamped(points[index], points[index + 1], indexT);
        }

        /// <summary>
        /// Linearly interpolates between two, three or multiple points.
        /// <br>The function selects the best method for linear interpolation.</br>
        /// </summary>
        public static Vector3 Lerp(float t, Vector3[] points)
        {
            if (points.Length > 3)
            {
                return RangeLerp(t, points);
            }
            else if (points.Length == 3)
            {
                return Lerp3(points[0], points[1], points[2], t);
            }
            else if (points.Length == 2)
            {
                return Vector3.Lerp(points[0], points[1], t);
            }
            else if (points.Length == 1)
            {
                return points[0];
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Scale a vector by another vector and return the scaled vector.
        /// </summary>
        public static Vector3 Multiply(this Vector3 lhs, Vector3 rhs)
        {
            lhs.Scale(rhs);
            return lhs;
        }

        /// <summary>
        /// Checks if a collection contains all values from another collection.
        /// </summary>
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
        {
            return !source.Except(values).Any();
        }
    }
}
