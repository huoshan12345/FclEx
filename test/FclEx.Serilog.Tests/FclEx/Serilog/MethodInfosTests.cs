using System.Globalization;

namespace FclEx.Serilog;

public class MethodInfosTests
{
    [Fact]
    public void RendererDescriptor_InvokesInternalStaticRenderer()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "Hello {Name}", "Ada");
        using var writer = new StringWriter();
        var method = MethodInfos.MessageTemplateRenderer_Render;
        Assert.True(method.IsStatic);
        Assert.Equal(Types.MessageTemplateRenderer, method.DeclaringType);
        method.Invoke(null, [logEvent.MessageTemplate, logEvent.Properties, writer, "l", CultureInfo.InvariantCulture]);
        Assert.Equal("Hello Ada", writer.ToString());
    }
}
