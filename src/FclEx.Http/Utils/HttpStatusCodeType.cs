using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Http.Utils
{
    public enum HttpStatusCodeType
    {
        Unknown = -1,
        None = 0,
        Info = 1,
        Success = 2,
        Redirection = 3,
        ClientError = 4,
        ServerError = 5,
    }
}
