namespace FclEx.Utils
{
    public readonly struct StringError
    {
        public readonly bool HasError;
        public readonly string Error;

        public StringError(bool hasError, string error)
        {
            HasError = hasError;
            Error = error;
        }

        public void Deconstruct(out bool hasError, out string error)
        {
            hasError = HasError;
            error = Error;
        }

        public static implicit operator StringError((bool, string) tuple)
        {
            return new(tuple.Item1, tuple.Item2);
        }

        public static implicit operator StringError(string? error)
        {
            return new(error.IsValid(), error ?? "");
        }
    }
}
