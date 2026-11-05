namespace FclEx.Serilog.Formatting;

public class JsonFormatterOptionsTests
{
    [Fact]
    public void NewOptions_HaveIndependentExceptionOptions()
    {
        var first = new JsonFormatterOptions();
        var second = new JsonFormatterOptions();

        Assert.NotSame(first.ExceptionFormatOptions, second.ExceptionFormatOptions);
    }

    [Fact]
    public void DefaultFormatters_HaveIndependentOptions()
    {
        var first = new InspectableFormatter();
        var second = new InspectableFormatter();

        Assert.NotSame(first.FormatterOptions, second.FormatterOptions);
    }

    private sealed class InspectableFormatter : JsonFormatter
    {
        public JsonFormatterOptions FormatterOptions => Options;
    }
}
