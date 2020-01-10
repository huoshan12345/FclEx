using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Actions
{
    public class CancellationAction : AbstractAction
    {
        private readonly Exception _exception;

        public CancellationAction(Exception exception = null)
        {
            _exception = exception;
        }

        protected override Task<IOperateResult> ExecuteInternalAsync(CancellationToken token = default)
        {
            return _exception == null
                ? OperateResult.CreateCancel()
                : OperateResult.CreateCancel(_exception);
        }

        public static CancellationAction Instance { get; } = new CancellationAction();
    }
}
