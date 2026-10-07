namespace Microsoft.Extensions.Logging;

/// <summary>
/// Marks a property to be ignored when logging (it will not be written to the log output).
/// <para>
/// Primarily intended for Serilog, and requires the following configuration:<br/>
/// <c>Destructure.UsingAttributes(x => x.RespectLogPropertyIgnoreAttribute = true)</c>
/// </para>
/// <para>
/// The official <c>LogPropertyIgnoreAttribute</c> is defined in the
/// <c>Microsoft.Extensions.Telemetry.Abstractions</c> package.
/// A same-named attribute is built in here directly to avoid taking a dependency on that package.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class LogPropertyIgnoreAttribute : Attribute;
