using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http;

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
    Task<OperateResult> Login(CancellationToken token = default);
    Task<OperateResult> FakeLogin(bool loginIfFail = true, CancellationToken token = default);
    Task<OperateResult> Logout(CancellationToken token = default);
    Task WaitForLogin(CancellationToken token = default);
}