using Microsoft.Extensions.Logging;

namespace FclEx.Http.Actions
{  
    public interface IAction : IActor
    {
        ILogger Logger { get; }
    }
}
