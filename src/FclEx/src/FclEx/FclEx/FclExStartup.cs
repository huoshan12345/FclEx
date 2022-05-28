using System.Net;
using System.Text;
using FclEx.Utils;

namespace FclEx;

public static class FclExStartup
{
    private static readonly Initializer _initializer = new();

    public static void Init()
    {
        _initializer.Init(() =>
        {
            ServicePointManager.DefaultConnectionLimit = short.MaxValue;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        });
    }
}