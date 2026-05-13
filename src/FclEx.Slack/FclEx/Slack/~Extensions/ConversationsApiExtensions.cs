using SlackNet.Events;
using SlackNet.WebApi;

namespace FclEx.Slack;

public static class ConversationsApiExtensions
{
    public static async Task<MessageEvent?> GetMessage(this IConversationsApi api, string channelId, string ts, bool includeAllMetadata = false, bool retryAfterJoin = false)
    {
        var history = await api.History(
            channelId: channelId,
            latestTs: ts,
            oldestTs: ts,
            inclusive: true,
            limit: 1,
            includeAllMetadata: includeAllMetadata,
            retryAfterJoin: retryAfterJoin);
        return history.Messages.FirstOrDefault();
    }

    public static async Task<ConversationHistoryResponse> History(this IConversationsApi api,
        string channelId,
        string? latestTs = null,
        string? oldestTs = null,
        bool inclusive = false,
        int limit = 100,
        bool includeAllMetadata = false,
        string? cursor = null, bool
            retryAfterJoin = false,
        CancellationToken cancellationToken = default)
    {
        ConversationHistoryResponse? res = null;
        try
        {
            res = await HistoryAction();
        }
        catch (SlackException ex) when (ex.ErrorCode == "not_in_channel" && retryAfterJoin)
        {
            await api.Join(channelId, cancellationToken);
            res = await HistoryAction();
        }
        return res;

        async Task<ConversationHistoryResponse> HistoryAction()
        {
            return await api.History(channelId, latestTs, oldestTs, inclusive, limit, includeAllMetadata, cursor, cancellationToken);
        }
    }

    public static Task<ConversationHistoryResponse> History(this IConversationsApi api,
        string channelId,
        DateTimeOffset? latest = null,
        DateTimeOffset? oldest = null,
        bool inclusive = false,
        int limit = 100,
        bool includeAllMetadata = false,
        string? cursor = null,
        bool retryAfterJoin = false,
        CancellationToken cancellationToken = default)
    {
        var latestTs = latest?.ToTs();
        var oldestTs = oldest?.ToTs();
        return api.History(channelId, latestTs, oldestTs, inclusive, limit, includeAllMetadata, cursor, retryAfterJoin, cancellationToken);
    }

    public static async Task<MessageEvent?> GetReply(this IConversationsApi api, string channelId, string threadTs, string ts)
    {
        var res = await api.Replies(channelId, threadTs, ts, ts, true, 1);
        return res.Messages.FirstOrDefault();
    }

    public static Task<ConversationMessagesResponse> Replies(this IConversationsApi api,
        string channelId,
        string threadTs,
        DateTimeOffset? latest = null,
        DateTimeOffset? oldest = null,
        bool inclusive = false,
        int limit = 10,
        bool includeAllMetadata = false,
        string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var latestTs = latest?.ToTs();
        var oldestTs = oldest?.ToTs();
        return api.Replies(channelId, threadTs, latestTs, oldestTs, inclusive, limit, includeAllMetadata, cursor, cancellationToken);
    }

    public static async Task<Conversation?> LookupByName(this IConversationsApi api, string name, IReadOnlyCollection<ConversationType> conversationTypes)
    {
        Check.NotEmpty(name);

        var dic = await api.LookupByNames([name], conversationTypes);
        return dic.Get(name);
    }

    public static async Task<Dictionary<string, Conversation>> LookupByNames(this IConversationsApi api, IReadOnlyCollection<string> names, IReadOnlyCollection<ConversationType> conversationTypes)
    {
        Check.NotEmpty(names);
        Check.NotEmpty(conversationTypes);

        var dic = new Dictionary<string, Conversation>();
        string? cursor = null;
        while (true)
        {
            var list = await api.List(true, 1000, conversationTypes, cursor);
            foreach (var item in list.Channels.Where(m => names.Contains(m.Name)))
            {
                dic[item.Name] = item;
            }

            cursor = list.ResponseMetadata.NextCursor;

            if (cursor.IsNullOrEmpty() || dic.Count >= names.Count)
                return dic;

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }
    }

    public static Task<Conversation?> LookupChannel(this IConversationsApi api, string name, bool isPrivate = false)
    {
        var type = isPrivate
            ? ConversationType.PrivateChannel
            : ConversationType.PublicChannel;
        return api.LookupByName(name, [type]);
    }

    public static Task<Dictionary<string, Conversation>> LookupChannels(this IConversationsApi api, IReadOnlyCollection<string> names, bool isPrivate = false)
    {
        var type = isPrivate
            ? ConversationType.PrivateChannel
            : ConversationType.PublicChannel;
        return api.LookupByNames(names, [type]);
    }

    public static Task<Conversation?> LookupGroup(this IConversationsApi api, string name)
    {
        return api.LookupByName(name, [ConversationType.PrivateChannel]);
    }
}