using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Abp.RabbitMQ.Serializers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Abp.RabbitMQ.MsgRoles;

public class CommonPublisher<TMsg> : Publisher<TMsg>
{
    public CommonPublisher(ILoggerFactory? logger = null, IMemoryBytesSerializer? serializer = null)
        : base(serializer, logger)
    {
    }
}