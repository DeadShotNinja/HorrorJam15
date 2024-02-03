using HJ.Tools;
using UnityEngine;

namespace HJ.Runtime
{
    public static class CharacterGizmos
    {
        public static void DrawGizmos(CharacterController controller, LookController lookController, Mesh gizmosMesh, 
            float scaleOffset, Color gizmosColor, bool drawFrame)
        {
            float height = controller.height;
            Vector3 scale = (0.73f + scaleOffset) * height * Vector3.one;
            Quaternion lookRotation = Application.isPlaying ? lookController.RotationX : controller.transform.rotation;
            Quaternion rotation = lookRotation * Quaternion.Euler(-90f, 0f, 0f);

            Gizmos.color = gizmosColor.Alpha(0.1f);
            if (drawFrame) Gizmos.DrawWireMesh(gizmosMesh, controller.transform.position, rotation, scale);
            else Gizmos.DrawMesh(gizmosMesh, controller.transform.position, rotation, scale);
        }
    }
}
