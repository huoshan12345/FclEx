using FclEx.TestModels;

// ReSharper disable ConvertToConstant.Local

namespace System.Collections.Generic;

public class ObjectMemoryEqualityComparerTests
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

    private static readonly MethodInfo _equals = typeof(ObjectMemoryEqualityComparerTests).GetRequiredMethod(nameof(SameValue_Equals));

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
        AssertObjectMemoryEqual(x, x);
    }

    [Fact]
    public void Record_Equals()
    {
        var random = new Random(0);
        var x = random.Next<CommonRecord>();
        var y = x with { }; // clone
        AssertObjectMemoryEqual(x, y);
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
            AssertObjectMemoryEqual(x, y);
        }
    }

    [Fact]
    public void Object_Equals()
    {
        var x = new object();
        var y = new object();
        AssertObjectMemoryEqual(x, y);
    }

    [Fact]
    public void StringLiteral_Equals()
    {
        var x = nameof(StringLiteral_Equals);
        var y = nameof(StringLiteral_Equals);
        AssertObjectMemoryEqual(x, y);
    }

    [Fact]
    public void StringObject_Equals()
    {
        var x = new string(nameof(StringObject_Equals).ToArray());
        var y = new string(nameof(StringObject_Equals).ToArray());
        AssertObjectMemoryEqual(x, y);
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameHash_ForSameObjectMemory()
    {
        var random = new Random(0);
        var x = random.Next<CommonRecordStruct>();
        var y = x;

        var comparer = ObjectMemoryEqualityComparer<CommonRecordStruct>.Instance;

        Assert.Equal(comparer.GetHashCode(x), comparer.GetHashCode(y));
    }

    private static void AssertObjectMemoryEqual<T>(T expected, T actual)
    {
        if (ObjectMemoryEqualityComparer<T>.Instance.Equals(expected, actual))
            return;

        var method = ObjectMemoryEqualityComparer<T>.GetBytes;
        var expectedBytes = method(expected);
        var actualBytes = method(actual);
        Assert.Equal(expectedBytes, actualBytes);
        Assert.Fail("ObjectMemoryEqualityComparer reported inequality, but the byte sequences are identical.");
    }
}
