using System;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class ControllerSettings
    {
        public float BaseGravity = -9.81f;
        public float PlayerWeight = 70f;
        public float AntiBumpFactor = 4.5f;
        public float WallRicochet = 0.1f;
        public float StateChangeSpeed = 3f;
    }
}
