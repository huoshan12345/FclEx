using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using FclEx.Utils;

namespace FclEx
{
    public static class FclExStartup
    {
        private static readonly Initializer _initializer = new Initializer();
        public static void Init()
        {
            _initializer.Init(() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance));
        }
    }
}
