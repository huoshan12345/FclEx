using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FclEx.Consumers;
using FclEx.Helpers;
using FclEx.Test.TypeCasters;
using MoreLinq.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Test.Consumers
{
    public class BatchConsumerTests
    {
        private readonly ITestOutputHelper _output;

        public BatchConsumerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Test()
        {
            var consumer = new BatchConsumer<int>(5, 1);
            consumer.OnConsume += (sender, ints) =>
            {
                _output.WriteLine("OnConsume");
                throw new Exception();
            };
            consumer.OnException += (sender, args) => args.ForEach(m =>
            {
                _output.WriteLine("OnException");
                Assert.NotNull(m.Exception);
                Assert.IsAssignableFrom<Exception>(m.Exception);
            });
            consumer.OnDiscard += (sender, args) => args.ForEach(m =>
            {
                _output.WriteLine("OnDiscard");
                Assert.NotNull(m.Exception);
                Assert.IsAssignableFrom<Exception>(m.Exception);
            });
            var task = consumer.Start();
            await Enumerable.Range(1, 3).ForEachAsync(async m =>
            {
                consumer.Add(m);
                await TaskHelper.DelayMilli(100);
            });
            consumer.CompleteAdding();
            await task;
        }
    }
}
