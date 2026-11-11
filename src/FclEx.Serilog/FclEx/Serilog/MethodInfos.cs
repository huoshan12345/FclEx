namespace FclEx.Serilog;

/// <summary>Cached Serilog implementation methods used by message-rendering helpers.</summary>
/// <remarks>Resolution depends on Serilog's private implementation and may fail with incompatible versions.</remarks>
public static class MethodInfos
{
    /// <summary>The internal renderer accepting a message template, properties, writer, format, and format provider.</summary>
    public static readonly MethodInfo MessageTemplateRenderer_Render = Types.MessageTemplateRenderer.GetRequiredMethod("Render");
}