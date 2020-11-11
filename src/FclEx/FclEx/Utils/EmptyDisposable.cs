using System;

namespace FclEx.Utils
{
    public class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Instance { get; } = new EmptyDisposable();

        public void Dispose() { }
    }
}
