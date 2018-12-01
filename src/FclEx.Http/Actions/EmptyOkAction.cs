using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Http.Event;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Actions
{
    public class EmptyOkAction : OkAction
    {
        private EmptyOkAction(ILogger logger = null, ActionEventListener listener = null)
            : base(null, logger, listener)
        {
        }

        public static EmptyOkAction Instance { get; } = new EmptyOkAction();
    }
}
