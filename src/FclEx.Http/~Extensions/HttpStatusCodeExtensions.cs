using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FclEx.Http
{
    public static class HttpStatusCodeExtensions
    {
        public static bool IsSuccess(this HttpStatusCode code)
        {
            return ((int)code >= 200) && ((int)code <= 299);
        }

        public static bool IsRedirect(this HttpStatusCode code)
        {
            return code == HttpStatusCode.Redirect
                   || code == HttpStatusCode.Moved
                   || code == HttpStatusCode.SeeOther
                   || code == HttpStatusCode.RedirectKeepVerb;
        }

    }
}
