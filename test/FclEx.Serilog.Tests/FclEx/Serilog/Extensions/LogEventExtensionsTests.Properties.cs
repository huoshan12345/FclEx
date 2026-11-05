namespace FclEx.Serilog.Extensions;

public partial class LogEventExtensionsTests
{
    public static TheoryData<object?, object?, bool> ScalarMatches => new()
    {
        { null, null, true }, { 5, 5, true }, { 5, "5", false }, { "value", "VALUE", false }
    };

    [Theory]
    [MemberData(nameof(ScalarMatches))]
    public void MatchHelpers_UseScalarEqualityWithoutConversion(object? value, object? expected, bool matches)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "message");
        var structure = new StructureValue(new[] { new LogEventProperty("Value", new ScalarValue(value)) });
        logEvent.AddOrUpdateProperty(new LogEventProperty("Value", new ScalarValue(value)));
        logEvent.AddOrUpdateProperty(new LogEventProperty("Payload", structure));

        Assert.Equal(matches, logEvent.MatchScalar("Value", expected));
        Assert.Equal(matches, structure.Match("Value", expected));
        Assert.Equal(matches, logEvent.MatchStructure("Value", expected));
        Assert.False(logEvent.MatchScalar("value", expected));
        Assert.False(logEvent.MatchScalar("Payload", expected));
    }

    [Fact]
    public void MatchStructure_DoesNotSearchNestedMembers()
    {
        var inner = new StructureValue(new[] { new LogEventProperty("Code", new ScalarValue(5)) });
        var outer = new StructureValue(new[] { new LogEventProperty("Nested", inner) });
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "message");
        logEvent.AddOrUpdateProperty(new LogEventProperty("Payload", outer));

        Assert.False(logEvent.MatchStructure("Code", 5));
        Assert.False(outer.Match("Code", 5));
        Assert.True(inner.Match("Code", 5));
    }

    [Fact]
    public void GetPropertyValue_ReturnsOnlyAssignableScalarValues()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "message");
        logEvent.AddOrUpdateProperty(new LogEventProperty("Number", new ScalarValue(5)));
        logEvent.AddOrUpdateProperty(new LogEventProperty("Text", new ScalarValue("5")));
        logEvent.AddOrUpdateProperty(new LogEventProperty("Null", new ScalarValue(null)));
        logEvent.AddOrUpdateProperty(new LogEventProperty("Object", new StructureValue(Array.Empty<LogEventProperty>())));

        Assert.Equal(5, logEvent.GetPropertyValue<int>("Number"));
        Assert.Null(logEvent.GetPropertyValue<string>("Number"));
        Assert.Equal(0, logEvent.GetPropertyValue<int>("Text"));
        Assert.Equal("5", logEvent.GetPropertyValue<string>("Text"));
        Assert.Null(logEvent.GetPropertyValue<object>("Null"));
        Assert.Null(logEvent.GetPropertyValue<object>("Object"));
        Assert.Equal(0, logEvent.GetPropertyValue<int>("Missing"));
        logEvent.AddOrUpdateProperty(new LogEventProperty(Constants.SourceContext, new ScalarValue(5)));
        Assert.Null(logEvent.GetSourceContext());
    }

    [Fact]
    public void TryAddProperty_RetainsExistingValueAndDestructuresNewObjects()
    {
        var sink = new CollectingSink();
        var enricher = new Mock<ILogEventEnricher>();
        enricher.Setup(value => value.Enrich(It.IsAny<LogEvent>(), It.IsAny<ILogEventPropertyFactory>()))
            .Callback<LogEvent, ILogEventPropertyFactory>((value, factory) =>
            {
                value.TryAddProperty(factory, "Name", "replacement");
                value.TryAddProperty(factory, "Payload", new { Name = "structured" }, destructureObjects: true);
            });
        using var logger = new LoggerConfiguration().Enrich.With(enricher.Object).WriteTo.Sink(sink).CreateLogger();

        logger.ForContext("Name", "original").Information("message");

        var logEvent = Assert.Single(sink.Events);

        Assert.Equal("original", logEvent.GetPropertyValue<string>("Name"));
        var structure = Assert.IsType<StructureValue>(logEvent.Properties["Payload"]);
        Assert.True(structure.Match("Name", "structured"));
    }

    [Fact]
    public void ShouldExclude_StopsAtFirstMatchAndAcceptsEmptyRules()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "message");
        ILogEventExcluder[] rules =
        [
            new LogEventExcluder(_ => true),
            new LogEventExcluder(_ => throw new InvalidOperationException("must not run"))
        ];

        Assert.True(logEvent.ShouldExclude(rules));
        Assert.False(logEvent.ShouldExclude(Array.Empty<ILogEventExcluder>()));
    }

    [Fact]
    public void Mutations_ReturnSameEventAndPreserveExistingProperties()
    {
        var exception = new InvalidOperationException("new exception");
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "{Name}", "preserved");
        var template = new MessageTemplate(new[] { new TextToken("literal {braces}") });

        var result = logEvent.SetLevel(LogEventLevel.Warning).SetException(exception).SetMessageTemplate(template);

        Assert.Same(logEvent, result);
        Assert.Same(exception, logEvent.Exception);
        Assert.Equal(LogEventLevel.Warning, logEvent.Level);
        Assert.Equal("preserved", logEvent.GetPropertyValue<string>("Name"));
        Assert.Equal("literal {braces}", logEvent.RenderMessage());
        Assert.Same(logEvent, logEvent.SetException(null));
        Assert.Null(logEvent.Exception);
    }

    [Theory]
    [InlineData(null, "Hello \"Tom\"")]
    [InlineData("l", "Hello Tom")]
    public void RenderMessage_StringAndWriterOverloadsHonorFormat(string? format, string expected)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "Hello {Name}", "Tom");
        using var output = new StringWriter();

        LogEventExtensions.RenderMessage(logEvent, output, format);

        Assert.Equal(expected, output.ToString());
        Assert.Equal(expected, LogEventExtensions.RenderMessage(logEvent, format));
    }

    [Fact]
    public void FormatException_IsIdempotentAndPreservesOriginalException()
    {
        var exception = new InvalidOperationException("failure");
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, exception, "message");

        Assert.Same(logEvent, logEvent.FormatException());
        var wrapper = logEvent.Exception;
        logEvent.FormatException();

        Assert.Same(wrapper, logEvent.Exception);
        Assert.Same(exception, logEvent.GetOriginalException());
    }
}
