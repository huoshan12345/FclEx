namespace FclEx.YamlDotNet;

public abstract record YamlOptions
{
    public YamlNamingConvention NamingConvention { get; init; }
    public bool UseTypeConverterAttributes { get; init; }
    public IReadOnlyCollection<Assembly>? TypeConverterAssemblies { get; init; }
}
