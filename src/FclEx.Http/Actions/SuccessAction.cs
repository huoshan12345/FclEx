using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Http.Actions
{
    public class SuccessAction<T> : AbstractAction
    {
        private readonly T _obj;

        public SuccessAction(T obj)
        {
            _obj = obj;
        }

        protected override Task<IOperateResult> ExecuteInternalAsync(CancellationToken token = default)
        {
            return OperateResult.CreateSuccess(_obj);
        }
    }

    public class SuccessAction : AbstractAction
    {
        protected override Task<IOperateResult> ExecuteInternalAsync(CancellationToken token = default)
        {
            return OperateResult.Success;
        }

        public static SuccessAction Instance { get; } = new SuccessAction();
    }
}
