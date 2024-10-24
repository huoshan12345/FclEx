namespace FclEx.Tests;

public static class Startup
{
    [ModuleInitializer]
    public static void Init()
    {
        ThreadPool.SetMaxThreads(200, 200);
        ThreadPool.SetMinThreads(100, 100);
        ServicePointManager.DefaultConnectionLimit = short.MaxValue;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}