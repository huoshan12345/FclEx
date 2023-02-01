using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Extensions;

public static class BoolExtensions
{
    public static string ToLower(this bool flag)
    {
        return flag ? "true" : "false";
    }
}