namespace FclEx.Web;

public abstract class WebTests
{
    protected readonly ITestOutputHelper _outputHelper;

    protected WebTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ServiceProvider = new ServiceCollection()
            .AddUserClient<TestUserClient>()
            .AddLogging(m => m.AddXunit(outputHelper, false).SetMinimumLevel(LogLevel.Trace))
            .BuildServiceProvider();
    }

    protected IServiceProvider ServiceProvider { get; }
}