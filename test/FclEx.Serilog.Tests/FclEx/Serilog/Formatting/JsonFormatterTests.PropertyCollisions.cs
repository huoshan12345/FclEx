using System.Text.Json;

namespace FclEx.Serilog.Formatting;

public partial class JsonFormatterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Format_PropertyNamingCollision_DoesNotEmitDuplicateMembers(bool useLinkingMetadata)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "message");
        logEvent.AddOrUpdateProperty(new LogEventProperty("UserId", new ScalarValue("first")));
        if (useLinkingMetadata)
            logEvent.AddOrUpdateProperty(new LogEventProperty("newrelic.linkingmetadata", new DictionaryValue(
                new[] { new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("userId"), new ScalarValue("second")) })));
        else
            logEvent.AddOrUpdateProperty(new LogEventProperty("userId", new ScalarValue("second")));
        using var output = new StringWriter();

        new JsonFormatter().Format(logEvent, output);

        using var document = JsonDocument.Parse(output.ToString());
        Assert.Single(document.RootElement.EnumerateObject(), property => property.Name == "userId");
        Assert.Equal("first", document.RootElement.GetProperty("userId").GetString());
    }

    public static TheoryData<JsonPropertyCollisionPolicy, bool> PropertyCollisionCases =>
        (from policy in Enum.GetValues(typeof(JsonPropertyCollisionPolicy)).Cast<JsonPropertyCollisionPolicy>()
         from metadataFirst in new[] { false, true }
         select (policy, metadataFirst)).ToTheoryData();

    [Theory]
    [MemberData(nameof(PropertyCollisionCases))]
    public void Format_PropertyCollisionPolicy_ResolvesEventAndMetadataValues(JsonPropertyCollisionPolicy policy, bool metadataFirst)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "message");
        var metadata = new LogEventProperty("newrelic.linkingmetadata", new DictionaryValue(
            new[] { new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("userId"), new ScalarValue("metadata")) }));
        var property = new LogEventProperty("UserId", new ScalarValue("event"));
        logEvent.AddOrUpdateProperty(metadataFirst ? metadata : property);
        logEvent.AddOrUpdateProperty(metadataFirst ? property : metadata);
        var formatter = new JsonFormatter(new JsonFormatterOptions { PropertyCollisionPolicy = policy });
        using var output = new StringWriter();

        if (policy == JsonPropertyCollisionPolicy.Throw)
        {
            output.Write("existing output");
            var error = Assert.Throws<InvalidOperationException>(() => formatter.Format(logEvent, output));
            Assert.Contains("userId", error.Message);
            Assert.Equal("existing output", output.ToString());
            return;
        }

        formatter.Format(logEvent, output);

        using var document = JsonDocument.Parse(output.ToString());
        var actual = Assert.Single(document.RootElement.EnumerateObject(), value => value.Name == "userId");
        var first = metadataFirst ? "metadata" : "event";
        var last = metadataFirst ? "event" : "metadata";
        Assert.Equal(policy == JsonPropertyCollisionPolicy.KeepFirst ? first : last, actual.Value.GetString());
    }

    [Theory]
    [InlineData(JsonPropertyCollisionPolicy.KeepFirst)]
    [InlineData(JsonPropertyCollisionPolicy.KeepLast)]
    [InlineData(JsonPropertyCollisionPolicy.Throw)]
    public void Format_AllCollisionPolicies_PreserveReservedFields(JsonPropertyCollisionPolicy policy)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, null, "message");
        logEvent.AddOrUpdateProperty(new LogEventProperty("UserId", new ScalarValue("first")));
        logEvent.AddOrUpdateProperty(new LogEventProperty("userId", new ScalarValue("last")));
        using var output = new StringWriter();

        new JsonFormatter(new JsonFormatterOptions { LogLevelName = "userId", PropertyCollisionPolicy = policy }).Format(logEvent, output);

        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal("Error", Assert.Single(document.RootElement.EnumerateObject(), value => value.Name == "userId").Value.GetString());
    }

    [Fact]
    public void Format_KeepLast_PreservesFirstOccurrencePositionAndFormatsNamesOnce()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "message");
        logEvent.AddOrUpdateProperty(new LogEventProperty("X", new ScalarValue("first")));
        logEvent.AddOrUpdateProperty(new LogEventProperty("A", new ScalarValue("between")));
        logEvent.AddOrUpdateProperty(new LogEventProperty("x", new ScalarValue("last")));
        using var output = new StringWriter();
        var formatter = new JsonFormatter(new JsonFormatterOptions
        {
            PropertyNamingPolicy = new PrefixNamingPolicy(), PropertyCollisionPolicy = JsonPropertyCollisionPolicy.KeepLast
        });

        formatter.Format(logEvent, output);

        using var document = JsonDocument.Parse(output.ToString());
        var properties = document.RootElement.EnumerateObject().Where(value => value.Name is "prefix_x" or "prefix_a").ToArray();
        Assert.Equal(new[] { "prefix_x", "prefix_a" }, properties.Select(value => value.Name));
        Assert.Equal("last", properties[0].Value.GetString());
        Assert.False(document.RootElement.TryGetProperty("prefix_prefix_x", out _));
    }

    [Fact]
    public void Format_InvalidPropertyCollisionPolicy_ThrowsBeforeWriting()
    {
        var formatter = new JsonFormatter(new JsonFormatterOptions { PropertyCollisionPolicy = (JsonPropertyCollisionPolicy)99 });
        using var output = new StringWriter();

        Assert.Throws<ArgumentOutOfRangeException>(() => formatter.Format(
            SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "message"), output));

        Assert.Equal("", output.ToString());
    }

    private sealed class PrefixNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => "prefix_" + name.ToLowerInvariant();
    }
}
