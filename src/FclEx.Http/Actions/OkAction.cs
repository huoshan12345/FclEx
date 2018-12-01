using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Event;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Actions
{
    public class OkAction : AbstractAction
    {
        private readonly object _obj;
        public OkAction(object obj, ILogger logger = null, ActionEventListener listener = null) 
            : base(logger, listener)
        {
            _obj = obj;
        }

        protected override ValueTask<ActionEvent> ExecuteInternalAsync(CancellationToken token)
        {
            return NotifyOkEventAsync(_obj);
        }
    }
}
