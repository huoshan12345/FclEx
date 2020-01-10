using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Actions
{
    public class ErrorAction : AbstractAction
    {
        private readonly Exception _exception;

        public ErrorAction(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<IOperateResult> ExecuteInternalAsync(CancellationToken token = default)
        {
            return OperateResult.CreateError(_exception);
        }
    }
}
