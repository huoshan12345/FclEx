using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Actions
{
    public interface IActor
    {
        Task<IOperateResult> ExecuteAsync(CancellationToken token = default);
    }
}
