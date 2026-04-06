using FclEx.TestModels;

// ReSharper disable ConvertToConstant.Local

namespace System.Collections.Generic;

public class BytewiseEqualityComparerTests
{
    public static readonly IEnumerable<Type> TestTypes = Types.BlittableTypes.Concat([
        typeof(ValueTuple<int>), // non-blittable
        typeof(DateTime), // non-blittable
        typeof(DateTimeOffset), // non-blittable
        typeof(ValueTuple<int, long, DateTimeOffset, DateTime>), // non-blittable
        typeof(string),
        typeof(CommonStruct),
        typeof(CommonRecord),
        typeof(CommonRecordStruct),
        typeof(MarshalableClass),
        typeof(MarshalableStruct)]);

    public static readonly TheoryData<Type> TypeCases = TestTypes.ToTheoryData();

    private static readonly MethodInfo _equals = typeof(BytewiseEqualityComparerTests).GetRequiredMethod(nameof(SameValue_Equals));

    [Theory]
    [MemberData(nameof(TypeCases))]
    public void Equals_Test(Type type)
    {
        _equals.MakeGenericMethod(type).Invoke(this, null);
    }

    private void SameValue_Equals<T>()
    {
        var random = new Random(0);
        var x = random.Next<T>();
        AssertBytewiseEqual(x, x);
    }

    [Fact]
    public void Record_Equals()
    {
        var random = new Random(0);
        var x = random.Next<CommonRecord>();
        var y = x with { }; // clone
        AssertBytewiseEqual(x, y);
    }

    [Fact]
    public void Record_Lock_Equals()
    {
        var random = new Random(0);
        var x = random.Next<CommonRecord>();
        var y = x with { }; // clone
        lock (x)
        {
            // lock status stores in object header.
            // so the object headers of x and y are different.
            AssertBytewiseEqual(x, y);
        }
    }

    [Fact]
    public void Object_Equals()
    {
        var x = new object();
        var y = new object();
        AssertBytewiseEqual(x, y);
    }

    [Fact]
    public void StringLiteral_Equals()
    {
        var x = nameof(StringLiteral_Equals);
        var y = nameof(StringLiteral_Equals);
        AssertBytewiseEqual(x, y);
    }

    [Fact]
    public void StringObject_Equals()
    {
        var x = new string(nameof(StringObject_Equals).ToArray());
        var y = new string(nameof(StringObject_Equals).ToArray());
        AssertBytewiseEqual(x, y);
    }

    private static void AssertBytewiseEqual<T>(T expected, T actual)
    {
        if (BytewiseEqualityComparer<T>.Instance.Equals(expected, actual))
            return;

        var method = BytewiseEqualityComparer<T>.GetBytes;
        var expectedBytes = method(expected);
        var actualBytes = method(actual);
        Assert.Equal(expectedBytes, actualBytes);
        Assert.Fail("BytewiseEqualityComparer reported inequality, but the byte sequences are identical.");
    }
}