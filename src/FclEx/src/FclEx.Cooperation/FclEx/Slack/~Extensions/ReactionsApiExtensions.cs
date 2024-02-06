using SlackNet;
using SlackNet.WebApi;

namespace FclEx.Slack;

public static class ReactionsApiExtensions
{
    public static async Task TryRemoveFromMessage(this IReactionsApi api, string name, string channelId, string ts, CancellationToken? cancellationToken = null)
    {
        try
        {
            await api.RemoveFromMessage(name, channelId, ts, cancellationToken);
        }
        catch (SlackException ex) when (ex.ErrorCode == "no_reaction") { }
    }

    public static async Task TryAddToMessage(this IReactionsApi api, string name, string channelId, string ts, CancellationToken? cancellationToken = null)
    {
        try
        {
            await api.AddToMessage(name, channelId, ts, cancellationToken);
        }
        catch (SlackException ex) when (ex.ErrorCode == "already_reacted") { }
    }
}