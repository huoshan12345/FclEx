using System.Collections.Generic;

namespace FclEx.Extensions;

public static class BoolExtensions
{
    public static string ToLower(this bool flag)
    {
        return flag ? "true" : "false";
    }
}