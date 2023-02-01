using System.Runtime.CompilerServices;

public static class Startup
{
    [ModuleInitializer]
    public static void Init()
    {
        ThreadPool.SetMaxThreads(200, 200);
        ThreadPool.SetMinThreads(100, 100);
        FclExStartup.Init();
    }

    public static readonly bool IsGithubAction = Environment.GetEnvironmentVariable("GITHUB_ACTION").IsValid();
}