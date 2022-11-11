namespace Overby.Extensions.Attachments
{
    internal struct Optional<T>
    {
        public bool Set { get; }
        public T Value { get; }

        public Optional(T value)
        {
            Value = value;
            Set = true;
        }

        public static implicit operator Optional<T>(T value) => new(value);

        public static implicit operator T(Optional<T> o) => o.Value;
    }
}
