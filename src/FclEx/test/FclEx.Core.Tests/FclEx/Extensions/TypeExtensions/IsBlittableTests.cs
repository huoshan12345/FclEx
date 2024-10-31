using FclEx.TestModels;

namespace FclEx.Extensions.TypeExtensions;

public class IsBlittableTests(ITestOutputHelper output)
{
    /// <summary>
    /// Error from <see cref="GCHandle.Alloc(object, GCHandleType)"/> with <see cref="GCHandleType.Pinned"/>
    /// </summary>
    private const string NonPinnableError = "Object contains non-primitive or non-blittable data.";

    // NOTE: single-element ValueTuple of blittable type is also blittable
    // ValueTuple types that contains more than 1 element are marked as LayoutKind.Auto, so they are not blittable.
    public static readonly IEnumerable<object[]> BlittableTestCases = Types.BlittableTypes
        .Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(BlittableTestCases))]
    public void BlittableType_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.True(result, ex?.ToString());
    }

    [Theory]
    [MemberData(nameof(BlittableTestCases))]
    public void ArrayOfBlittableType_Test(Type type)
    {
        var result = type.MakeArrayType().IsBlittable(out var ex);
        Assert.True(result, ex?.ToString());
    }

    [Theory]
    [InlineData(typeof(BlittableStruct))]
    [InlineData(typeof(BlittableClass))]
    public void NonAutoLayout_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.True(result, ex?.ToString());
    }

    [Theory]
    [InlineData(typeof(char))]
    [InlineData(typeof(bool))]
    public void NonBlittableType_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.False(result);
        Assert.IsType<ArgumentException>(ex);
        output.WriteLine(ex.ToString());
        Assert.Contains("is not blittable.", ex.Message);
    }

    [Theory]
    [InlineData(typeof(char[]))]
    [InlineData(typeof(bool[]))]
    public void ArrayOfNonBlittableType_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.False(result);
        Assert.IsType<ArgumentException>(ex);
        output.WriteLine(ex.ToString());
        Assert.Contains("is not blittable.", ex.Message);
    }

    [Theory]
    [InlineData(typeof(TestStruct))]
    [InlineData(typeof(MarshalableStruct))]
    [InlineData(typeof(MarshalableClass))]
    public void NonAutoLayout_ContainsNonBlittable_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.False(result);
        Assert.IsType<ArgumentException>(ex);
        output.WriteLine(ex.ToString());
        Assert.Contains("is not blittable", ex.Message);
    }

    [Theory]
    [InlineData(typeof(TestClass))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(DateTimeOffset))]
    public void AutoLayout_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.False(result);
        Assert.IsType<ArgumentException>(ex);
        output.WriteLine(ex.ToString());
        Assert.Contains("is not blittable because it is laid out automatically.", ex.Message);
    }

    [Theory]
    [InlineData(typeof(ValueTuple<int>))]
    [InlineData(typeof(ValueTuple<int, long>))]
    public void GenericType_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.False(result);
        Assert.IsType<ArgumentException>(ex);
        Assert.Contains("is not blittable because it is generic.", ex.Message);
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(Func<int>))]
    [InlineData(typeof(Action))]
    public void StringOrDelegate_Test(Type type)
    {
        var result = type.IsBlittable(out var ex);
        Assert.False(result);
        Assert.IsType<ArgumentException>(ex);
        output.WriteLine(ex.ToString());
        Assert.Contains("is not blittable.", ex.Message);
    }

    public static readonly IEnumerable<object?[]> PinnableTestCases = new object?[]
    {
        null,
        "a",
        (int?)1,
        new[] { true, false },
        new[] { 'a', 'a' },
        new[] { 1, 2 },
        new ValueTuple<int>(1),
        new BlittableClass(),
        new BlittableStruct(),
    }.Select(m => new[] { m });

    [Theory]
    [MemberData(nameof(PinnableTestCases))]
    public void GCHandle_Pinned_Pinnable_Test(object? value)
    {
        GCHandle.Alloc(value, GCHandleType.Pinned).Free();
    }

    private static readonly IntCondition GreaterThan6 = new(ComparisonResult.GreaterThan, 6);
    public static readonly IEnumerable<object?[]> AccordingToClrVersion = new (object?, IntCondition)[]
    {
       ('a', GreaterThan6),
       (true, GreaterThan6),
       (new int?[] { null }, GreaterThan6),
       (new int?[] { 1, null }, GreaterThan6),
       (new int?[] { 1, 2 }, GreaterThan6),
       (new Tuple<int, int>(1, 1), GreaterThan6),
       (new ValueTuple<int, int>(1, 1), GreaterThan6),
    }.Select(m => new object?[] { m.Item1, m.Item2 });

    [Theory]
    [MemberData(nameof(AccordingToClrVersion))]
    public void GCHandle_Pinned_AccordingToClrVersion_Test(object? value, IntCondition versionCondition)
    {
        if (versionCondition.Compare(Environment.Version.Major))
        {
            Check();
        }
        else
        {
            Assert.Throws<ArgumentException>(Check);
        }
        return;

        void Check()
        {
            GCHandle.Alloc(value, GCHandleType.Pinned).Free();
        }
    }


    public static readonly IEnumerable<object?[]> NonPinnable = new object?[]
    {
        new MarshalableStruct(),
        new MarshalableClass(),
        new[] { "a" }
    }.Select(m => new[] { m });

    [Theory]
    [MemberData(nameof(NonPinnable))]
    public void GCHandle_Pinned_NonPinnable_Test(object? value)
    {
        Assert.Throws<ArgumentException>(() => GCHandle.Alloc(value, GCHandleType.Pinned).Free());
    }
}

public enum ComparisonResult
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
}

public record ComparableCondition<T>(ComparisonResult Comparison, T Value) where T : IComparable<T>
{
    /// <summary>
    /// Compare <paramref name="left"/> and <see cref="Value"/> then check if the result matches <see cref="Comparison"/>.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public bool Compare(T left)
    {
        var result = left.CompareTo(Value);
        return Comparison switch
        {
            ComparisonResult.Equal => result == 0,
            ComparisonResult.NotEqual => result != 0,
            ComparisonResult.GreaterThan => result > 0,
            ComparisonResult.GreaterThanOrEqual => result >= 0,
            ComparisonResult.LessThan => result < 0,
            ComparisonResult.LessThanOrEqual => result <= 0,
            _ => throw new ArgumentOutOfRangeException(nameof(Comparison), Comparison, null),
        };
    }

    public override string ToString()
    {
        var op = Comparison switch
        {
            ComparisonResult.Equal => "=",
            ComparisonResult.NotEqual => "!=",
            ComparisonResult.GreaterThan => ">",
            ComparisonResult.GreaterThanOrEqual => ">=",
            ComparisonResult.LessThan => "<",
            ComparisonResult.LessThanOrEqual => "<=",
            _ => throw new ArgumentOutOfRangeException(nameof(Comparison), Comparison, null),
        };
        return $"{op} {Value}";
    }
}

public record IntCondition(ComparisonResult Comparison, int Value)
    : ComparableCondition<int>(Comparison, Value);