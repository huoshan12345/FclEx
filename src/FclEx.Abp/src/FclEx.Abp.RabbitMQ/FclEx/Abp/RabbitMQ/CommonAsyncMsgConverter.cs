using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Abp.RabbitMQ
{
    public class CommonAsyncMsgConverter<TSource, TDestination> : IAsyncMsgConverter<TSource, TDestination>
    {
        protected readonly Func<TSource, Task<TDestination>> _handler;

        public CommonAsyncMsgConverter(Func<TSource, Task<TDestination>> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task<TDestination> Convert(TSource source)
        {
            return _handler(source);
        }
    }
}
