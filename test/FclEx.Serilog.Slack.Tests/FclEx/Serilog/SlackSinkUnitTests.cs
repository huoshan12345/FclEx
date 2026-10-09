using Moq;
using Serilog.Events;
using Serilog.Parsing;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.WebApi;

namespace FclEx.Serilog;

public class SlackSinkUnitTests
{
    private const string Channel = "C_TEST_LOGS";
    private static readonly DateTimeOffset StartTime = new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_NullClient_Throws()
    {
        Assert.Throws<ArgumentNullException>("client", () => new SlackSink((ISlackApiClient)null!, Channel));
    }

    [Fact]
    public void Constructor_NullChannel_Throws()
    {
        Assert.Throws<ArgumentNullException>("channel", () => new SlackSink(Mock.Of<ISlackApiClient>(), null!));
    }

    [Fact]
    public void Constructor_EmptyChannel_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SlackSink(Mock.Of<ISlackApiClient>(), ""));
    }

    [Fact]
    public async Task EmitBatchAsync_RendersChannelTimestampLevelSourceAndMessage()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);
        var logEvent = CreateEvent("  Hello {Name}  ", properties:
        [
            new LogEventProperty("Name", new ScalarValue("Tom")),
            new LogEventProperty(Constants.SourceContext, new ScalarValue("Example.Service"))
        ]);

        await sink.EmitBatchAsync([logEvent]);

        api.VerifyPosts(1);
        var message = Assert.Single(api.Messages);
        Assert.Equal(Channel, message.Channel);
        Assert.Equal($"@t: {StartTime:O}\n@l: Information\n@s: Example.Service\n@m: Hello Tom\n", GetText(message));
    }

    [Fact]
    public async Task EmitBatchAsync_WithoutSourceOrException_OmitsOptionalFields()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);

        await sink.EmitBatchAsync([CreateEvent("hello")]);

        api.VerifyPosts(1);
        Assert.Equal($"@t: {StartTime:O}\n@l: Information\n@m: hello\n", GetText(Assert.Single(api.Messages)));
    }

    [Fact]
    public async Task EmitBatchAsync_SpecialCharacters_ArePreservedInPreformattedText()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);
        const string text = "<value> & *bold* `code`\nsecond line";

        await sink.EmitBatchAsync([CreateEvent(text)]);

        api.VerifyPosts(1);
        Assert.Contains($"@m: {text}\n", GetText(Assert.Single(api.Messages)));
    }

    [Fact]
    public async Task EmitBatchAsync_EventsAreSentInTimestampOrder()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);

        await sink.EmitBatchAsync([
            CreateEvent("third", 2000),
            CreateEvent("first"),
            CreateEvent("second", 1000)
        ]);

        api.VerifyPosts(3);
        Assert.Collection(api.Messages,
            message => Assert.Contains("@m: first\n", GetText(message)),
            message => Assert.Contains("@m: second\n", GetText(message)),
            message => Assert.Contains("@m: third\n", GetText(message)));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(999, 1)]
    [InlineData(1000, 2)]
    [InlineData(1001, 2)]
    public async Task EmitBatchAsync_DuplicatesRespectOneSecondBoundary(int milliseconds, int expectedPosts)
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);

        await sink.EmitBatchAsync([CreateEvent("same"), CreateEvent("same", milliseconds)]);

        api.VerifyPosts(expectedPosts);
        Assert.Equal(expectedPosts, api.Messages.Count);
        Assert.Contains($"@t: {StartTime:O}\n", GetText(api.Messages[0]));
    }

    [Fact]
    public async Task EmitBatchAsync_NonAdjacentDuplicatesWithinWindow_AreSkipped()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);

        await sink.EmitBatchAsync([
            CreateEvent("same"),
            CreateEvent("different", 100),
            CreateEvent("same", 200)
        ]);

        api.VerifyPosts(2);
        Assert.Collection(api.Messages,
            message => Assert.Contains("@m: same\n", GetText(message)),
            message => Assert.Contains("@m: different\n", GetText(message)));
    }

    [Fact]
    public async Task EmitBatchAsync_DuplicateWindow_IsMeasuredFromLastSuccessfulSend()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);

        await sink.EmitBatchAsync([
            CreateEvent("same"),
            CreateEvent("same", 600),
            CreateEvent("same", 1200)
        ]);

        api.VerifyPosts(2);
        Assert.Contains($"@t: {StartTime.AddMilliseconds(1200):O}\n", GetText(api.Messages[1]));
    }

    [Fact]
    public async Task EmitBatchAsync_DifferentLevelsAndSources_AreNotDuplicates()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);

        await sink.EmitBatchAsync([
            CreateEvent("same"),
            CreateEvent("same", 100, LogEventLevel.Error),
            CreateEvent("same", 200, properties:
                [new LogEventProperty(Constants.SourceContext, new ScalarValue("Another.Service"))])
        ]);

        api.VerifyPosts(3);
        Assert.Contains("@l: Error\n", GetText(api.Messages[1]));
        Assert.Contains("@s: Another.Service\n", GetText(api.Messages[2]));
    }

    [Fact]
    public async Task EmitBatchAsync_DeduplicationDoesNotCarryAcrossBatches()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);
        var logEvent = CreateEvent("same");

        await sink.EmitBatchAsync([logEvent]);
        await sink.EmitBatchAsync([logEvent]);

        api.VerifyPosts(2);
        Assert.Equal(GetText(api.Messages[0]), GetText(api.Messages[1]));
    }

    [Fact]
    public async Task EmitBatchAsync_FailedSend_DoesNotSuppressDuplicateAndContinuesBatch()
    {
        var api = new CapturingSlackApi();
        api.Chat.SetupSequence(chat => chat.PostMessage(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Slack unavailable"))
            .ReturnsAsync(new PostMessageResponse())
            .ReturnsAsync(new PostMessageResponse());
        var sink = new SlackSink(api.Client.Object, Channel);

        await sink.EmitBatchAsync([
            CreateEvent("same"),
            CreateEvent("same", 100),
            CreateEvent("next", 200)
        ]);

        api.VerifyPosts(3);
        var attempts = api.Chat.Invocations.Select(invocation => Assert.IsType<Message>(invocation.Arguments[0])).ToArray();
        Assert.Collection(attempts,
            message => Assert.Contains("@m: same\n", GetText(message)),
            message => Assert.Contains("@m: same\n", GetText(message)),
            message => Assert.Contains("@m: next\n", GetText(message)));
    }

    [Fact]
    public async Task EmitBatchAsync_Exception_IsIncludedAndRepeatedLinesAreCollapsed()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);
        var exception = new InvalidOperationException("boom").SetStackTrace("repeated frame\nrepeated frame\nlast frame");

        await sink.EmitBatchAsync([CreateEvent("operation failed", exception: exception)]);

        api.VerifyPosts(1);
        var text = GetText(Assert.Single(api.Messages));
        Assert.Contains("@x: ", text);
        Assert.Contains("InvalidOperationException", text);
        Assert.Contains("boom", text);
        Assert.Contains("repeated frame (x2)", text);
        Assert.Contains("last frame", text);
    }

    [Fact]
    public async Task EmitBatchAsync_ExceptionLinesThatExceedLimit_AreNotAppended()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);
        var exception = new InvalidOperationException("boom").SetStackTrace(new string('x', 3000));

        await sink.EmitBatchAsync([CreateEvent("operation failed", exception: exception)]);

        api.VerifyPosts(1);
        var text = GetText(Assert.Single(api.Messages));
        Assert.Contains("@x: ", text);
        Assert.Contains("boom", text);
        Assert.DoesNotContain(new string('x', 3000), text);
        Assert.True(text.Length <= 2950);
    }

    [Fact]
    public async Task EmitBatchAsync_EmptyBatch_DoesNotPost()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);

        await sink.EmitBatchAsync([]);

        api.VerifyPosts(0);
        Assert.Empty(api.Messages);
    }

    [Fact]
    public async Task OnEmptyBatchAsync_DoesNotAccessClient()
    {
        var api = new CapturingSlackApi();
        var sink = new SlackSink(api.Client.Object, Channel);

        await sink.OnEmptyBatchAsync();

        api.Client.VerifyNoOtherCalls();
        api.Chat.VerifyNoOtherCalls();
    }

    private static LogEvent CreateEvent(
        string message,
        int milliseconds = 0,
        LogEventLevel level = LogEventLevel.Information,
        Exception? exception = null,
        params LogEventProperty[] properties)
    {
        return new LogEvent(StartTime.AddMilliseconds(milliseconds), level, exception,
            new MessageTemplateParser().Parse(message), properties);
    }

    private static string GetText(Message message)
    {
        var block = Assert.IsType<RichTextBlock>(Assert.Single(message.Blocks));
        var preformatted = Assert.IsType<RichTextPreformatted>(Assert.Single(block.Elements));
        return Assert.IsType<RichTextText>(Assert.Single(preformatted.Elements)).Text;
    }

    private sealed class CapturingSlackApi
    {
        public Mock<ISlackApiClient> Client { get; } = new(MockBehavior.Strict);
        public Mock<IChatApi> Chat { get; } = new(MockBehavior.Strict);
        public List<Message> Messages { get; } = [];

        public CapturingSlackApi()
        {
            Client.SetupGet(client => client.Chat).Returns(Chat.Object);
            Chat.Setup(chat => chat.PostMessage(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
                .Callback<Message, CancellationToken>((message, cancellationToken) => Messages.Add(message))
                .ReturnsAsync(new PostMessageResponse());
        }

        public void VerifyPosts(int count)
        {
            Client.VerifyGet(client => client.Chat, Times.Exactly(count));
            Chat.Verify(chat => chat.PostMessage(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Exactly(count));
            Client.VerifyNoOtherCalls();
            Chat.VerifyNoOtherCalls();
        }
    }
}
