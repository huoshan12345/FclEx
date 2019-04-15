using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FclEx.Consumers
{
    public interface IConsumer<in T> : IDisposable
    {
        bool IsComplete { get; }
        int Count { get; }
        ILogger Logger { get; set; }
        Counter Counter { get; }
        Task Start(bool clear = false);
        void Add(T item);
        void CompleteAdding();
        void Stop();
    }
}
