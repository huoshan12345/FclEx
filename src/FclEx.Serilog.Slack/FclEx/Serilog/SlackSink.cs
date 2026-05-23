using FclEx.Slack;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.WebApi;
using static FclEx.Serilog.Constants;

namespace FclEx.Serilog;

public class SlackSink : IBatchedLogEventSink
{
    private readonly ISlackApiClient _client;
    private readonly string _channel;
    private const int MaxLength = 2950;

    private static readonly JsonFormatterOptions _formatterOptions = new();
    private static readonly JsonFormatter _formatter = new(_formatterOptions);

    public SlackSink(string token, string channel)
    {
        _channel = channel;
        _client = CreateApiClient(token);
    }

    internal static ISlackApiClient CreateApiClient(string token)
    {
        return new ServiceCollection()
            .AddSlackNetExt(m => m.UseApiToken(token))
            .BuildServiceProvider()
            .GetRequiredService<ISlackApiClient>();
    }

    public virtual async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> events)
    {
        var dupKeys = new List<(int, DateTimeOffset)>();

        foreach (var logEvent in events.OrderBy(m => m.Timestamp))
        {
            try
            {
                var (message, hash) = RenderSlackMessage(logEvent, _channel);

                if (dupKeys.Any(m => m.Item1 == hash && (m.Item2 - logEvent.Timestamp).Duration() < TimeSpan.FromSeconds(1)))
                {
                    SelfLog.WriteLine("Skipped duplicate message within 1 second");
                    continue;
                }

                await _client.Chat.PostMessage(message);

                dupKeys.Add((hash, logEvent.Timestamp));
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Failed to send message to slack: {0}", ex);
            }
        }
    }

    public virtual Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    protected internal static (Message, int) RenderSlackMessage(LogEvent logEvent, string channel)
    {
        using var builder = new SlackStringBuilder();
        var writer = new StringWriter(builder.Builder);

        builder.Append(m =>
        {
            m.Append($"@t: {logEvent.Timestamp:O}").AppendLineFeed();
            m.Append($"@l: {logEvent.Level}").AppendLineFeed();
            if (logEvent.Properties.TryGetValue(SourceContext, out var source))
            {
                m.Append("@s: ");
                source.Render(writer, "l");
                m.AppendLineFeed();
            }
            var message = logEvent.RenderMessage("l").Trim();
            m.Append($"@m: {message}").AppendLineFeed();

            // ReSharper disable once AccessToDisposedClosure
            AppendException(builder.Builder, logEvent);
        });

        var text = builder.Builder.ToString();
        var hash = text.SkipUntil("\n").GetHashCode(); // skip timestamp line

        var message = new Message()
            .Channel(channel);

        var block = new RichTextBlock
        {
            Elements =
            [
                new RichTextPreformatted
                {
                    Elements =
                    [
                        new RichTextText
                        {
                            Text = text,
                        },
                    ],
                },
            ],
        };

        message.Blocks.Add(block);
        return (message, hash);
    }

    private static void AppendException(StringBuilder builder, LogEvent logEvent)
    {
        if (logEvent.Exception is null)
            return;

        var json = StringBuilderHelper.Build(m =>
        {
            var writer = new StringWriter(m);
            _formatter.Format(logEvent, writer);
        }).ToJsonElement();

        var lines = json.GetProperty(_formatterOptions.ExceptionName).Deserialize<string[]>() ?? [];

        builder.Append("@x: ");
        foreach (var line in lines)
        {
            if (builder.AppendLimited(line, MaxLength) == false)
                return;

            builder.AppendLineFeed();
        }
    }
}