using MoreLinq;
using Newtonsoft.Json.Linq;

namespace FclEx.NewRelic;

public static class NewRelicClientExtensions
{
    public static Task<NrqlResult<T>> NrqlQueryAsync<T>(this NewRelicClient client, string accountId, string nrql, int timeout = 30)
    {
        return client.NrqlQueryAsync<T>(int.Parse(accountId), nrql, timeout);
    }

    public static async Task<NrqlResult<JObject>> NrqlQueryAsync(this NewRelicClient client, int accountId, string nrql, int timeout = 30, bool flattenFacets = true)
    {
        var result = await client.NrqlQueryAsync<JObject>(accountId, nrql, timeout);
        if (flattenFacets && result.Metadata.Facets is { Length: > 1 } facets)
        {
            foreach (var item in result.Results)
            {
                var token = item["facet"];
                if (token is not { Type: JTokenType.Array })
                    continue;

                foreach (var (i, value) in token.Index())
                {
                    var key = facets[i];
                    item.Add(key, value);
                }
            }
        }
        return result;
    }
}