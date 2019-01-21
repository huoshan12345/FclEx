using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FclEx.Consumers;
using FclEx.Test.TypeCasters;
using MoreLinq.Extensions;
using Xunit;

namespace FclEx.Test.Consumers
{
    public class BatchConsumerTests
    {
        [Fact]
        public async Task Test()
        {
            var consumer = new BatchConsumer<int>(10, 1);
            consumer.OnConsume += (sender, ints) => throw new Exception();
            consumer.OnException += (sender, args) => args.ForEach(m =>
            {
                Assert.NotNull(m.Exception);
                Assert.IsAssignableFrom<Exception>(m.Exception);
            });
            consumer.OnDiscard += (sender, args) => args.ForEach(m =>
            {
                Assert.NotNull(m.Exception);
                Assert.IsAssignableFrom<Exception>(m.Exception);
            });
            var task = consumer.Start();
            Enumerable.Range(1, 10).ForEach(m => consumer.Add(m));
            consumer.CompleteAdding();
            await task;
        }
    }
}
