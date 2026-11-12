using System.Reflection;

namespace FclEx.Serilog;

public class FieldInfosTests
{
    public static TheoryData<FieldInfo, string> EventFields => new()
    {
        { FieldInfos.LogEvent_Level, nameof(LogEvent.Level) },
        { FieldInfos.LogEvent_Exception, nameof(LogEvent.Exception) },
        { FieldInfos.LogEvent_MessageTemplate, nameof(LogEvent.MessageTemplate) },
    };

    [Theory]
    [MemberData(nameof(EventFields))]
    public void EventDescriptors_ReadBackingValue(FieldInfo field, string propertyName)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, new Exception("failure"), "message");
        Assert.Equal(typeof(LogEvent), field.DeclaringType);
        Assert.Equal(typeof(LogEvent).GetProperty(propertyName)!.GetValue(logEvent), field.GetValue(logEvent));
    }

    [Fact]
    public void TemplateTextDescriptor_ReadsOriginalText()
    {
        var template = new MessageTemplateParser().Parse("Hello {Name}");
        Assert.Equal(template.Text, FieldInfos.MessageTemplate_Text.GetValue(template));
    }

    [Fact]
    public void SinkDescriptor_ExposesLiveWriteToList()
    {
        var configuration = new LoggerConfiguration();
        var sink = new CollectingSink();
        var sinks = Assert.IsType<List<ILogEventSink>>(FieldInfos.LoggerConfiguration_Sinks.GetValue(configuration));
        sinks.Add(sink);
        using var logger = configuration.CreateLogger();
        logger.Information("message");
        Assert.Single(sink.Events);
    }
}
