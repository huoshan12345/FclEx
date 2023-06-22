using System.Threading.Tasks;

namespace FclEx.Serilog.Sinks.Logstash.Inputs;

internal interface ILogstashInput
{
    /// <summary>
    /// Send items by input
    /// </summary>
    /// <param name="list">Items to be sent</param>
    /// <returns>Failed Items</returns>
    Task SendAsync(IReadOnlyList<string> list);
}