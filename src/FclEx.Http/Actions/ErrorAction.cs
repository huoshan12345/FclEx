using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Event;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Actions
{
    public class ErrorAction : AbstractAction
    {
        private readonly Exception _ex;

        public ErrorAction(Exception ex, ILogger logger = null, ActionEventListener listener = null) 
            : base(logger, listener)
        {
            _ex = ex;
        }

        protected override ValueTask<ActionEvent> ExecuteInternalAsync(CancellationToken token)
        {
            return NotifyErrorAsync(_ex);
        }
    }
}
