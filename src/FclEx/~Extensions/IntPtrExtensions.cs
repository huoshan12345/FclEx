using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Utils;

namespace FclEx
{
    public static class IntPtrExtensions
    {
        public static IntPtrDisposable AsDisposable(this IntPtr ptr)
        {
            return new IntPtrDisposable(ptr);
        }
    }
}
