namespace FclEx;

internal class Program
{
    private static void Main(string[] args)
    {
        //TestRandom();
        //return;

        var config = DefaultConfig.Instance.WithArtifactsPath(Path.Combine(AppContext.BaseDirectory, "BenchmarkDotNet.Artifacts"));
        config.StopOnFirstError(true);

        // BenchmarkSwitcher.FromTypes([typeof(SizeOfBenchmark<>)]).Run([.. args, "-f", "*"], config);
        // new IntToByteArrayBenchmark().ExplicitLayoutStruct();
        BenchmarkRunner.Run<HeapBenchmarks>(config);
    }

    private static void TestRandom()
    {
        var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };
        Parallel.For(0, 10, options, _ =>  // run in parallel
        {
            var numbers = new int[10_000];
            for (var i = 0; i < numbers.Length; ++i)
            {
                numbers[i] = ThreadSafeRandom.Instance.Next(); // 👈 Call the static helper instead
            }

            var numZeros = numbers.Count(x => x == 0); // how many issues were there?
            Console.WriteLine($"Received {numZeros} zeroes"); // always 0 issues
        });
    }
}