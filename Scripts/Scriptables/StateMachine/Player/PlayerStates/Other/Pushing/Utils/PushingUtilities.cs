using UnityEngine;

namespace HJ.Runtime.States
{
    public static class PushingUtilities
    {
        public static bool CanMove(Vector3 direction, Transform movable, float movementSpeed, BoxCollider collider, LayerMask collisionMask)
        {
            Vector3 newPosition = movable.position + direction * (movementSpeed * Time.deltaTime);
            newPosition.y += 0.01f;

            return !Physics.CheckBox(newPosition, collider.size / 2, movable.rotation, collisionMask);
        }

        public static bool CanRotate(Quaternion rotation, Transform movable, BoxCollider collider, LayerMask collisionMask)
        {
            Quaternion newRotation = rotation * movable.rotation;
            Vector3 position = movable.position + Vector3.up * 0.01f;
            return !Physics.CheckBox(position, collider.size / 2, newRotation, collisionMask);
        }
    }
}
