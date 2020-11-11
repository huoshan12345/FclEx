using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx
{
    public static class AsyncEventHandlerExtensions
    {
        public static Task InvokeAsync<TSender, TEventArgs>(this AsyncEventHandler<TSender, TEventArgs> handler, 
            TSender sender, TEventArgs args)
        {
            return handler.GetInvocationList().Cast<AsyncEventHandler<TSender, TEventArgs>>()
                .Select(m => m(sender, args)).WhenAll();
        }
    }
}
