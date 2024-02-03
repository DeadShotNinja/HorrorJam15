using System;
using UnityEngine;

namespace HJ.Runtime
{
    public abstract class SimpleMotionModule : MotionModule
    {
        [NonSerialized] private Vector3 _positionTarget;
        [NonSerialized] private Vector3 _rotationTarget;

        public override abstract void MotionUpdate(float deltaTime);

        public override Vector3 GetPosition(float deltaTime) => _positionTarget;
        public override Quaternion GetRotation(float deltaTime) => Quaternion.Euler(_rotationTarget);

        protected override void SetTargetPosition(Vector3 target)
        {
            target *= Weight;
            _positionTarget = target;
        }

        protected override void SetTargetPosition(Vector3 target, float multiplier = 1)
        {
            target *= Weight * multiplier;
            _positionTarget = target;
        }

        protected override void SetTargetRotation(Vector3 target)
        {
            target *= Weight;
            _rotationTarget = target;
        }

        protected override void SetTargetRotation(Vector3 target, float multiplier = 1)
        {
            target *= Weight * multiplier;
            _rotationTarget = target;
        }
    }
}
