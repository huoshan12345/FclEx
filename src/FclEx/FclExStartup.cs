using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Utils;

namespace FclEx
{
    public static class FclExStartup
    {
        private static bool _inited;
        private static readonly AsyncLocker _locker = new AsyncLocker();

        public static void Init()
        {
            _locker.DoubleCheckAndDo(() => !_inited, InitInternal);

            void InitInternal()
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                _inited = true;
            }
        }
    }
}
