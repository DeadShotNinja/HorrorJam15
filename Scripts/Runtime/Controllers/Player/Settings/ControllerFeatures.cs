using System;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class ControllerFeatures
    {
        public bool EnableJump = true;
        public bool EnableRun = true;
        public bool EnableCrouch = true;
        public bool EnableStamina = false;
        public bool RunToggle = false;
        public bool CrouchToggle = false;
        public bool NormalizeMovement = false;
    }
}
