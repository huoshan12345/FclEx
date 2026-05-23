namespace FclEx.YamlDotNet;

public abstract record YamlOptions
{
    public NamingConventionType NamingConventionType { get; set; }
    public bool WithTypeConverterAttribute { get; set; } = true;
}