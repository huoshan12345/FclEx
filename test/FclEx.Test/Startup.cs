using System.Runtime.CompilerServices;
using System.Threading;

public static class Startup
{
    [ModuleInitializer]
    public static void Init()
    {
        ThreadPool.SetMinThreads(4, 4);
        FclExStartup.Init();
    }
}