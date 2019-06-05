using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Utils
{
    public static class OperateResultCodes
    {
        public const int Success = 0;
        public const int FromString = -1;
        public const int FromException = -2;
        public const int NotImplemented = -3;
        public const int NullData = -4;
        public const int Cancel = -5;
    }
}
