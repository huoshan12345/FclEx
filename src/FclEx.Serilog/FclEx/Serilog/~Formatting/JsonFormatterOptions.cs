namespace FclEx.Serilog;

/// <summary>Configures JSON output. Nonempty basic field names are reserved against event and linking-metadata properties.</summary>
/// <remarks>
/// Nonempty basic field names must be distinct. Property collisions are checked after the naming policy
/// and any @ escaping are applied; colliding properties are omitted even when an event has no exception.
/// </remarks>
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class JsonFormatterOptions
{
    /// <summary>
    /// Specifies the formatter for JSON values. The default formatter does not include type tags.
    /// </summary>
    public JsonValueFormatter ValueFormatter { get; set; } = new(null);
    /// <summary>
    /// The UTC timestamp member name, defaulting to @t. An empty name omits the timestamp.
    /// </summary>
    public string UtcTimeName { get; set; } = "@t";
    /// <summary>
    /// The rendered message member name, defaulting to @m. An empty name omits the message.
    /// </summary>
    public string MessageName { get; set; } = "@m";
    /// <summary>
    /// The event level member name, defaulting to @l. An empty name omits the level.
    /// </summary>
    public string LogLevelName { get; set; } = "@l";
    /// <summary>
    /// The exception member name, defaulting to @x. An empty name omits exception formatting and output.
    /// </summary>
    public string ExceptionName { get; set; } = "@x";
    /// <summary>
    /// Names top-level event and flattened linking-metadata properties. Defaults to CamelCase.
    /// Basic field names and nested members formatted by ValueFormatter are not transformed by this policy.
    /// </summary>
    public JsonNamingPolicy PropertyNamingPolicy { get; set; } = JsonNamingPolicy.CamelCase;
    /// <summary>Resolves duplicate top-level names after applying the naming policy and @ escaping. Defaults to KeepFirst.</summary>
    /// <remarks>
    /// Applies to event properties and flattened New Relic linking metadata in their enumeration order.
    /// Basic field names remain reserved; matching properties are omitted under every policy.
    /// Nested object and dictionary members are handled by ValueFormatter and are outside this policy.
    /// </remarks>
    public JsonPropertyCollisionPolicy PropertyCollisionPolicy { get; set; } = JsonPropertyCollisionPolicy.KeepFirst;
    /// <summary>
    /// True writes an array of exception descriptions and frames using ExceptionFormatOptions.
    /// False writes Exception.ToString() as one quoted string. Defaults to true.
    /// </summary>
    public bool EnableExceptionFormat { get; set; } = true;
    /// <summary>
    /// Specifies the formatting options for exceptions when logging or rendering them. Each new options instance owns independent defaults.
    /// </summary>
    public ExceptionFormatOptions ExceptionFormatOptions { get; set; } = new();
    /// <summary>
    /// A shared, mutable options instance for callers that explicitly choose shared configuration.
    /// Default-constructed formatters create their own options instead of using this instance.
    /// </summary>
    public static readonly JsonFormatterOptions Default = new();
}
