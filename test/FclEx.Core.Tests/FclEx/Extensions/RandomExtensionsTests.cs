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

#if !NET6_0_OR_GREATER
    private sealed class MaximumIntRandom : Random
    {
        public override int Next(int maxValue) => maxValue - 1;
    }
#endif

    private sealed class Node
    {
        public Node? Next = null;
    }

    private sealed class TwoNodes
    {
        public Node? Left = null;
        public Node? Right = null;
    }

    private sealed class ConstructorOnly
    {
        public ConstructorOnly(int id, string name, Node node)
        {
            Id = id;
            Name = name;
            Node = node;
        }

        public int Id { get; }
        public string Name { get; }
        public Node Node { get; }
    }

    private sealed class ArrayHolder
    {
        public int[]? Numbers = null;
        public string[]? Names = null;
    }

    private interface ITestDataContract { }

    private enum SampleEnum
    {
        Zero,
        One,
        Two
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
    public void ParameterlessIntegerMethods_ShouldCoverTheCompleteBitPattern()
    {
        var random = new QueueBytesRandom(
            byte.MaxValue,
            byte.MaxValue,
            ushort.MaxValue,
            ushort.MaxValue,
            uint.MaxValue,
            ulong.MaxValue);

        Assert.Equal(-1, random.NextSByte());
        Assert.Equal(byte.MaxValue, random.NextByte());
        Assert.Equal(-1, random.NextInt16());
        Assert.Equal(ushort.MaxValue, random.NextUInt16());
        Assert.Equal(uint.MaxValue, random.NextUInt32());
        Assert.Equal(ulong.MaxValue, random.NextUInt64());
    }

#if !NET6_0_OR_GREATER
    [Fact]
    public void NextSingle_ShouldNeverRoundUpToOne()
    {
        var value = new MaximumIntRandom().NextSingle();

        Assert.True(value < 1f);
        Assert.Equal(1f - 1f / (1 << 24), value);
    }
#endif

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

    [Theory]
    [InlineData(typeof(bool))]
    [InlineData(typeof(byte))]
    [InlineData(typeof(sbyte))]
    [InlineData(typeof(short))]
    [InlineData(typeof(ushort))]
    [InlineData(typeof(int))]
    [InlineData(typeof(uint))]
    [InlineData(typeof(long))]
    [InlineData(typeof(ulong))]
    [InlineData(typeof(float))]
    [InlineData(typeof(double))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(char))]
    [InlineData(typeof(string))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(TimeSpan))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(IntPtr))]
    [InlineData(typeof(UIntPtr))]
    [InlineData(typeof(object))]
    [InlineData(typeof(DBNull))]
    public void Next_ShouldCreateCommonTypes(Type type)
    {
        var random = new Random(0);

        var value = random.Next(type);

        Assert.NotNull(value);
        Assert.IsAssignableFrom(type, value);
    }

    [Fact]
    public void NextGuid_ShouldBeDeterministicForASeededRandom()
    {
        var first = new Random(42).Next<Guid>();
        var second = new Random(42).Next<Guid>();

        Assert.Equal(first, second);
    }

    [Fact]
    public void Next_ShouldBeDeterministicForASeededObjectGraph()
    {
        var first = new Random(42).Next<ArrayHolder>();
        var second = new Random(42).Next<ArrayHolder>();

        Assert.Equal(first.Numbers, second.Numbers);
        Assert.Equal(first.Names, second.Names);
    }

    [Fact]
    public void Next_ShouldRejectAnInterfaceAtRuntime()
    {
        var random = new Random(0);

        Assert.Throws<ArgumentException>(() => random.Next<ITestDataContract>());
    }

#if NET6_0_OR_GREATER
    [Theory]
    [InlineData(typeof(DateOnly))]
    [InlineData(typeof(TimeOnly))]
    public void Next_ShouldCreateNet6Types(Type type)
    {
        var random = new Random(0);

        var value = random.Next(type);

        Assert.NotNull(value);
        Assert.IsAssignableFrom(type, value);
    }
#endif

    [Fact]
    public void Next_ShouldCreateNullableUnderlyingValue()
    {
        var random = new Random(0);

        var value = random.Next<int?>();

        Assert.True(value.HasValue);
    }

    [Fact]
    public void Next_ShouldCreateArrays()
    {
        var random = new Random(0);

        var values = random.Next<int[]>();

        Assert.NotNull(values);
        Assert.InRange(values.Length, 1, 4);
    }

    [Fact]
    public void Next_ShouldPopulateArrayFields()
    {
        var random = new Random(0);

        var value = random.Next<ArrayHolder>();

        Assert.NotNull(value.Numbers);
        Assert.NotNull(value.Names);
        Assert.InRange(value.Numbers.Length, 1, 4);
        Assert.InRange(value.Names.Length, 1, 4);
        Assert.All(value.Names, Assert.NotNull);
    }

    [Fact]
    public void Next_ShouldCreateTypesWithParameterizedConstructors()
    {
        var random = new Random(0);

        var value = random.Next<ConstructorOnly>();

        Assert.NotEqual(0, value.Id);
        Assert.NotNull(value.Name);
        Assert.NotNull(value.Node);
    }

    [Fact]
    public void Next_ShouldCreateRecordsWithPrimaryConstructor()
    {
        var random = new Random(0);

        var value = random.Next<CommonRecord>();

        Assert.NotEqual(0, value.Int);
        Assert.NotEqual(default, value.DateTime);
        Assert.NotNull(value.String);
    }

    [Fact]
    public void Next_ShouldCreateRecordStructs()
    {
        var random = new Random(0);

        var value = random.Next<CommonRecordStruct>();

        Assert.NotEqual(0, value.Int);
        Assert.NotEqual(default, value.DateTime);
        Assert.NotNull(value.String);
    }

    [Fact]
    public void Next_ShouldCreateEnumValues()
    {
        var random = new Random(0);

        var value = random.Next<SampleEnum>();

        Assert.True(Enum.IsDefined(typeof(SampleEnum), value));
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
    public void Next_ShouldTrackRecursiveDepthPerPath()
    {
        var random = new Random(0);

        var value = random.Next<TwoNodes>();

        Assert.Equal(10, CountNodes(value.Left));
        Assert.Equal(10, CountNodes(value.Right));
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

    private static int CountNodes(Node? node)
    {
        var count = 0;
        for (var current = node; current is not null; current = current.Next)
        {
            count++;
            Assert.True(count <= 10);
        }
        return count;
    }
}
