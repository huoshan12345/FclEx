namespace FclEx.Serilog;

internal interface ILogstashInput
{
    /// <summary>
    /// Send items by input
    /// </summary>
    /// <param name="list">Items to be sent</param>
    /// <returns>Failed Items</returns>
    Task SendAsync(IEnumerable<string> list);
}