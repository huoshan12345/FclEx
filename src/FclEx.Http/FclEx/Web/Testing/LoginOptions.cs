namespace FclEx.Web.Testing;

/// <summary>
/// Controls how <see cref="ClientCreator{TClient,TAccount}"/> initializes a user client.
/// </summary>
/// <param name="Login">Whether to run the real login flow when the client is still offline after fake login.</param>
/// <param name="FakeLogin">Whether to run the fake-login flow before real login.</param>
/// <param name="UseCache">Whether clients should be reused for equal account values.</param>
/// <param name="ReadCookie">Whether saved cookies should be loaded into newly created clients.</param>
/// <param name="Proxy">The proxy assigned to the client's HTTP service.</param>
/// <param name="CancellationToken">The cancellation token to cancel the login operation.</param>
public readonly record struct LoginOptions(
    bool Login,
    bool FakeLogin,
    bool UseCache,
    bool ReadCookie,
    IWebProxy? Proxy = null,
    CancellationToken CancellationToken = default);
