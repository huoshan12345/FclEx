using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Http;

public static class HttpContentHeadersExtensions
{
    public static void CopyTo(this HttpContentHeaders headers, HttpContentHeaders other)
    {
        foreach (var (key, values) in headers)
            other.Add(key, values);
    }
}