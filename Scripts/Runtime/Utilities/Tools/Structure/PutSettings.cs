using UnityEngine;

namespace HJ.Runtime
{
    public struct PutSettings
    {
        public Vector3 PutPosition;
        public Quaternion PutRotation;
        public Vector3 PutControl;
        public PutCurve PutPositionCurve;
        public PutCurve PutRotationCurve;
        public bool IsLocalSpace;

        public PutSettings(Transform tr, Vector3 controlOffset, PutCurve posCurve, PutCurve rotCurve, bool isLocalSpace)
        {
            PutPosition = isLocalSpace ? tr.localPosition : tr.position;
            PutRotation = isLocalSpace ? tr.localRotation : tr.rotation;
            PutControl = isLocalSpace ? tr.localPosition + controlOffset : tr.position + controlOffset;
            PutPositionCurve = posCurve;
            PutRotationCurve = rotCurve;
            IsLocalSpace = isLocalSpace;
        }
    }
}
