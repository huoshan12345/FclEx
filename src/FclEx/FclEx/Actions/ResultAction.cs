using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Actions
{
    public readonly struct ResultAction<T> : IAction<T>
    {
        private readonly OperateResult<T> _result;

        public ResultAction(OperateResult<T> result)
        {
            _result = result;
        }

        public Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default)
        {
            return _result;
        }
    }
}
