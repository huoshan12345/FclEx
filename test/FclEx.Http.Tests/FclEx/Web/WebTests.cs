namespace FclEx.Web;

public abstract class WebTests
{
    protected WebTests()
    {
        ServiceProvider = new ServiceCollection()
            .AddUserClient<TestUserClient>()
            .AddLogging(m => m.AddXunit(false).SetMinimumLevel(LogLevel.Trace))
            .BuildServiceProvider();
    }

    protected IServiceProvider ServiceProvider { get; }
}