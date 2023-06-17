using FclEx.Abp.RabbitMQ.Serializers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Volo.Abp.DependencyInjection;

namespace FclEx.Abp.RabbitMQ.MsgRoles.Testers
{
    public sealed class DiTestRouter : AsyncMsgRouter<string, string>, ITransientDependency
    {
        protected override bool AutomaticRecoveryEnabled { get; } = false;

        public DiTestRouter(IAsyncMsgConverter<string, string> converter, 
            IMemoryBytesSerializer? serializer = null,
            ILoggerFactory? loggerFactory = null)
            : base(converter, serializer, loggerFactory)
        {
        }

        protected override string GetRoutingKey(IBasicProperties props, string output)
        {
            return output;
        }

        protected override void DisposeInternal()
        {
            if (Settings != null)
            {
                Channel.QueueDeleteNoWait(Settings.Queue.Name);
                Channel.ExchangeDeleteNoWait(Settings.Exchange.Name);
                Channel.ExchangeDeleteNoWait(Settings.TargetExchange.Name);
            }
            base.DisposeInternal();
        }
    }
}
