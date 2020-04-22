using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Http.Core;
using FclEx.Http.Services;

namespace FclEx.Actions
{
    public static class Extensions
    {
        public static IAction<HttpRes> ToAction(this HttpReq req, IHttpService httpService, bool unwrapError = true)
        {
            return (new HttpReqAction(req, httpService, unwrapError));
        }
    }
}
