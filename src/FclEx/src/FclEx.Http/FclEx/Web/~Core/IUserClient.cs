namespace FclEx.Web;

public interface IUserClient
{
    int Id { get; }
    ILogger Logger { get; }
    bool IsOnline { get; }
    IUserAccount Account { get; set; }
    ISession Session { get; }
    AccountStatus AccountStatus { get; set; }
    event Action<AccountStatus> OnAccountStatusChanged;
    [AllowNull] IHttpService HttpService { get; set; }
    Task<OperateResult> LoginAsync(CancellationToken token = default);
    Task<OperateResult> FakeLoginAsync(bool loginIfFail = true, CancellationToken token = default);
    Task<OperateResult> LogoutAsync(CancellationToken token = default);
    Task WaitLoginAsync(CancellationToken token = default);
}