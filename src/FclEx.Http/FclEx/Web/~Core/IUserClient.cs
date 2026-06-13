namespace FclEx.Web;

/// <summary>
/// Represents a stateful client that acts on behalf of a user account.
/// </summary>
/// <typeparam name="TAccount">The account type assigned to the client.</typeparam>
public interface IUserClient<TAccount> where TAccount : IUserAccount
{
    /// <summary>
    /// A process-local client identifier.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Logger associated with this client.
    /// </summary>
    ILogger Logger { get; }

    /// <summary>
    /// The account currently assigned to the client.
    /// </summary>
    TAccount Account { get; set; }

    /// <summary>
    /// User session data populated by implementations.
    /// </summary>
    IUserClientSession Session { get; }

    /// <summary>
    /// Session and account status for the client.
    /// </summary>
    IUserClientState State { get; }

    /// <summary>
    /// The HTTP service used by actions executed through the client.
    /// </summary>
    IHttpService HttpService { get; set; }

    /// <summary>
    /// Runs the real login flow when the client is not already online.
    /// </summary>
    Task<OperationResult> LoginAsync(CancellationToken token = default);

    /// <summary>
    /// Runs the fake-login flow and optionally falls back to real login when fake login fails.
    /// </summary>
    Task<OperationResult> FakeLoginAsync(bool loginIfFail = true, CancellationToken token = default);

    /// <summary>
    /// Clears client session state and cookies.
    /// </summary>
    Task<OperationResult> LogoutAsync(CancellationToken token = default);

    /// <summary>
    /// Waits until the current login attempt, if any, releases the login lock.
    /// </summary>
    Task WaitLoginAsync(CancellationToken token = default);
}

/// <summary>
/// A user client that uses the default <see cref="IUserAccount"/> abstraction.
/// </summary>
public interface IUserClient : IUserClient<IUserAccount>;
