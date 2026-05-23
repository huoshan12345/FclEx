namespace FclEx.Web;

public interface IUserClient<TAccount> where TAccount : IUserAccount
{
    int Id { get; }
    ILogger Logger { get; }
    TAccount Account { get; set; }
    IUserClientSession Session { get; }
    IUserClientState State { get; }
    IHttpService HttpService { get; set; }
    Task<OperationResult> LoginAsync(CancellationToken token = default);
    Task<OperationResult> FakeLoginAsync(bool loginIfFail = true, CancellationToken token = default);
    Task<OperationResult> LogoutAsync(CancellationToken token = default);
    Task WaitLoginAsync(CancellationToken token = default);
}

public interface IUserClient : IUserClient<IUserAccount>;