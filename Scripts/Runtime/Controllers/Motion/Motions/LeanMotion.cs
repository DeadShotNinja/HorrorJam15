using System;
using HJ.Input;
using HJ.Tools;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class LeanMotion : SpringMotionModule
    {
        [Header("General Settings")]
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _leanPosition;
        [SerializeField] private float _leanTiltAmount;
        [SerializeField] private float _leanColliderRadius;
        
        public override string Name => "Camera/Lean Motion";

        public override void MotionUpdate(float deltaTime)
        {
            if (!IsUpdatable)
                return;

            float leanDir = InputManager.ReadInput<float>(Controls.LEAN);
            Vector3 leanPos = new Vector3(leanDir * _leanPosition, 0f, 0f);

            // calculate the lean tilt value
            float leanBlend = VectorExtension.InverseLerp(Vector3.zero, leanPos, _transform.localPosition);
            Vector3 leanTilt = -1 * leanDir * _leanTiltAmount * leanBlend * Vector3.forward;

            // calculate the head position offset value
            Vector3 leanDirection = _transform.right * leanDir;
            Ray leanRay = new Ray(_transform.position, leanDirection);

            // convert the max lean distance to a multiplier and multiply it with the leanPos value
            if (Physics.SphereCast(leanRay, _leanColliderRadius, out RaycastHit hit, _leanPosition, _layerMask))
                leanPos *= GameTools.Remap(0f, _leanPosition, 0f, 1f, hit.distance);

            SetTargetPosition(leanPos);
            SetTargetRotation(leanTilt);
        }
    }
}
