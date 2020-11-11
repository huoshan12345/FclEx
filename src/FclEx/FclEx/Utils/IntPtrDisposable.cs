using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FclEx.Utils
{
    public struct IntPtrDisposable : IDisposable
    {
        public IntPtr Ptr { get; }

        public IntPtrDisposable(IntPtr ptr)
        {
            Ptr = ptr;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Ptr);
        }
    }
}
