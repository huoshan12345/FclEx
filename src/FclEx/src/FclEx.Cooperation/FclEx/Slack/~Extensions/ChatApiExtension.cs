using SlackNet.WebApi;

namespace FclEx.Slack;

public static class ChatApiExtension
{
    public static Task<PostMessageResponse> PostMessage(this IChatApi api, string channel, TableData tableData)
    {
        return api.PostMessage(tableData.ToSlackMessage().Channel(channel));
    }

    public static Task<List<PostMessageResponse>> PostChunked(this IChatApi api, string channel, TableData tableData)
    {
        return api.PostChunked(tableData, (c, t) => c.PostMessage(channel, t));
    }
}