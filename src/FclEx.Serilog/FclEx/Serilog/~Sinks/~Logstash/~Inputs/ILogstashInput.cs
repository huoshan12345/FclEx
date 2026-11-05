namespace FclEx.Serilog;

internal interface ILogstashInput
{
    /// <summary>
    /// Sends the serialized items to the configured transport, appending a newline to each item.
    /// </summary>
    /// <param name="list">Items to be sent</param>
    /// <returns>An operation that completes when sending succeeds; transport failures propagate to the caller.</returns>
    Task SendAsync(IEnumerable<string> list);
}
