using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Event;

namespace FclEx.Http.Actions
{
    public interface IActor
    {
        Task<ActionEvent> ExecuteAsync(CancellationToken token);
    }
}
