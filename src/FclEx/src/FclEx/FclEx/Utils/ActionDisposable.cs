using System;

namespace FclEx.Utils
{
    public readonly struct ActionDisposable : IDisposable
    {
        private readonly Action _action;

        public ActionDisposable(Action action)
        {
            _action = Check.NotNull(action);
        }

        public void Dispose()
        {
            _action();
        }
    }
}
