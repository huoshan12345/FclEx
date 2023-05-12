using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

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