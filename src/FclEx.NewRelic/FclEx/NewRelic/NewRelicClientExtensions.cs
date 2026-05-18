namespace FclEx.NewRelic;

public static class NewRelicClientExtensions
{
    public static Task<NrqlResult<T>> NrqlQueryAsync<T>(this NewRelicClient client, string accountId, string nrql, int timeout = 30)
    {
        return client.NrqlQueryAsync<T>(int.Parse(accountId), nrql, timeout);
    }

    public static async Task<NrqlResult<JsonNode>> NrqlQueryAsync(this NewRelicClient client, string accountId, string nrql, int timeout = 30, bool flattenFacets = true)
    {
        var result = await client.NrqlQueryAsync<JsonNode>(accountId, nrql, timeout);
        if (flattenFacets && result.Metadata.Facets is { Length: > 1 } facets)
        {
            foreach (var item in result.Results)
            {
                var token = item["facet"];
                if (token is not JsonArray array)
                    continue;

                foreach (var (i, value, _, _) in array.IndexEx())
                {
                    var key = facets[i];
                    item[key] = value;
                }
            }
        }
        return result;
    }
}