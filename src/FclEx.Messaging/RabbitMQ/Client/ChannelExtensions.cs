using System.Reflection;
using System.Threading;

namespace RabbitMQ.Client;

public static class ChannelExtensions
{
    /// <summary>Asynchronously declare an exchange.</summary>
    /// <param name="channel"></param>
    /// <param name="exchange">The name of the exchange.</param>
    /// <param name="type">The type of the exchange.</param>
    /// <param name="durable">Should this exchange survive a broker restart?</param>
    /// <param name="autoDelete">Should this exchange be auto-deleted?</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="isDelayed"></param>
    /// <param name="passive">Optional; Set to <code>true</code> to passively declare the exchange (i.e. check for its existence)</param>
    /// <param name="noWait">If set to <c>true</c>, do not require a response from the server.</param>
    /// <param name="cancellationToken">CancellationToken for this operation.</param>
    /// <remarks>The exchange is declared non-internal.</remarks>
    public static Task ExchangeDeclareAsync(this IChannel channel,
        string exchange,
        string type,
        bool durable,
        bool autoDelete,
        IDictionary<string, object?>? arguments,
        bool isDelayed,
        bool passive = false,
        bool noWait = false,
        CancellationToken cancellationToken = default)
    {
        arguments ??= new Dictionary<string, object?>();
        if (isDelayed)
        {
            arguments[key: RabbitMQHeaderNames.DelayType] = type;
            return channel.ExchangeDeclareAsync(
                exchange: exchange,
                type: RabbitMQConstants.DelayExchange,
                durable: durable,
                autoDelete: autoDelete,
                arguments: arguments,
                passive: passive,
                noWait: noWait,
                cancellationToken: cancellationToken);
        }
        else
        {
            return channel.ExchangeDeclareAsync(
                exchange: exchange,
                type: type,
                durable: durable,
                autoDelete: autoDelete,
                arguments: arguments,
                passive: passive,
                noWait: noWait,
                cancellationToken: cancellationToken);
        }
    }

    public static async Task<AsyncDisposableValue<IChannel>> CreateAutoCloseableChannelAsync(this IConnection con)
    {
        var channel = await con.CreateChannelAsync();
        return new AsyncDisposableValue<IChannel>(channel, async m =>
        {
            await m.CloseAsync();
            await m.DisposeAsync();
        });
    }
}