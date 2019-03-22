using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using FclEx.Http;
using FclEx.Http.Core;
using FclEx.Http.Services;
using FclEx.Utils;

namespace FclEx.Benchmark
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            //BenchmarkRunner.Run<HttpServiceTest>();
            //Console.Read();
            //await ExcuteResult.ExcuteAsync(async () => await HttpServiceRawTest.RawTest(500).DonotCapture())
            //    .Error(e => Console.WriteLine(e));

            // BenchmarkRunner.Run<SortTest>();

            var test = new SortTest();

            for (int i = 0; i < 100; i++)
            {
                test.Comparison_Prop();
            }
        }
    }
}
