using System.Linq;
using System.Threading.Tasks;
using FclEx;

namespace FclEx.Helpers;

public delegate Task AsyncEventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs e);

public delegate Task AsyncEventHandler<in TSender>(TSender sender);

public delegate void EventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs e);

public delegate void EventHandler<in TSender>(TSender sender);

public delegate void StatelessTimerCallback();

public static class AsyncEventHandlerExtensions
{
    public static Task InvokeAsync<TSender, TEventArgs>(this AsyncEventHandler<TSender, TEventArgs> handler,
        TSender sender, TEventArgs args)
    {
        return handler.GetInvocationList().Cast<AsyncEventHandler<TSender, TEventArgs>>()
            .Select(m => m(sender, args)).WhenAll();
    }
}