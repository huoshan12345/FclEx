using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Services;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace FclEx.Web.Core
{
    public interface IUserClient
    {
        int Id { get; }
        ILogger Logger { get; }
        bool IsOnline { get; }
        [AllowNull] IHttpService HttpService { get; set; }
        Task<OperateResult> Login(CancellationToken token = default);
        Task<OperateResult> FakeLogin(bool appendLoginIfFail = true, CancellationToken token = default);
        Task<OperateResult> Logout(CancellationToken token = default);
        Task WaitForLogin(CancellationToken token = default);
    }
}
