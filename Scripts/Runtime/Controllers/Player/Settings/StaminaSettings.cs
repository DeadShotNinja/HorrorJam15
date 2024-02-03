using System;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class StaminaSettings
    {
        public float JumpExhaustion = 1f;
        public float RunExhaustionSpeed = 1f;
        public float StaminaRegenSpeed = 1f;
        public float RegenerateAfter = 2f;
    }
}
