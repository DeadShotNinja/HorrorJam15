using UnityEngine;
using HJ.Runtime;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HJ.Tools
{
    public static class HandlesDrawing
    {
        public static void DrawLimits(Vector3 position, MinMax limits, Vector3 forward, Vector3 upward, bool bothSides = true, bool flip = false, float radius = 1f)
        {
#if UNITY_EDITOR
            Vector3 from = Quaternion.AngleAxis(bothSides || !flip ? limits.Min : limits.Min - limits.Min, upward) * forward;

            float angle = bothSides
                ? limits.Max - limits.Min % 360f
                : !flip ? -limits.Min % 360 : limits.Max % 360;

            Handles.color = Color.white;
            Handles.DrawWireArc(position, upward, from, angle, radius);

            Handles.color = Color.white.Alpha(0.1f);
            Handles.DrawSolidArc(position, upward, from, angle, radius);
#endif
        }

        public static void DrawLimitsArc(Vector3 position, MinMax limits, Vector3 forward, Vector3 upward, Color color, bool bothSides = true, bool flip = false, float radius = 1f)
        {
#if UNITY_EDITOR
            Vector3 from = Quaternion.AngleAxis(bothSides || !flip ? limits.Min : limits.Min - limits.Min, upward) * forward;

            float angle = bothSides
                ? limits.Max - limits.Min % 360f
                : !flip ? -limits.Min % 360 : limits.Max % 360;

            Handles.color = color;
            Handles.DrawWireArc(position, upward, from, angle, radius);
#endif
        }
    }
}