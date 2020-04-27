using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Actions
{
    public readonly struct MapAction<T, TDest> : IAction<TDest>
    {
        private readonly IAction<T> _action;
        private readonly Func<T, TDest> _map;

        public MapAction(IAction<T> action, Func<T, TDest> map)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _map = map ?? throw new ArgumentNullException(nameof(_map));
        }

        public async Task<IOperateResult<TDest>> ExecuteAsync(CancellationToken token = default)
        {
            var result = await _action.ExecuteAsync(token).DonotCapture();
            return result.Map(_map);
        }
    }
}
