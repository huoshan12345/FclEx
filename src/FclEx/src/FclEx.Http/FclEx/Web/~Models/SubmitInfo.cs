using System;
using System.Collections.Generic;

namespace FclEx.Web;

public class SubmitInfo
{
    public SubmitInfo(Uri submitUrl)
    {
        SubmitUrl = submitUrl;
    }

    public Uri SubmitUrl { get; set; }
    public Dictionary<string, string?> Paras { get; set; } = new();
}