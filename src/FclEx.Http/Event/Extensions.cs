using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FclEx.Http.Actions;
using FclEx.Utils;

namespace FclEx.Http.Event
{
    public static class Extensions
    {
        public static async ValueTask InvokeAsync(
            this ActionEventListener listener,
            IAction sender,
            ActionEvent actionEvent)
        {
            var tasks = listener.GetInvocationList().Cast<ActionEventListener>()
                .Select(m => m.Invoke(sender, actionEvent)).ToArray();
            await tasks.WhenAll();
        }
    }
}
