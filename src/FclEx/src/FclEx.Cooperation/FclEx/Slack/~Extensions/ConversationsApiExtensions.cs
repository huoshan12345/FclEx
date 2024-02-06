using SlackNet;
using SlackNet.Events;
using SlackNet.WebApi;

namespace FclEx.Slack;

public static class ConversationsApiExtensions
{
    public static async Task<MessageEvent?> GetMessage(this IConversationsApi api, string channelId, string ts)
    {
        var res = await api.History(channelId, ts, ts, true, 1);
        return res.Messages.FirstOrDefault();
    }

    public static async Task<MessageEvent?> GetReply(this IConversationsApi api, string channelId, string threadTs, string ts)
    {
        var res = await api.Replies(channelId, threadTs, ts, ts, true, 1);
        return res.Messages.FirstOrDefault();
    }

    public static async Task<Conversation?> LookupByName(this IConversationsApi api, string name, IReadOnlyCollection<ConversationType> conversationTypes)
    {
        Check.NotEmpty(name);

        var dic = await api.LookupByNames(new[] { name }, conversationTypes);
        return dic.GetValueOrDefault(name);
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

            if (cursor is null || dic.Count >= names.Count)
                return dic;

            await Task.Delay(200);
        }
    }

    public static Task<Conversation?> LookupChannel(this IConversationsApi api, string name, bool isPrivate = false)
    {
        var type = isPrivate
            ? ConversationType.PrivateChannel
            : ConversationType.PublicChannel;
        return api.LookupByName(name, new[] { type });
    }

    public static Task<Dictionary<string, Conversation>> LookupChannels(this IConversationsApi api, IReadOnlyCollection<string> names, bool isPrivate = false)
    {
        var type = isPrivate
            ? ConversationType.PrivateChannel
            : ConversationType.PublicChannel;
        return api.LookupByNames(names, new[] { type });
    }
}