using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FclEx.Http.Utils;

public class WebProxyEqualityComparerTests
{
    public static readonly IEqualityComparer<WebProxy> Comparer = WebProxyEqualityComparer.Instance;

    [Fact]
    public void Equals_Empty_Test()
    {
        Assert.True(Comparer.Equals(WebProxyHelper.Empty, new WebProxy()));
    }
}