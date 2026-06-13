using FclEx.TestModels;

namespace FclEx.Extensions;

public class RandomExtensionsTests
{
    private sealed class QueueBytesRandom(params ulong[] values) : Random
    {
        private readonly Queue<ulong> _values = new(values);

        public override void NextBytes(byte[] buffer)
        {
            WriteNextValue(buffer);
        }

#if NET6_0_OR_GREATER
        public override void NextBytes(Span<byte> buffer)
        {
            WriteNextValue(buffer);
        }
#endif

        private void WriteNextValue(Span<byte> buffer)
        {
            var value = _values.Dequeue();
            var bytes = BitConverter.GetBytes(value);
            bytes.AsSpan(0, buffer.Length).CopyTo(buffer);
        }
    }

    private sealed class Node
    {
        public Node? Next = null;
    }

    [Fact]
    public void NextMarshalable_Struct_Test()
    {
        var random = new Random(0);
        for (var i = 0; i < 10; i++)
        {
            var x = random.NextMarshalable<MarshalableStruct>();
            Assert.Equal(4, x.Array?.Length);
        }
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void NextBoolean_ShouldRespectProbabilityBoundaries(double probability, bool expected)
    {
        var random = new Random(0);

        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(expected, random.NextBoolean(probability));
        }
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    public void NextBoolean_ShouldThrow_WhenProbabilityIsOutOfRange(double probability)
    {
        var random = new Random(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextBoolean(probability));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void NextBooleanPercent_ShouldRespectPercentageBoundaries(double percentage, bool expected)
    {
        var random = new Random(0);

        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(expected, random.NextBooleanPercent(percentage));
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(double.NaN)]
    public void NextBooleanPercent_ShouldThrow_WhenPercentageIsOutOfRange(double percentage)
    {
        var random = new Random(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextBooleanPercent(percentage));
    }

    [Fact]
    public void NextUInt64_ShouldUseRejectionSampling()
    {
        var random = new QueueBytesRandom(ulong.MaxValue, 7);

        var value = random.NextUInt64(10, 20);

        Assert.Equal<ulong>(17, value);
    }

    [Fact]
    public void NextUInt64_ShouldReturnMin_WhenRangeIsEmpty()
    {
        var random = new Random(0);

        Assert.Equal<ulong>(42, random.NextUInt64(42, 42));
    }

    [Fact]
    public void NextUInt64_ShouldThrow_WhenMaxIsLessThanMin()
    {
        var random = new Random(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextUInt64(2, 1));
    }

    [Fact]
    public void NextDouble_ShouldReturnMin_WhenRangeIsEmpty()
    {
        var random = new Random(0);

        Assert.Equal(1.25, random.NextDouble(1.25, 1.25));
    }

    [Fact]
    public void NextDecimal_ShouldThrow_WhenMaxIsLessThanMin()
    {
        var random = new Random(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextDecimal(2, 1));
    }

    [Fact]
    public void NextDateTime_ShouldPreserveMinKind()
    {
        var random = new Random(0);
        var min = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var max = min.AddDays(1);

        var value = random.NextDateTime(min, max);

        Assert.Equal(DateTimeKind.Utc, value.Kind);
        Assert.InRange(value, min, max.AddTicks(-1));
    }

    [Fact]
    public void NextDateTimeOffset_ShouldPreserveMinOffset()
    {
        var random = new Random(0);
        var min = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.FromHours(8));
        var max = min.AddDays(1);

        var value = random.NextDateTimeOffset(min, max);

        Assert.Equal(min.Offset, value.Offset);
        Assert.True(value >= min);
        Assert.True(value < max);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void NextDateOnly_ShouldReturnMin_WhenRangeIsEmpty()
    {
        var random = new Random(0);
        var date = new DateOnly(2024, 1, 1);

        Assert.Equal(date, random.NextDateOnly(date, date));
    }

    [Fact]
    public void NextTimeOnly_ShouldThrow_WhenMaxIsLessThanMin()
    {
        var random = new Random(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextTimeOnly(new TimeOnly(2, 0), new TimeOnly(1, 0)));
    }
#endif

    [Fact]
    public void Next_ShouldUseMarshalAsSizeConst_ForFixedArrayFields()
    {
        var random = new Random(0);

        var value = random.Next<MarshalableStruct>();

        Assert.Equal(4, value.Array?.Length);
    }

    [Fact]
    public void Next_ShouldStopRecursiveReferenceChains()
    {
        var random = new Random(0);

        var node = random.Next<Node>();

        var count = 0;
        for (var current = node; current is not null; current = current.Next)
        {
            count++;
            Assert.True(count <= 10);
        }

        Assert.Equal(10, count);
    }

    [Fact]
    public void Next_ShouldThrow_WhenTypeIsNull()
    {
        var random = new Random(0);

        Assert.Throws<ArgumentNullException>(() => random.Next(null!));
    }

    [Fact]
    public void Sample_ShouldThrow_WhenSourceIsEmpty()
    {
        var random = new Random(0);

        Assert.Throws<ArgumentException>(() => random.Sample(Array.Empty<int>().AsEnumerable()));
    }

    [Fact]
    public void Sample_ShouldReturnTheOnlyItem()
    {
        var random = new Random(0);

        Assert.Equal(42, random.Sample([42]));
    }

    [Fact]
    public void Shuffle_ShouldKeepTheSameItems()
    {
        var random = new Random(0);
        var list = Enumerable.Range(0, 10).ToList();

        random.Shuffle(list);

        Assert.Equal(Enumerable.Range(0, 10), list.OrderBy(x => x));
    }
}
