using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Event;

namespace FclEx.Http.Actions
{
    /// <summary>
    /// 一个伪Actor只是为了让ActorLoop停下来
    /// </summary>
    public class ExitActor : IActor
    {
        public Task<ActionEvent> ExecuteAsync(CancellationToken token)
        {
            return ActionEvent.EmptyOkEvent.ToTask();
        }
    }
}
