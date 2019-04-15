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
    public class BatchRetryConsumerTests
    {
        private class Tester
        {
            public int Number { get; set; }
        }

        private readonly ITestOutputHelper _output;

        public BatchRetryConsumerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Test()
        {
            const int retryTimes = 3;
            var numbers = Enumerable.Range(1, 10).Select(m => new Tester { Number = m }).ToArray();
            var consumer = new BatchRetryConsumer<Tester>(5, 1, retryTimes);
            consumer.OnConsume += (sender, list) =>
            {
                _output.WriteLine(nameof(consumer.OnConsume));
                if (list.Any(m => m.Number % 3 == 0))
                    throw new Exception();
                return Task.CompletedTask;
            };
            consumer.OnException += (sender, args) =>
            {
                _output.WriteLine(nameof(consumer.OnException));
                Assert.NotNull(args.Exception);
            };
            consumer.OnDiscard += (sender, args) =>
            {
                _output.WriteLine(nameof(consumer.OnDiscard));
                Assert.NotNull(args.Exception);
                Assert.Equal(retryTimes, args.ErrorTimes);
            };
            consumer.AddRange(numbers);
            var task = consumer.Start();
            consumer.CompleteAdding();
            await task;

            var errors = numbers.Count(m => m.Number % 3 == 0);
            Assert.Equal(0, consumer.Count);
            Assert.Equal(numbers.Length - errors, consumer.Counter.Consume);
            Assert.Equal(errors * retryTimes, consumer.Counter.Exception);
            Assert.Equal(errors, consumer.Counter.Discard);
        }
    }
}
