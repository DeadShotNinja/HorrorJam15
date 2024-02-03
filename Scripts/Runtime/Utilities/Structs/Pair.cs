namespace HJ.Tools
{
    public struct Pair<T1, T2>
    {
        public T1 Key { get; set; }
        public T2 Value { get; set; }
        public bool IsAssigned
        {
            get => Key != null && Value != null;
        }

        public Pair(T1 key, T2 value)
        {
            Key = key;
            Value = value;
        }
    }
}
