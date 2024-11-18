namespace FclEx.Tests;

public static class Startup
{
    [ModuleInitializer]
    public static void Init()
    {
        ThreadPool.SetMaxThreads(200, 200);
        ThreadPool.SetMinThreads(100, 100);
#pragma warning disable SYSLIB0014
        ServicePointManager.DefaultConnectionLimit = short.MaxValue;
#pragma warning restore SYSLIB0014
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}