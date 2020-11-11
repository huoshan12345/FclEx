using System;
using System.Collections.Generic;

namespace FclEx.Web.Models
{
    public class SubmitInfo
    {
        public Uri? SubmitUrl { get; set; }
        public Dictionary<string, string>? Paras { get; set; }
    }
}
