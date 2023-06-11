using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Utils;

public class WebProxyHelper
{
    public static IWebProxy Create(Uri? address, bool bypassOnLocal = false, string[]? bypassList = null, ICredentials? credentials = null)
        => new WebProxy(address, bypassOnLocal, bypassList, credentials);

    public static IWebProxy Create(string? address, bool bypassOnLocal = false, string[]? bypassList = null, ICredentials? credentials = null)
        => new WebProxy(address, bypassOnLocal, bypassList, credentials);

    public static readonly IWebProxy None = new WebProxy();
}