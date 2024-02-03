using UnityEngine;

namespace HJ.Runtime
{
    /// <summary>
    /// Procedural animation of damped harmonic oscillator.
    /// </summary>
    public sealed class Spring3D
    {
        public Vector3 CurrentValue;
        public Vector3 CurrentVelocity;
        public Vector3 CurrentAcceleration;
        public Vector3 TargetValue;
        public SpringSettings SpringSettings;

        private const float stepSizeConstant = 0.01f;

        public bool IsIdle { get; private set; } = true;

        public Spring3D() : this(SpringSettings.Default) { }

        public Spring3D(SpringSettings springSettings)
        {
            this.SpringSettings = springSettings;
            TargetValue = Vector3.zero;
            CurrentVelocity = Vector3.zero;
            CurrentAcceleration = Vector3.zero;
            IsIdle = true;
        }

        public void SetTarget(Vector3 targetVector)
        {
            TargetValue = targetVector;
            IsIdle = false;
        }

        public Vector3 Evaluate(float deltaTime)
        {
            if (IsIdle) return Vector3.zero;

            float dampingFactor = SpringSettings.Damping;
            float stiffnessFactor = SpringSettings.Stiffness;
            float objectMass = SpringSettings.Mass;

            Vector3 currentVal = CurrentValue;
            Vector3 currentVel = CurrentVelocity;
            Vector3 currentAcc = CurrentAcceleration;

            // actual step size based on the time delta and speed.
            float actualStepSize = deltaTime * SpringSettings.Speed;

            // cap the effective step size at the constant or slightly less than the actual step size.
            float effectiveStepSize = Mathf.Min(stepSizeConstant, actualStepSize - 0.001f);

            // determine the number of simulation steps.
            float calculationSteps = (int)(actualStepSize / effectiveStepSize + 0.5f);

            for (var i = 0; i < calculationSteps; i++)
            {
                // adjust the last time step to ensure the total steps add up to actualStepSize.
                var dt = Mathf.Abs(i - (calculationSteps - 1)) < 0.01f ? actualStepSize - i * effectiveStepSize : effectiveStepSize;

                // update position based on current velocity, acceleration, and time step.
                currentVal += currentVel * dt + currentAcc * (dt * dt * 0.5f);

                // calculate new acceleration based on Hooke's law (spring force) and Newton's second law (F = ma).
                Vector3 newAcc = (-stiffnessFactor * (currentVal - TargetValue) + (-dampingFactor * currentVel)) / objectMass;

                // update velocity based on the average of current and new accelerations and the time step.
                currentVel += (currentAcc + newAcc) * (dt * 0.5f);

                // update acceleration to the new value.
                currentAcc = newAcc;
            }

            CurrentValue = currentVal;
            CurrentVelocity = currentVel;
            CurrentAcceleration = currentAcc;

            // check if the object has stopped moving.
            if (Mathf.Approximately(currentAcc.sqrMagnitude, 0f))
                IsIdle = true;

            return CurrentValue;
        }
    }
}
