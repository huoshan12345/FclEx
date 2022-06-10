using BenchmarkDotNet.Running;
using FclEx.Benchmarks;

namespace FclEx
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            BenchmarkRunner.Run<InterfaceBaseInvocationBenchmark>();
        }
    }
}
