using System;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class Settings
    {
        public ushort Rows = 5;
        public ushort Columns = 5;
        public float CellSize = 100f;
        public float Spacing = 10f;
        public float DragTime = 0.05f;
        public float RotateTime = 0.05f;
        public float DropStrength = 10f;
    }
}
