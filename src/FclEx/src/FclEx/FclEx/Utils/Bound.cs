namespace FclEx.Utils
{
    public struct Bound<T> where T : struct
    {
        public Bound(T? value, bool includeEqual) : this()
        {
            Value = value;
            IncludeEqual = includeEqual;
        }

        public T? Value { get; private set; }
        public bool IncludeEqual { get; private set; }

        public static implicit operator Bound<T>(T? value)
        {
            return new Bound<T>(value, true);
        }
    }
}