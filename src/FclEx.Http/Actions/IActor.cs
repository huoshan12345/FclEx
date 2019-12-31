using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Http.Actions
{
    public interface IActor
    {
        Task<IOperateResult> ExecuteAsync(CancellationToken token = default);
    }
}
