using System.Threading;
using System.Threading.Tasks;

namespace FclEx.Actions;

public interface IAction<T>
{
    Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default);
    string GetName() => GetType().ShortName();
}