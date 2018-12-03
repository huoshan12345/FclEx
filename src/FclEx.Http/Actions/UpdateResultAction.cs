using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Event;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Actions
{
    public class UpdateResultAction : AbstractAction
    {
        private readonly IActor _actor;
        private readonly Func<ActionEvent, ActionEvent> _func;
        public UpdateResultAction(IActor actor, Func<ActionEvent, ActionEvent> func, ILogger logger = null, ActionEventListener listener = null)
            : base(logger, listener)
        {
            _actor = actor;
            _func = func ?? (r => r);
        }

        protected override async ValueTask<ActionEvent> ExecuteInternalAsync(CancellationToken token)
        {
            var r = await _actor.ExecuteAutoAsync(token);
            r = _func(r);
            return r;
        }
    }
}
