using System.Runtime.InteropServices;

namespace FclEx.Benchmarks;

/*
| Method                | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| BitConverter_GetBytes | 1.964 ns | 0.0884 ns | 0.1524 ns |  1.01 |    0.11 | 0.0019 |      32 B |        1.00 |
| ExplicitLayoutStruct  | 3.139 ns | 0.0582 ns | 0.0486 ns |  1.61 |    0.12 | 0.0019 |      32 B |        1.00 |
| Bitwise               | 1.980 ns | 0.0765 ns | 0.0752 ns |  1.01 |    0.08 | 0.0019 |      32 B |        1.00 |
| Pointer               | 3.008 ns | 0.1063 ns | 0.1044 ns |  1.54 |    0.13 | 0.0019 |      32 B |        1.00 |
*/
[MemoryDiagnoser]
// [MinInvokeCount(10)]
public class IntToByteArrayBenchmark
{
    private const int Value = 1234567890;
    private static readonly byte[] ValueBytes = BitConverter.GetBytes(Value);

    private static void AssertEqual(byte[] actualBytes)
    {
        if (ValueBytes.Length != actualBytes.Length)
            throw new ArgumentException($"The expected length is {ValueBytes.Length}, but actual is {actualBytes.Length}", nameof(actualBytes));

        for (var i = 0; i < ValueBytes.Length; i++)
        {
            var expected = ValueBytes[i];
            var actual = actualBytes[i];
            if (expected != actual)
                throw new ArgumentException($"The expected value at {i} is {expected}, but actual is {actual}", nameof(actualBytes));
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        var results = new[]
        {
            BitConverter_GetBytes(),
            ExplicitLayoutStruct(),
            Bitwise(),
            Pointer(),
        };

        foreach (var result in results)
        {
            AssertEqual(result);
        }
    }

    [Benchmark(Baseline = true)]
    public byte[] BitConverter_GetBytes()
    {
        return BitConverter.GetBytes(Value);
    }

    [Benchmark]
    public byte[] ExplicitLayoutStruct()
    {
        return GetBytes(Value);

        static byte[] GetBytes(int value)
        {
            var intByte = new IntByte { Value = value };
            return [intByte.Byte0, intByte.Byte1, intByte.Byte2, intByte.Byte3];
        }
    }

    [Benchmark]
    public byte[] Bitwise()
    {
        return GetBytes(Value);

        static byte[] GetBytes(int value)
        {
            var buffer = new byte[4];
            unchecked
            {
                buffer[0] = (byte)(value);
                buffer[1] = (byte)(value >> 8);
                buffer[2] = (byte)(value >> 16);
                buffer[3] = (byte)(value >> 24);
            }
            return buffer;
        }
    }


    [Benchmark]
    public byte[] Pointer()
    {
        return GetBytes(Value);

        static unsafe byte[] GetBytes(int value)
        {
            var buffer = new byte[4];
            fixed (byte* p = buffer)
            {
                *(int*)p = value;
            }
            return buffer;
        }
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct IntByte
{
    [FieldOffset(0)]
    public int Value;
    [FieldOffset(0)]
    public byte Byte0;
    [FieldOffset(1)]
    public byte Byte1;
    [FieldOffset(2)]
    public byte Byte2;
    [FieldOffset(3)]
    public byte Byte3;
}