using GraphQL;
using Polly;

namespace FclEx.NewRelic;

public static class NerdGraphRetryPolicy<T>
{
    private static readonly string[] _retryErrors =
    [
        "exceeded the set timeout",
        "An error occurred resolving this field"
    ];

    public static readonly AsyncPolicy<GraphQLResponse<T>> Instance =
        Policy.HandleResult<GraphQLResponse<T>>(m => m.Errors?.Any(m => m.Message.ContainsAny(_retryErrors)) == true)
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(1 + retryAttempt));
}