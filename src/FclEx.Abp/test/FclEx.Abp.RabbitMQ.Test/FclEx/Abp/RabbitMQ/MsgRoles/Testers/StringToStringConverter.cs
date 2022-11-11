using Volo.Abp.DependencyInjection;

namespace FclEx.Abp.RabbitMQ.MsgRoles.Testers
{
    public sealed class StringToStringAsyncMsgConverter : IAsyncMsgConverter<string, string>, ISingletonDependency
    {
        public static StringToStringAsyncMsgConverter Instance { get; } = new();
        public Task<string> Convert(string source) => Task.FromResult(source);
    }
}