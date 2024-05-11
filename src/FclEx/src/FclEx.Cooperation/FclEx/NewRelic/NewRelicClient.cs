using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace FclEx.NewRelic;

public class NewRelicClient
{
    private readonly GraphQLHttpClient _graphQlClient;

    public const string DefaultEndPoint = "https://api.newrelic.com/graphql";

    public NewRelicClient(HttpClientResolver httpClientResolver, string apiKey, string? endPoint = null)
    {
        endPoint ??= DefaultEndPoint;
        _graphQlClient = new GraphQLHttpClient(new GraphQLHttpClientOptions { EndPoint = new(endPoint) }, new NewtonsoftJsonSerializer(), httpClientResolver());
        _graphQlClient.HttpClient.DefaultRequestHeaders.Add("API-Key", apiKey);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="accountId">The New Relic account ID you want to query.</param>
    /// <param name="nrql">The NRQL query string.</param>
    /// <param name="timeout">The timeout we will apply to the NRQL Query. The value will be clamped to between 5 and 120</param>
    /// <returns></returns>
    /// <exception cref="NrqlException"></exception>
    public async Task<NrqlResult<T>> NrqlQueryAsync<T>(int accountId, string nrql, int timeout = 30)
    {
        timeout = Math.Clamp(timeout, 5, 120);

        const string query = """
                             query ($accountId: Int!, $nrql: Nrql, $timeout: Seconds) {
                               actor {
                                 account(id: $accountId) {
                                   nrql(query: $nrql, timeout: $timeout) {
                                     results
                                     metadata {
                                       facets
                                       timeWindow {
                                         begin
                                         end
                                       }
                                     }
                                   }
                                 }
                               }
                             }
                             """;

        var request = new GraphQLRequest
        {
            Query = query,
            Variables = new
            {
                AccountId = accountId,
                Nrql = nrql,
                Timeout = timeout,
            }
        };

        var response = await NerdGraphRetryPolicy<NerdGraphResponse<T>>.Instance.ExecuteAsync(() => _graphQlClient.SendQueryAsync<NerdGraphResponse<T>>(request));
        if (response.Errors is { Length: > 0 } errors)
        {
            throw new NrqlException(nrql, errors);
        }

        return response.Data.Actor.Account.Nrql;
    }
}