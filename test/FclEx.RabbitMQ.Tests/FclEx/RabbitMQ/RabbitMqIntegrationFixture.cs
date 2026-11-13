using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace FclEx.RabbitMQ;

public static class RabbitMqIntegrationFixture
{
    public const string SkipReason = "Configure RabbitMQ test connection settings before running broker integration tests.";
    private static IConfigurationRoot Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.local.json", optional: true)
        .AddEnvironmentVariables("FCLEX_RABBITMQ_TEST_")
        .Build();

    public static bool IsConfigured => !string.IsNullOrWhiteSpace(Configuration["RabbitMQ:HostName"]);

    internal static ConnectionFactory Factory() => new()
    {
        HostName = Configuration["RabbitMQ:HostName"]!,
        Port = int.Parse(Configuration["RabbitMQ:Port"] ?? "5672"),
        VirtualHost = Configuration["RabbitMQ:VirtualHost"] ?? "/",
        UserName = Configuration["RabbitMQ:UserName"] ?? "guest",
        Password = Configuration["RabbitMQ:Password"] ?? "guest",
        AutomaticRecoveryEnabled = false,
    };
}

internal sealed class BrokerScope : IAsyncDisposable
{
    private readonly List<string> _queues = [];
    private readonly List<string> _exchanges = [];
    private readonly string _prefix = "fclex-" + Guid.NewGuid().ToString("N");
    private readonly IConnection _adminConnection;
    internal RabbitMqClient Client { get; }
    internal IChannel Raw { get; }

    private BrokerScope(RabbitMqClient client, IConnection connection, IChannel channel)
    {
        Client = client;
        _adminConnection = connection;
        Raw = channel;
    }

    internal static async Task<BrokerScope> CreateAsync()
    {
        var factory = RabbitMqIntegrationFixture.Factory();
        var client = await RabbitMqClient.ConnectAsync(factory);
        var admin = await factory.CreateConnectionAsync();
        return new(client, admin, await admin.CreateChannelAsync());
    }

    internal RabbitMqExchangeOptions Exchange(string suffix)
    {
        var name = _prefix + "." + suffix;
        _exchanges.Add(name);
        return new() { Name = name };
    }

    internal RabbitMqConsumerOptions Input(string suffix, RabbitMqExchangeOptions? exchange = null, RabbitMqRetryOptions? retry = null)
    {
        var name = _prefix + "." + suffix;
        _queues.AddRange([name, name + ".dead"]);
        _exchanges.Add(name + ".dlx");
        return new()
        {
            Queue = new() { Name = name, Retry = retry ?? new() { MinDelay = TimeSpan.FromMilliseconds(100), MaxDelay = TimeSpan.FromMilliseconds(100), DeliveryLimit = 3 } },
            Bindings = exchange is null ? [] : [new() { Exchange = exchange, RoutingKey = "#" }],
            ShutdownTimeout = TimeSpan.FromSeconds(5),
        };
    }

    internal async Task<BasicGetResult> GetAsync(string queue, TimeSpan? timeout = null)
    {
        var watch = Stopwatch.StartNew();
        do
        {
            var result = await Raw.BasicGetAsync(queue, autoAck: true);
            if (result is not null) return result;
            await Task.Delay(20);
        } while (watch.Elapsed < (timeout ?? TimeSpan.FromSeconds(10)));
        throw new TimeoutException($"No message became available in '{queue}'.");
    }

    internal async Task AssertEmptyForAsync(string queue, TimeSpan duration)
    {
        var watch = Stopwatch.StartNew();
        do
        {
            Assert.Null(await Raw.BasicGetAsync(queue, autoAck: true));
            await Task.Delay(20);
        } while (watch.Elapsed < duration);
    }

    internal static string Text(BasicGetResult delivery) => Encoding.UTF8.GetString(delivery.Body.ToArray());

    public async ValueTask DisposeAsync()
    {
        try { await Client.DisposeAsync(); }
        finally
        {
            try
            {
                await using var cleanup = await _adminConnection.CreateChannelAsync();
                foreach (var queue in _queues.Distinct()) await cleanup.QueueDeleteAsync(queue);
                foreach (var exchange in _exchanges.Distinct()) await cleanup.ExchangeDeleteAsync(exchange);
            }
            finally
            {
                await Raw.DisposeAsync();
                await _adminConnection.DisposeAsync();
            }
        }
    }
}
