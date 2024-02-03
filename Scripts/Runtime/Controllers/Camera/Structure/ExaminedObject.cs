using UnityEngine;

namespace HJ.Runtime
{
    public sealed class ExaminedObject
    {
        public InteractableItem InteractableItem;
        public PutSettings PutSettings;
        public Vector3 HoldPosition;
        public Vector3 StartPosition;
        public Quaternion StartRotation;
        public Vector3 ControlPoint;
        public float ExamineDistance;
        public float Velocity;
        public float TFactor;

        public GameObject GameObject => InteractableItem.gameObject;
    }
}
