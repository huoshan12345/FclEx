using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Abp.RabbitMQ.Serializers;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FclEx.Abp.RabbitMQ.MsgRoles
{
    public class CommonAsyncConsumer<TMessage> : AsyncConsumer<TMessage>
    {
        protected readonly ConsumeHandler _handler;

        public CommonAsyncConsumer(ConsumeHandler handler, IMemoryBytesSerializer? serializer = null,
            ILoggerFactory? logger = null)
            : base(serializer, logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        protected override Task<OperateResult> ConsumeInternalAsync(BasicDeliverEventArgs args, TMessage message)
        {
            return _handler(args, message);
        }
    }
}
