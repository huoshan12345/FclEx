namespace FclEx.YamlDotNet;

/// <summary>
/// Provides options shared by YAML serialization and deserialization helpers.
/// </summary>
public abstract record YamlOptions
{
    /// <summary>
    /// Gets the naming convention used to map .NET member names to YAML keys.
    /// </summary>
    public YamlNamingConvention NamingConvention { get; init; }

    /// <summary>
    /// Gets whether types decorated with <see cref="YamlTypeConverterAttribute"/> should be scanned and registered.
    /// </summary>
    public bool UseTypeConverterAttributes { get; init; }

    /// <summary>
    /// Gets the assemblies to scan for attributed YAML converters.
    /// When <c>null</c> and <see cref="UseTypeConverterAttributes"/> is <c>true</c>, all currently loaded app-domain assemblies are scanned.
    /// </summary>
    public IReadOnlyCollection<Assembly>? TypeConverterAssemblies { get; init; }
}
