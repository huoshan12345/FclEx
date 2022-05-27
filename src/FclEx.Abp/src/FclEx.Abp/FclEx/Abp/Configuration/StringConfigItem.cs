namespace FclEx.Abp.Configuration
{
    public struct StringConfigItem
    {
        private string? _value;

        public StringConfigItem(string @default, bool useDefaultIfNull = true, bool useDefaultIfEmpty = true)
        {
            if (useDefaultIfNull)
                Check.NotNull(@default);
            if (useDefaultIfEmpty)
                Check.NotEmpty(@default);

            Default = @default;
            UseDefaultIfNull = useDefaultIfNull;
            UseDefaultIfEmpty = useDefaultIfEmpty;
            _value = @default;
        }

        public string? Value
        {
            get => _value;
            set => _value = SetValue(value);
        }

        public bool UseDefaultIfNull { get; }
        public bool UseDefaultIfEmpty { get; }
        public string Default { get; }

        private string? SetValue(string? attemptValue)
        {
            var v = attemptValue;
            if (v == null && UseDefaultIfNull)
                v = Default;
            else if (v == string.Empty && UseDefaultIfEmpty)
                v = Default;
            return v;
        }

        public static implicit operator string?(StringConfigItem item)
        {
            return item.Value;
        }
    }
}
