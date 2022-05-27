using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Utils;

namespace FclEx.Actions
{
    public readonly struct BindAction<T, TDest> : IAction<TDest>
    {
        private readonly IAction<T> _action;
        private readonly Func<T, OperateResult<TDest>> _map;

        public BindAction(IAction<T> action, Func<T, OperateResult<TDest>> map)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public async Task<OperateResult<TDest>> ExecuteAsync(CancellationToken token = default)
        {
            var result = await _action.ExecuteAsync(token).DonotCapture();
            return result.Bind(_map);
        }
    }
}
