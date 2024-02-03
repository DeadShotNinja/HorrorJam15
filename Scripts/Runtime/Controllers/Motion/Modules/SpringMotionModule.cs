using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public abstract class SpringMotionModule : MotionModule
    {
        private Spring3D _positionSpring = new ();
        private Spring3D _rotationSpring = new ();

        public SpringSettings PositionSpringSettings = new (10f, 100f, 1f, 1f);
        public SpringSettings RotationSpringSettings = new (10f, 100f, 1f, 1f);

        public override void Initialize(MotionSettings motionSettings)
        {
            base.Initialize(motionSettings);
            _positionSpring = new(PositionSpringSettings);
            _rotationSpring = new(RotationSpringSettings);
        }

        public override abstract void MotionUpdate(float deltaTime);

        public override Vector3 GetPosition(float deltaTime) => _positionSpring.Evaluate(deltaTime);
        public override Quaternion GetRotation(float deltaTime) => Quaternion.Euler(_rotationSpring.Evaluate(deltaTime));

        protected override void SetTargetPosition(Vector3 target)
        {
            target *= Weight;
            _positionSpring.SetTarget(target);
        }

        protected override void SetTargetPosition(Vector3 target, float multiplier = 1)
        {
            target *= Weight * multiplier;
            _positionSpring.SetTarget(target);
        }

        protected override void SetTargetRotation(Vector3 target)
        {
            target *= Weight;
            _rotationSpring.SetTarget(target);
        }

        protected override void SetTargetRotation(Vector3 target, float multiplier = 1)
        {
            target *= Weight * multiplier;
            _rotationSpring.SetTarget(target);
        }
    }
}
