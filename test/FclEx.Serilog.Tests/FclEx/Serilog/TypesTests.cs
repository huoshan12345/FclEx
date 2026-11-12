namespace FclEx.Serilog;

public class TypesTests
{
    [Fact]
    public void MessageTemplateRenderer_ResolvesFromTheSerilogAssembly()
    {
        var type = Types.MessageTemplateRenderer;
        Assert.Same(typeof(LogEvent).Assembly, type.Assembly);
        Assert.Equal("Serilog.Rendering.MessageTemplateRenderer", type.FullName);
        Assert.False(type.IsPublic);
    }
}
