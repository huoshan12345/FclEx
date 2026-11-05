namespace FclEx.Serilog.Formatting;

public class JsonFormatterOptionsTests
{
    [Fact]
    public void NewOptions_HaveIndependentExceptionOptions()
    {
        var first = new JsonFormatterOptions();
        var second = new JsonFormatterOptions();

        Assert.NotSame(first.ExceptionFormatOptions, second.ExceptionFormatOptions);
        first.ExceptionFormatOptions.MaxMessageLength = 5;
        Assert.Null(second.ExceptionFormatOptions.MaxMessageLength);
    }

    [Fact]
    public void DefaultFormatters_HaveIndependentOptions()
    {
        var first = new InspectableFormatter();
        var second = new InspectableFormatter();

        Assert.NotSame(first.FormatterOptions, second.FormatterOptions);
        Assert.NotSame(first.FormatterOptions.ValueFormatter, second.FormatterOptions.ValueFormatter);
        Assert.NotSame(first.FormatterOptions.ExceptionFormatOptions, second.FormatterOptions.ExceptionFormatOptions);
        first.FormatterOptions.MessageName = "customMessage";
        first.FormatterOptions.ExceptionFormatOptions.MaxMessageLength = 5;
        Assert.Equal("@m", second.FormatterOptions.MessageName);
        Assert.Null(second.FormatterOptions.ExceptionFormatOptions.MaxMessageLength);
    }

    [Fact]
    public void NullExceptionOptions_AreReplacedWithIndependentDefaults()
    {
        var firstOptions = new JsonFormatterOptions { ExceptionFormatOptions = null! };
        var secondOptions = new JsonFormatterOptions { ExceptionFormatOptions = null! };
        _ = new JsonFormatter(firstOptions);
        _ = new JsonFormatter(secondOptions);

        Assert.NotNull(firstOptions.ExceptionFormatOptions);
        Assert.NotSame(firstOptions.ExceptionFormatOptions, secondOptions.ExceptionFormatOptions);
        Assert.NotSame(ExceptionFormatOptions.Default, firstOptions.ExceptionFormatOptions);
    }

    private sealed class InspectableFormatter : JsonFormatter
    {
        public JsonFormatterOptions FormatterOptions => Options;
    }
}
