using UnityEngine;

namespace HJ.Scriptable
{
    [CreateAssetMenu(fileName = "StrafeMovementGroup", menuName = "HJ/Player/Strafe Movement Group")]
    public class BasicMovementGroup : PlayerStatesGroup
    {
        [Header("Movement")]
        [SerializeField] private float _friction = 10f;
        [SerializeField] private float _groundAcceleration = 10f;
        [SerializeField] private float _airAcceleration = 1.75f;
        [SerializeField] private float _airAccelerationCap = 0.1f;

        [Header("Sliding")]
        [SerializeField] private LayerMask _slidingMask;
        [SerializeField] private float _slideRayLength = 1f;
        [SerializeField] private float _slopeLimit = 40f;

        public float Friction => _friction;
        public float GroundAcceleration => _groundAcceleration;
        public float AirAcceleration => _airAcceleration;
        public float AirAccelerationCap => _airAccelerationCap;

        public LayerMask SlidingMask => _slidingMask;
        public float SlideRayLength => _slideRayLength;
        public float SlopeLimit => _slopeLimit;
    }
}
