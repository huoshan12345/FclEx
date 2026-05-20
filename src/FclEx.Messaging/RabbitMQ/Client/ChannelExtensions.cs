namespace RabbitMQ.Client;

public static class ChannelExtensions
{
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