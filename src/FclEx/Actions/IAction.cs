using Microsoft.Extensions.Logging;

namespace FclEx.Actions
{  
    public interface IAction : IActor
    {
        ILogger Logger { get; }
    }
}
