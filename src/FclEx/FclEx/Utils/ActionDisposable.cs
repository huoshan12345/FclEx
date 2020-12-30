using System;
using System.Collections.Generic;
using System.Text;
using Dawn;

namespace FclEx.Utils
{
    public readonly struct ActionDisposable : IDisposable
    {
        private readonly Action _action;

        public ActionDisposable(Action action)
        {
            _action = Guard.Argument(action, nameof(action)).NotNull();
        }

        public void Dispose()
        {
            _action();
        }
    }
}
