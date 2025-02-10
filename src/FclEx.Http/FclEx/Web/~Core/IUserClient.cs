namespace FclEx.Web;

public interface IUserClient
{
    int Id { get; }
    ILogger Logger { get; }
    bool IsOnline { get; }
    IUserAccount Account { get; set; }
    IClientSession Session { get; }
    AccountStatus AccountStatus { get; set; }
    event Action<AccountStatus> OnAccountStatusChanged;
    [AllowNull] IHttpService HttpService { get; set; }
    Task<OperationResult> LoginAsync(CancellationToken token = default);
    Task<OperationResult> FakeLoginAsync(bool loginIfFail = true, CancellationToken token = default);
    Task<OperationResult> LogoutAsync(CancellationToken token = default);
    Task WaitLoginAsync(CancellationToken token = default);
}