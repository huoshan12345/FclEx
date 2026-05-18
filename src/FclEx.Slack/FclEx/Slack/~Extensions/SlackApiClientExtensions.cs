namespace FclEx.Slack;

public static class SlackApiClientExtensions
{
    internal static async Task<List<TResult>> PostChunked<TApi, TResult>(this TApi api, TableData tableData, Func<TApi, TableData, Task<TResult>> postFunc)
    {
        var results = new List<TResult>();
        var total = tableData.Rows.Count();
        int? partIndex = null;
        var skip = 0;
        var take = total;

        while (true)
        {
            try
            {
                var data = tableData.WithTableTitle(partIndex, skip, take);
                var result = await postFunc(api, data);
                results.Add(result);

                skip += take;
                partIndex++;
            }
            catch (Exception ex) when (ex.Message.Contains("invalid_blocks"))
            {
                if (take < 2)
                    throw;

                take = Math.Ceiling(take / 2.0).CastTo<int>();
                partIndex ??= 0;
            }

            if (skip >= total)
                break;

            await Task.Delay(500);
        }
        return results;
    }

    public static Task PostToWebhookChunked(this ISlackApiClient client, string webhookUrl, TableData tableData)
    {
        return client.PostChunked(tableData, (c, t) => c.PostToWebhook(webhookUrl, t).ToTaskUnit());
    }

    public static Task PostToWebhook(this ISlackApiClient client, string webhookUrl, TableData tableData)
    {
        return client.PostToWebhook(webhookUrl, tableData.ToSlackMessage());
    }
}