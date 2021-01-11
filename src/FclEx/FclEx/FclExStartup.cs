using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using FclEx.Utils;

namespace FclEx
{
    public static class FclExStartup
    {
        private static readonly Initializer _initializer = new();

        [ModuleInitializer]
        internal static void Init()
        {
            _initializer.Init(() =>
            {
                ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            });
        }
    }
}
