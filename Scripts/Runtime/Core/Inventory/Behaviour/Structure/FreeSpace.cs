namespace HJ.Runtime
{
    public struct FreeSpace
    {
        public int X;
        public int Y;
        public Orientation Orientation;

        public FreeSpace(int x, int y, Orientation orientation)
        {
            X = x;
            Y = y;
            Orientation = orientation;
        }
    }
}
