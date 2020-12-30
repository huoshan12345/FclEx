namespace FclEx.Utils
{
    public readonly struct Option<T> where T : class
    {
        public bool HasValue { get; }
        public T? Value { get; }

        public Option(T? value)
        {
            Value = value;
            HasValue = !(value is null);
        }

        public static implicit operator Option<T>(T? value) => new Option<T>(value);

        public static implicit operator T?(Option<T> o) => o.Value;

        public void Deconstruct(out bool hasValue, out T? value)
        {
            hasValue = HasValue;
            value = Value;
        }
    }
}
