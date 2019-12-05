namespace FclEx.Utils
{
    public readonly struct Optional<T> where T : class
    {
        public bool HasValue { get; }
        public T Value { get; }

        public Optional(T value)
        {
            Value = value;
            HasValue = !(value is null);
        }

        public static implicit operator Optional<T>(T value) => new Optional<T>(value);

        public static implicit operator T(Optional<T> o) => o.Value;

        public void Deconstruct(out bool hasValue, out T value)
        {
            hasValue = HasValue;
            value = Value;
        }
    }
}
