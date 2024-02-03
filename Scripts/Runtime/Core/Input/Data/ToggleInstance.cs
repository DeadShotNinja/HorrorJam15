namespace HJ.Input
{
    public struct ToggleInstance
    {
        public string Action;
        public bool Unpressed;

        public ToggleInstance(string action)
        {
            Action = action;
            Unpressed = false;
        }
    }
}
