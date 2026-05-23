using Microsoft.Extensions.Configuration;

namespace FclEx.NewRelic;

public class NewRelicConfig
{
    public string ApiKey { get; set; } = "";
    public string LicenseKey { get; set; } = "";
    public string AccountId { get; set; } = "";
}

public class NewRelicTestsFixture : CoreTestsFixture
{
    public static NewRelicConfig NewRelicConfig { get; } = Config.GetSection("NewRelic").Get<NewRelicConfig>()!;

    public static readonly IServiceProvider Services = new ServiceCollection()
        .AddSingleton(NewRelicConfig)
        .AddNewRelicClient(NewRelicConfig.ApiKey)
        .BuildServiceProvider();

    public static NewRelicClient NewRelicApi => Services.GetRequiredService<NewRelicClient>();
}
