namespace FclEx.Http.Auth;

public class AccessTokenProviderExtensionsTests
{
    [Fact]
    public async Task GetTokenAsync_WithSingleScope_ForwardsAsSingleElementArray()
    {
        var provider = new CaptureAccessTokenProvider();
        using var cts = new CancellationTokenSource();

        var token = await provider.GetTokenAsync("scope-a", forceRefresh: true, cts.Token);

        Assert.Equal("token", token);
        var request = Assert.Single(provider.Requests);
        Assert.Equal(["scope-a"], request.Scopes);
        Assert.True(request.ForceRefresh);
        Assert.Equal(cts.Token, request.CancellationToken);
    }

    private sealed class CaptureAccessTokenProvider : IAccessTokenProvider
    {
        public List<(string[] Scopes, bool ForceRefresh, CancellationToken CancellationToken)> Requests { get; } = [];

        public Task<string> GetTokenAsync(string[] scopes, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            Requests.Add((scopes, forceRefresh, cancellationToken));
            return Task.FromResult("token");
        }
    }
}
