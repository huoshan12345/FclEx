using FclEx.Xunit;
using Microsoft.Extensions.Logging;

namespace FclEx.Web;

public abstract class WebTests
{
    protected readonly ITestOutputHelper _outputHelper;

    protected WebTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        ServiceProvider = new ServiceCollection()
            .AddUserClient<TestUserClient>()
            .AddLogging(m => m.AddXunitTest(outputHelper, false).SetMinimumLevel(LogLevel.Trace))
            .BuildServiceProvider();
    }

    protected IServiceProvider ServiceProvider { get; }
}